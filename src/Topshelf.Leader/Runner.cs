using System;
using System.Threading;
using System.Threading.Tasks;

namespace Topshelf.Leader
{
    public class Runner<T>
    {
        private readonly T service;
        private readonly LeaderConfiguration<T> config;

        public Runner(T service, LeaderConfiguration<T> config)
        {
            this.service = service;
            this.config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task Start()
        {
            while (!config.ServiceIsStopping.IsCancellationRequested)
            {
                try
                {
                    await BlockUntilWeAreTheLeader();
                }
                catch (TaskCanceledException)
                {
                    continue;
                }

                var noLongerTheLeader = new CancellationTokenSource();
                var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(config.ServiceIsStopping.Token, noLongerTheLeader.Token);
                try
                {
                    var leaseTask = RenewLease(noLongerTheLeader);
                    var startupTask = config.Startup(service, linkedTokenSource.Token);
                    var whenAnyTask = await Task.WhenAny(leaseTask, startupTask);

                    if (startupTask.IsFaulted)
                    {
                        config.ServiceIsStopping.Cancel();
                        await startupTask;
                    }
                    await whenAnyTask;
                }
                catch (TaskCanceledException)
                {
                }
            }
        }

        private async Task BlockUntilWeAreTheLeader()
        {
            var token = config.ServiceIsStopping.Token;
            while (!await config.LeadershipManager.AcquireLock(config.NodeId, token))
            {
                await Task.Delay(config.LeaderCheckEvery, token);
            }

            config.WhenLeaderIsElected(true);
        }

        private async Task RenewLease(CancellationTokenSource noLongerTheLeader)
        {
            try
            {
                var token = config.ServiceIsStopping.Token;
                while (await config.LeadershipManager.RenewLock(config.NodeId, token))
                {
                    await Task.Delay(config.LeaseUpdateEvery, token);
                }
            }
            finally 
            {
                noLongerTheLeader.Cancel();
                config.WhenLeaderIsElected(false);
            }
        }
    }
}