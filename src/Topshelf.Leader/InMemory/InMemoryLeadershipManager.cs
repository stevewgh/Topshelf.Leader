using System.Threading;
using System.Threading.Tasks;

namespace Topshelf.Leader.InMemory
{
    public class InMemoryLeadershipManager : ILeadershipManager
    {
        private readonly string owningNodeId;

        public InMemoryLeadershipManager(string owningNodeId)
        {
            this.owningNodeId = owningNodeId;
        }
        public Task<bool> AcquireLock(string nodeId, CancellationToken token)
        {
            return Task.FromResult(nodeId == owningNodeId);
        }

        public Task<bool> RenewLock(string nodeId, CancellationToken token)
        {
            return Task.FromResult(nodeId == owningNodeId);
        }
    }
}
