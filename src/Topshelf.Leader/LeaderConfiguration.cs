using System;
using System.Threading;
using System.Threading.Tasks;

namespace Topshelf.Leader
{
    public class LeaderConfiguration<T>
    {
        private readonly Action<bool> whenLeaderIsElected;

        public LeaderConfiguration(
            Func<T, CancellationToken, Task> startup,
            string nodeId,
            ILeaseManager leaseManager,
            LeaseConfiguration leaseConfiguration,
            CancellationTokenSource serviceIsStopping, 
            Action<bool> whenLeaderIsElected,
            TimeSpan heartBeatInterval,
            Func<bool, CancellationToken, Task> onHeartBeat)
        {
            Startup = startup ?? throw new ArgumentNullException(nameof(startup));
            LeaseManager = leaseManager ?? throw new ArgumentNullException(nameof(leaseManager));

            if (string.IsNullOrEmpty(nodeId))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(nodeId));
            }

            NodeId = nodeId;
            LeaseConfiguration = leaseConfiguration;
            ServiceIsStopping = serviceIsStopping;
            this.whenLeaderIsElected = whenLeaderIsElected;
            HeartBeatInterval = heartBeatInterval;
            OnHeartBeat = onHeartBeat;
        }

        public string NodeId { get; }

        public bool IsLeader { get; private set; }

        public CancellationTokenSource ServiceIsStopping { get; }

        public Action<bool> WhenLeaderIsElected
        {
            get
            {
                return isLeader =>
                {
                    this.IsLeader = isLeader;
                    this.whenLeaderIsElected(isLeader);
                };
            }
        }

        public TimeSpan HeartBeatInterval { get; }

        public Func<bool, CancellationToken, Task> OnHeartBeat { get; }

        public Func<T, CancellationToken, Task> Startup { get; }

        public ILeaseManager LeaseManager { get; }

        public LeaseConfiguration LeaseConfiguration { get; }
    }
}