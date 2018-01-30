using System;
using System.Threading;
using System.Threading.Tasks;

namespace Topshelf.Leader
{
    public class LeaderConfiguration<T>
    {
        public LeaderConfiguration(
            Func<T, CancellationToken, Task> startup,
            string nodeId,
            ILockManager lockManager,
            TimeSpan leaseUpdateEvery,
            TimeSpan leaderCheckEvery,
            CancellationToken serviceIsStopping, 
            Action<bool> whenLeaderIsElected)
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
            WhenLeaderIsElected = whenLeaderIsElected;
        }

        public string NodeId { get; }

        public TimeSpan LeaseUpdateEvery { get; }

        public TimeSpan LeaderCheckEvery { get; }

        public CancellationToken ServiceIsStopping { get; }

        public Action<bool> WhenLeaderIsElected { get; }

        public Func<T, CancellationToken, Task> Startup { get; }

        public ILockManager LockManager { get; }
    }
}