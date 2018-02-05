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

            var configurationBuilder = new LeaderConfigurationBuilder<T>();

            builder(configurationBuilder);

            if (!configurationBuilder.ServiceStoppingTokenIsSet)
            {
                var serviceStoppingTokenSource = new CancellationTokenSource();
                configurationBuilder.WhenStopping(serviceStoppingTokenSource);
            }

            var leaderConfiguration = configurationBuilder.Build();

            configurator.BeforeStoppingService(async () =>
            {
                try
                {
                    leaderConfiguration.ServiceIsStopping.Cancel();
                }
                catch (TaskCanceledException) { }

                await leaderConfiguration.LeaseManager.ReleaseLease(leaderConfiguration.NodeId);
            });

            configurator.WhenStarted(async service =>
            {
                try
                {
                    var worker = new Runner<T>(service, leaderConfiguration);
                    await worker.Start();
                }
                catch (TaskCanceledException)
                {
                    // TaskCanceledException bubbles up if the service stopping cancellation token is set
                    // so we swallow the exception so that the Topshelf OnException handler doesn't see it
                }
            });
        }
    }
}