using System.Threading.Tasks;

namespace Topshelf.Leader
{
    public interface IDistributedLockManager
    {
        Task<bool> AcquireLock(string nodeId);
    }
}