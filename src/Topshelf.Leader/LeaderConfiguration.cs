using System;
using System.Threading;

namespace Topshelf.Leader
{
    public class LeaderConfiguration<T>
    {
        public LeaderConfiguration(
            Action<T, CancellationToken> startup, 
            string nodeId,
            ILockManager lockManager, 
            TimeSpan leaseUpdateEvery, 
            TimeSpan leaderCheckEvery,
            CancellationToken serviceIsStopping)
        {
            Startup = startup ?? throw new ArgumentNullException(nameof(startup));
            LockManager = lockManager ?? throw new ArgumentNullException(nameof(lockManager));

            if (leaseUpdateEvery <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(leaseUpdateEvery), "Must not be less than or equal to zero.");
            }

            if (leaderCheckEvery <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(leaseUpdateEvery), "Must not be less than or equal to zero.");
            }

            if (string.IsNullOrEmpty(nodeId))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(nodeId));
            }

            NodeId = nodeId;
            LeaseUpdateEvery = leaseUpdateEvery;
            LeaderCheckEvery = leaderCheckEvery;
            ServiceIsStopping = serviceIsStopping;
        }

        public string NodeId { get; }

        public TimeSpan LeaseUpdateEvery { get; }

        public TimeSpan LeaderCheckEvery { get; }

        public CancellationToken ServiceIsStopping { get; }

        public Action<T, CancellationToken> Startup { get; }

        public ILockManager LockManager { get; }
    }
}