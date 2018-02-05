using System.Threading;
using System.Threading.Tasks;

namespace Topshelf.Leader.InMemory
{
    public class InMemoryLeaseManager : ILeaseManager
    {
        private string owningNodeId;

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
            return Task.FromResult(nodeId == owningNodeId);
        }

        public Task<bool> RenewLease(string nodeId, CancellationToken token)
        {
            return Task.FromResult(nodeId == owningNodeId);
        }

        public Task ReleaseLease(string nodeId)
        {
            owningNodeId = string.Empty;
            return Task.FromResult(true);
        }
    }
}
