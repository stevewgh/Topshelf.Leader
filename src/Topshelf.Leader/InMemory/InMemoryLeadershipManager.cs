using System.Threading;
using System.Threading.Tasks;

namespace Topshelf.Leader.InMemory
{
    public class InMemoryLeadershipManager : ILeadershipManager
    {
        private string owningNodeId;

        public InMemoryLeadershipManager(string owningNodeId)
        {
            this.owningNodeId = owningNodeId;
        }

        public void AssignLeader(string newLeaderId)
        {
            this.owningNodeId = newLeaderId;
        }

        public Task<bool> AcquireLock(string nodeId, CancellationToken token)
        {
            return Task.FromResult(nodeId == owningNodeId);
        }

        public Task<bool> RenewLock(string nodeId, CancellationToken token)
        {
            return Task.FromResult(nodeId == owningNodeId);
        }

        public Task ReleaseLock(string nodeId, CancellationToken token)
        {
            owningNodeId = string.Empty;
            return Task.FromResult(true);
        }
    }
}
