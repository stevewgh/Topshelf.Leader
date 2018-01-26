using System;
using System.Threading;
using System.Threading.Tasks;
using Topshelf.ServiceConfigurators;

namespace Topshelf.Leader.HighAvailability
{
    public static class TopShelfLeaderExtension
    {
        public static ServiceConfigurator<T> WhenStartedAsLeader<T>(
            this ServiceConfigurator<T> configurator, CancellationToken serviceIsStoppingCancellationToken, LeaderConfiguration leaderConfiguration, Func<T,CancellationToken,Task> callback) where T : class
        {
            if (configurator == null)
            {
                throw new ArgumentNullException(nameof(configurator));
            }

            configurator.WhenStarted(async service =>
            {
                await WhenStarted(callback, service, leaderConfiguration, serviceIsStoppingCancellationToken);
            });
            return configurator;
        }

        private static async Task RenewHeartbeat(LeaderConfiguration leaderConfiguration, CancellationTokenSource noLongerTheLeaderTokenSource, CancellationToken serviceIsStoppingCancellationToken)
        {
            try
            {
                while (await leaderConfiguration.LeaderManager.AmITheLeader(leaderConfiguration.UniqueIdentifier) && !serviceIsStoppingCancellationToken.IsCancellationRequested)
                {
                    await TaskExtension.DelayWithoutTimeoutException(leaderConfiguration.HeartBeatEvery, serviceIsStoppingCancellationToken);
                }
            }
            finally
            {
                noLongerTheLeaderTokenSource.Cancel();
            }
        }

        private static async Task BlockUntilWeAreTheLeader(LeaderConfiguration leaderConfiguration, CancellationToken serviceIsStoppingCancellationToken)
        {
            while (!await leaderConfiguration.LeaderManager.AmITheLeader(leaderConfiguration.UniqueIdentifier) && !serviceIsStoppingCancellationToken.IsCancellationRequested)
            {
                await TaskExtension.DelayWithoutTimeoutException(leaderConfiguration.LeaderCheckEvery, serviceIsStoppingCancellationToken);
            }
        }

        private static async Task WhenStarted<T>(Func<T, CancellationToken, Task> callback, T service, LeaderConfiguration leaderConfiguration, CancellationToken serviceIsStoppingCancellationToken)
        {
            while (!serviceIsStoppingCancellationToken.IsCancellationRequested)
            {
                await BlockUntilWeAreTheLeader(leaderConfiguration, serviceIsStoppingCancellationToken);

                if (serviceIsStoppingCancellationToken.IsCancellationRequested)
                {
                    continue;
                }

                try
                {
                    var noLongerTheLeaderTokenSource = new CancellationTokenSource();
                    await Task.WhenAll(
                        RenewHeartbeat(leaderConfiguration, noLongerTheLeaderTokenSource, serviceIsStoppingCancellationToken),
                        callback(service, noLongerTheLeaderTokenSource.Token));
                }
                catch (TaskCanceledException)
                {
                }
            }
        }
    }
}