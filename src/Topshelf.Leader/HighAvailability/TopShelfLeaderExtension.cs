using System;
using System.Threading;
using System.Threading.Tasks;
using Topshelf.ServiceConfigurators;

namespace Topshelf.Leader.HighAvailability
{
    public static class TopShelfLeaderExtension
    {
        public static ServiceConfigurator<T> WhenStartedAsLeader<T>(
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

            configurator.WhenStarted(async service =>
            {
                var configurationBuilder = new LeaderConfigurationBuilder<T>();
                builder(configurationBuilder);
                var leaderConfiguration = configurationBuilder.Build();

                await WhenStarted(service, leaderConfiguration);
            });
            return configurator;
        }

        private static async Task RenewHeartbeat<T>(LeaderConfiguration<T> config, CancellationTokenSource noLongerTheLeader)
        {
            try
            {
                while (await config.LockManager.AcquireLock(config.UniqueIdentifier) && !config.ServiceIsStopping.IsCancellationRequested)
                {
                    await TaskExtension.DelayWithoutTimeoutException(config.LeaseUpdateEvery, config.ServiceIsStopping);
                }
            }
            finally
            {
                noLongerTheLeader.Cancel();
            }
        }

        private static async Task BlockUntilWeAreTheLeader<T>(LeaderConfiguration<T> config)
        {
            while (!await config.LockManager.AcquireLock(config.UniqueIdentifier) && !config.ServiceIsStopping.IsCancellationRequested)
            {
                await TaskExtension.DelayWithoutTimeoutException(config.LeaderCheckEvery, config.ServiceIsStopping);
            }
        }

        private static async Task WhenStarted<T>(T service, LeaderConfiguration<T> config)
        {
            while (!config.ServiceIsStopping.IsCancellationRequested)
            {
                await BlockUntilWeAreTheLeader(config);

                if (config.ServiceIsStopping.IsCancellationRequested)
                {
                    continue;
                }

                try
                {
                    var noLongerTheLeaderTokenSource = new CancellationTokenSource();
                    await Task.WhenAll(
                        RenewHeartbeat(config, noLongerTheLeaderTokenSource),
                        Task.Run(() => config.Startup(service, noLongerTheLeaderTokenSource.Token), config.ServiceIsStopping));
                }
                catch (TaskCanceledException)
                {
                }
            }
        }
    }
}