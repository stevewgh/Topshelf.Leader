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
            ILeaseManager leaseManager,
            TimeSpan leaseRenewalEvery,
            TimeSpan leaderCheckEvery,
            CancellationTokenSource serviceIsStopping, 
            Action<bool> whenLeaderIsElected)
        {
            Startup = startup ?? throw new ArgumentNullException(nameof(startup));
            LeaseManager = leaseManager ?? throw new ArgumentNullException(nameof(leaseManager));

            if (leaseRenewalEvery <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(leaseRenewalEvery), "Must not be less than or equal to zero.");
            }

            if (leaderCheckEvery <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(leaseRenewalEvery), "Must not be less than or equal to zero.");
            }

            if (string.IsNullOrEmpty(nodeId))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(nodeId));
            }

            NodeId = nodeId;
            LeaseCriteria = new LeaseCriteria(leaseRenewalEvery, leaderCheckEvery);
            ServiceIsStopping = serviceIsStopping;
            WhenLeaderIsElected = whenLeaderIsElected;
        }

        public string NodeId { get; }

        public CancellationTokenSource ServiceIsStopping { get; }

        public Action<bool> WhenLeaderIsElected { get; }

        public Func<T, CancellationToken, Task> Startup { get; }

        public ILeaseManager LeaseManager { get; }

        public LeaseCriteria LeaseCriteria { get; }
    }
}