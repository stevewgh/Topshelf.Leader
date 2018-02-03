using System;
using System.Collections.Generic;
using System.Linq;
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
                    var leaseTask = RenewLease(linkedTokenSource.Token, noLongerTheLeader);
                    var startupTask = config.Startup(service, linkedTokenSource.Token);
                    var whenAnyTask = await Task.WhenAny(leaseTask, startupTask);

                    var exceptions = new List<Exception>();
                    if (startupTask.IsFaulted)
                    {
                        config.ServiceIsStopping.Cancel();
                        if (startupTask.Exception != null)
                        {
                            exceptions.Add(startupTask.Exception.GetBaseException());
                        }
                    }

                    if (leaseTask.IsFaulted)
                    {
                        if (leaseTask.Exception != null)
                        {
                            exceptions.Add(leaseTask.Exception.GetBaseException());
                        }
                    }

                    if (exceptions.Any())
                    {
                        throw new AggregateException(exceptions);
                    }

                    await whenAnyTask;
                }
                catch (TaskCanceledException)
                {
                }
                finally
                {
                    linkedTokenSource.Cancel();
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

        private async Task RenewLease(CancellationToken stopRenewing, CancellationTokenSource noLongerTheLeader)
        {
            try
            {
                while (await config.LeadershipManager.RenewLock(config.NodeId, stopRenewing))
                {
                    await Task.Delay(config.LeaseUpdateEvery, stopRenewing);
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