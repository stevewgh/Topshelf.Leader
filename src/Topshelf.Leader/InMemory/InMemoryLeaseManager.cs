using System.Threading;
using System.Threading.Tasks;

namespace Topshelf.Leader.InMemory
{
    using Logging;

    public class InMemoryLeaseManager : ILeaseManager
    {
        private string owningNodeId;
        private static bool warningGiven = false;

        public InMemoryLeaseManager(string owningNodeId)
        {
            this.owningNodeId = owningNodeId;
        }

        public void AssignLeader(string newLeaderId)
        {
            this.owningNodeId = newLeaderId;
        }

        public Task<bool> AcquireLease(LeaseOptions options, CancellationToken token)
        {
            WarnOfUse();
            return Task.FromResult(options.NodeId == owningNodeId);
        }

        public Task<bool> RenewLease(LeaseOptions options, CancellationToken token)
        {
            WarnOfUse();
            return Task.FromResult(options.NodeId == owningNodeId);
        }

        public Task ReleaseLease(LeaseReleaseOptions options)
        {
            WarnOfUse();
            owningNodeId = string.Empty;
            return Task.FromResult(true);
        }

        private static void WarnOfUse()
        {
            if (warningGiven)
            {
                return;
            }
            HostLogger.Get<InMemoryLeaseManager>().WarnFormat("{0} should not be used in Production !", nameof(InMemoryLeaseManager));
            warningGiven = true;
        }
    }
}
