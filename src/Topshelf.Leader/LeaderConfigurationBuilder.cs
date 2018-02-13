using System;
using System.Threading;
using System.Threading.Tasks;
using Topshelf.Leader.InMemory;

namespace Topshelf.Leader
{
    public class LeaderConfigurationBuilder<T>
    {
        private Func<T, CancellationToken, Task> whenStarted;
        private string nodeId;
        private CancellationTokenSource serviceIsStopping;
        private Action<bool> whenLeaderIsElected;
        private Action<LeaseConfigurationBuilder> leaseManagerAction = builder => builder.WithLeaseManager(c => new InMemoryLeaseManager(c.NodeId));

        public LeaderConfigurationBuilder()
        {
            nodeId = Guid.NewGuid().ToString();
            whenLeaderIsElected = b => { };
            serviceIsStopping = new CancellationTokenSource();
        }

        public LeaderConfigurationBuilder<T> Lease(Action<LeaseConfigurationBuilder> action)
        {
            leaseManagerAction = action ?? throw new ArgumentNullException(nameof(action));
            return this;
        }

        public LeaderConfigurationBuilder<T> WhenStarted(Func<T, CancellationToken, Task> startup)
        {
            whenStarted = startup ?? throw new ArgumentNullException(nameof(startup));
            return this;
        }

        public LeaderConfigurationBuilder<T> SetNodeId(string id)
        {
            nodeId = id;
            return this;
        }

        public LeaderConfigurationBuilder<T> WhenLeaderIsElected(Action<bool> leaderElection)
        {
            whenLeaderIsElected = leaderElection ?? throw new ArgumentNullException(nameof(leaderElection));
            return this;
        }

        internal bool ServiceStoppingTokenIsSet { get; private set; }

        internal LeaderConfigurationBuilder<T> WhenStopping(CancellationTokenSource serviceStopping)
        {
            serviceIsStopping = serviceStopping;
            ServiceStoppingTokenIsSet = true;
            return this;
        }

        public LeaderConfiguration<T> Build()
        {
            if (whenStarted == null)
            {
                throw new HostConfigurationException($"{nameof(WhenStarted)} must be provided.");
            }

            var leaseManagerBuilder = new LeaseConfigurationBuilder(nodeId);
            leaseManagerAction(leaseManagerBuilder);

            var leaseManagerConfiguration = leaseManagerBuilder.Build();
            return new LeaderConfiguration<T>(
                whenStarted,
                nodeId,
                leaseManagerConfiguration.LeaseManager(leaseManagerConfiguration),
                leaseManagerConfiguration,
                serviceIsStopping,
                whenLeaderIsElected);
        }
    }
}
