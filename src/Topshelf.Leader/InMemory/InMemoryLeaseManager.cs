using System.Threading;
using System.Threading.Tasks;
using Topshelf.Logging;

namespace Topshelf.Leader.InMemory
{
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

        public Task<bool> AcquireLease(string nodeId, CancellationToken token)
        {
            WarnOfUse();
            return Task.FromResult(nodeId == owningNodeId);
        }

        public Task<bool> RenewLease(string nodeId, CancellationToken token)
        {
            WarnOfUse();
            return Task.FromResult(nodeId == owningNodeId);
        }

        public Task ReleaseLease(string nodeId)
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
