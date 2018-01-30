using System;
using System.Threading;
using Topshelf.Leader.InMemory;

namespace Topshelf.Leader
{
    public class LeaderConfigurationBuilder<T>
    {
        internal readonly TimeSpan DefaultTimeBetweenLeaseUpdates = TimeSpan.FromSeconds(5);
        internal readonly TimeSpan DefaultTimeBetweenCheckingLeaderHealth = TimeSpan.FromSeconds(60);

        private Action<T, CancellationToken> whenStarted;
        private TimeSpan timeBetweenLeaseUpdates;
        private TimeSpan timeBetweenCheckingLeaderHealth;
        private string nodeId;
        private ILockManager lockManager;
        private CancellationToken serviceIsStopping = CancellationToken.None;

        public LeaderConfigurationBuilder()
        {
            timeBetweenLeaseUpdates = DefaultTimeBetweenLeaseUpdates;
            timeBetweenCheckingLeaderHealth = DefaultTimeBetweenCheckingLeaderHealth;
            nodeId = Guid.NewGuid().ToString();
            lockManager = new InMemoryLockManager(nodeId);
        }

        public LeaderConfigurationBuilder<T> WithLockManager(ILockManager manager)
        {
            lockManager = manager ?? throw new ArgumentNullException(nameof(manager));
            return this;
        }

        public LeaderConfigurationBuilder<T> WhenStarted(Action<T, CancellationToken> startup)
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

        public LeaderConfigurationBuilder<T> CheckHealthOfLeaderEvery(TimeSpan timeBetweenChecks)
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

        internal bool ServiceStoppingTokenIsSet { get; private set; }

        internal LeaderConfigurationBuilder<T> WhenServiceIsStopping(CancellationToken serviceStopping)
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

            return new LeaderConfiguration<T>(whenStarted, nodeId, lockManager, timeBetweenLeaseUpdates, timeBetweenCheckingLeaderHealth, serviceIsStopping);
        }
    }
}