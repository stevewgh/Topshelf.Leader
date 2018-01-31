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
            ILeadershipManager leadershipManager,
            TimeSpan leaseUpdateEvery,
            TimeSpan leaderCheckEvery,
            CancellationTokenSource serviceIsStopping, 
            Action<bool> whenLeaderIsElected)
        {
            Startup = startup ?? throw new ArgumentNullException(nameof(startup));
            LeadershipManager = leadershipManager ?? throw new ArgumentNullException(nameof(leadershipManager));

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

        public CancellationTokenSource ServiceIsStopping { get; }

        public Action<bool> WhenLeaderIsElected { get; }

        public Func<T, CancellationToken, Task> Startup { get; }

        public ILeadershipManager LeadershipManager { get; }
    }
}