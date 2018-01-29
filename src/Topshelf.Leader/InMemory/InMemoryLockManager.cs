using System.Threading.Tasks;

namespace Topshelf.Leader.InMemory
{
    public class InMemoryLockManager : IDistributedLockManager
    {
        public Task<bool> AcquireLock(string nodeId)
        {
            return Task.FromResult(true);
        }
    }
}
