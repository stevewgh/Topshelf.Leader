using System;
using System.Threading;
using System.Threading.Tasks;

namespace Topshelf.Leader
{
    public class LeaseOptions
    {
        public LeaseCriteria LeaseCriteria { get; }
        public string NodeId { get; }

        public LeaseOptions(string nodeId, LeaseCriteria leaseCriteria)
        {
            if (string.IsNullOrEmpty(nodeId))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(nodeId));
            }

            LeaseCriteria = leaseCriteria ?? throw new ArgumentNullException(nameof(leaseCriteria));
            NodeId = nodeId;
        }
    }

    public class LeaseReleaseOptions
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