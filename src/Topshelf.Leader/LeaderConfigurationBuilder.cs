using System;
using System.Threading;
using System.Threading.Tasks;
using Topshelf.Leader.InMemory;

namespace Topshelf.Leader
{
    public class LeaderConfigurationBuilder<T>
    {
        internal readonly TimeSpan DefaultTimeBetweenLeaseUpdates = TimeSpan.FromSeconds(5);
        internal readonly TimeSpan DefaultTimeBetweenCheckingLeaderHealth = TimeSpan.FromSeconds(60);

        private Func<T, CancellationToken, Task> whenStarted;
        private TimeSpan timeBetweenLeaseUpdates;
        private TimeSpan timeBetweenCheckingLeaderHealth;
        private string nodeId;
        private ILeaseManager leaseManager;
        private CancellationTokenSource serviceIsStopping;
        private Action<bool> whenLeaderIsElected;

        public LeaderConfigurationBuilder()
        {
            timeBetweenLeaseUpdates = DefaultTimeBetweenLeaseUpdates;
            timeBetweenCheckingLeaderHealth = DefaultTimeBetweenCheckingLeaderHealth;
            nodeId = Guid.NewGuid().ToString();
            leaseManager = new InMemoryLeaseManager(nodeId);
            whenLeaderIsElected = b => { };
            serviceIsStopping = new CancellationTokenSource();
        }

        public LeaderConfigurationBuilder<T> WithLeaseManager(ILeaseManager manager)
        {
            leaseManager = manager ?? throw new ArgumentNullException(nameof(manager));
            return this;
        }

        public LeaderConfigurationBuilder<T> WhenStarted(Func<T, CancellationToken, Task> startup)
        {
            whenStarted = startup ?? throw new ArgumentNullException(nameof(startup));
            return this;
        }

        public LeaderConfigurationBuilder<T> UpdateLeaseEvery(TimeSpan timeBetweenChecks)
        {
            if (timeBetweenChecks <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(timeBetweenChecks), "Must be greater than zero.");
            }

            timeBetweenLeaseUpdates = timeBetweenChecks;

            return this;
        }

        public LeaderConfigurationBuilder<T> AttemptToBeTheLeaderEvery(TimeSpan timeBetweenChecks)
        {
            if (timeBetweenChecks <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(timeBetweenChecks), "Must be greater than zero.");
            }

            timeBetweenCheckingLeaderHealth = timeBetweenChecks;

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

        internal LeaderConfiguration<T> Build()
        {
            if (whenStarted == null)
            {
                throw new HostConfigurationException($"{nameof(WhenStarted)} must be provided.");
            }

            return new LeaderConfiguration<T>(whenStarted, nodeId, leaseManager, timeBetweenLeaseUpdates, timeBetweenCheckingLeaderHealth, serviceIsStopping, whenLeaderIsElected);
        }

    }
}