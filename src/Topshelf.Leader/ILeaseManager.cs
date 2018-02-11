using System;
using System.Threading;
using System.Threading.Tasks;

namespace Topshelf.Leader
{
    public struct LeaseOptions
    {
        public string NodeId { get; }
        public TimeSpan LeaseLength { get; }

        public LeaseOptions(string nodeId, TimeSpan leaseLength)
        {
            if (string.IsNullOrEmpty(nodeId))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(nodeId));
            }

            if(leaseLength <= TimeSpan.Zero)
            {
                throw new ArgumentException("Value cannot be less than or equal to TimeSpan.Zero.", nameof(leaseLength));
            }

            NodeId = nodeId;
            LeaseLength = leaseLength;
        }
    }

    public struct LeaseReleaseOptions
    {
        public string NodeId { get; }

        public LeaseReleaseOptions(string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(nodeId));
            }
            NodeId = nodeId;
        }
    }

    public interface ILeaseManager
    {
        Task<bool> AcquireLease(LeaseOptions options, CancellationToken token);

        Task<bool> RenewLease(LeaseOptions options, CancellationToken token);

        Task ReleaseLease(LeaseReleaseOptions options);
    }
}