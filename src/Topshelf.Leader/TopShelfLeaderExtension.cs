using System;
using System.Threading;
using System.Threading.Tasks;
using Topshelf.ServiceConfigurators;

namespace Topshelf.Leader
{
    public static class TopShelfLeaderExtension
    {
        public static void WhenStartedAsLeader<T>(
            this ServiceConfigurator<T> configurator, Action<LeaderConfigurationBuilder<T>> builder) where T : class
        {
            if (configurator == null)
            {
                throw new ArgumentNullException(nameof(configurator));
            }

            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var serviceStoppingTokenSource = new CancellationTokenSource();
            configurator.BeforeStoppingService(() =>
            {
                try
                {
                    serviceStoppingTokenSource.Cancel();
                }
                catch (TaskCanceledException)
                {
                }
            });

            configurator.WhenStarted(async service =>
            {
                var configurationBuilder = new LeaderConfigurationBuilder<T>();
                configurationBuilder.WhenServiceIsStopping(serviceStoppingTokenSource.Token);

                builder(configurationBuilder);
                var leaderConfiguration = configurationBuilder.Build();

                try
                {
                    await WhenStarted(service, leaderConfiguration);
                }
                catch (TaskCanceledException)
                {
                    // TaskCanceledException bubbles up if the service stopping cancellation token is set
                    // so we swallow the exception so that the Topshelf OnException handler doesn't see it
                }
            });
        }

        private static async Task BlockUntilWeAreTheLeader<T>(LeaderConfiguration<T> config)
        {
            while (!await config.LockManager.AcquireLock(config.NodeId, config.ServiceIsStopping))
            {
                await Task.Delay(config.LeaderCheckEvery, config.ServiceIsStopping);
            }

            if (!config.ServiceIsStopping.IsCancellationRequested)
            {
                config.WhenLeaderIsElected(true);
            }
        }

        private static async Task RenewLease<T>(LeaderConfiguration<T> config, CancellationTokenSource noLongerTheLeader)
        {
            try
            {
                while (await config.LockManager.RenewLock(config.NodeId, config.ServiceIsStopping))
                {
                    await Task.Delay(config.LeaseUpdateEvery, config.ServiceIsStopping);
                }
            }
            finally 
            {
                noLongerTheLeader.Cancel();
                config.WhenLeaderIsElected(false);
            }
        }

        private static async Task WhenStarted<T>(T service, LeaderConfiguration<T> config)
        {
            while (!config.ServiceIsStopping.IsCancellationRequested)
            {
                try
                {
                    await BlockUntilWeAreTheLeader(config);
                    var noLongerTheLeader = new CancellationTokenSource();
                    await Task.WhenAll(
                        RenewLease(config, noLongerTheLeader),
                        config.Startup(service, noLongerTheLeader.Token));
                }
                catch (TaskCanceledException)
                {
                }
            }
        }
    }
}