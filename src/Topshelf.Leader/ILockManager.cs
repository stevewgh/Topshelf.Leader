using System.Threading;
using System.Threading.Tasks;

namespace Topshelf.Leader
{
    public interface ILockManager
    {
        Task<bool> AcquireLock(string nodeId, CancellationToken token);

        Task<bool> RenewLock(string nodeId, CancellationToken token);
    }
}