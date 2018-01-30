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
            configurator.BeforeStoppingService(() => serviceStoppingTokenSource.Cancel());

            configurator.WhenStarted(async service =>
            {
                var configurationBuilder = new LeaderConfigurationBuilder<T>();
                configurationBuilder.WhenServiceIsStopping(serviceStoppingTokenSource.Token);

                builder(configurationBuilder);
                var leaderConfiguration = configurationBuilder.Build();

                await WhenStarted(service, leaderConfiguration);
            });
        }

        private static async Task BlockUntilWeAreTheLeader<T>(LeaderConfiguration<T> config)
        {
            while (!await config.LockManager.AcquireLock(config.NodeId, config.ServiceIsStopping))
            {
                await Task.Delay(config.LeaderCheckEvery, config.ServiceIsStopping);
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
            }
        }

        private static async Task WhenStarted<T>(T service, LeaderConfiguration<T> config)
        {
            while (!config.ServiceIsStopping.IsCancellationRequested)
            {
                try
                {
                    await BlockUntilWeAreTheLeader(config);
                }
                catch (TaskCanceledException)
                {
                    continue;
                }

                var noLongerTheLeaderTokenSource = new CancellationTokenSource();
                try
                {
                    await Task.WhenAll(
                        RenewLease(config, noLongerTheLeaderTokenSource),
                        Task.Run(() => config.Startup(service, noLongerTheLeaderTokenSource.Token), config.ServiceIsStopping));
                }
                catch (TaskCanceledException)
                {
                }
            }
        }
    }
}