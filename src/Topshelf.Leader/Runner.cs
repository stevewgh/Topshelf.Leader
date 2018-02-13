using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Topshelf.Logging;

namespace Topshelf.Leader
{
    public class Runner<T>
    {
        private readonly T service;
        private readonly LeaderConfiguration<T> config;
        private readonly LogWriter logger = HostLogger.Get<Runner<T>>();

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

                using (var noLongerTheLeader = new CancellationTokenSource())
                {
                    using (var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(config.ServiceIsStopping.Token, noLongerTheLeader.Token))
                    {
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
            }
        }

        private async Task BlockUntilWeAreTheLeader()
        {
            var token = config.ServiceIsStopping.Token;
            while (!await config.LeaseManager.AcquireLease(new LeaseOptions(config.NodeId), token))
            {
                logger.DebugFormat("NodeId {0} failed to aquire a lease. Waiting {1}", config.NodeId, config.LeaseConfiguration.LeaseCriteria.AquireLeaseEvery);
                await Task.Delay(config.LeaseConfiguration.LeaseCriteria.AquireLeaseEvery, token);
            }
            logger.DebugFormat("NodeId {0} has been elected as leader", config.NodeId);
            config.WhenLeaderIsElected(true);
        }

        private async Task RenewLease(CancellationToken stopRenewing, CancellationTokenSource noLongerTheLeader)
        {
            try
            {
                while (await config.LeaseManager.RenewLease(new LeaseOptions(config.NodeId), stopRenewing))
                {
                    logger.DebugFormat("NodeId {0} renewed the lease. Waiting {1}", config.NodeId, config.LeaseConfiguration.LeaseCriteria.RenewLeaseEvery);
                    await Task.Delay(config.LeaseConfiguration.LeaseCriteria.RenewLeaseEvery, stopRenewing);
                }
            }
            finally 
            {
                logger.DebugFormat("NodeId {0} stopped renewing the lease.", config.NodeId);
                try
                {
                    noLongerTheLeader.Cancel();
                }
                finally
                {
                    config.WhenLeaderIsElected(false);
                }
            }
        }
    }
}