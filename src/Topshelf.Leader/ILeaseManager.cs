using System.Threading;
using System.Threading.Tasks;

namespace Topshelf.Leader
{
    public interface ILeaseManager
    {
        Task<bool> AcquireLease(string nodeId, CancellationToken token);

        Task<bool> RenewLease(string nodeId, CancellationToken token);

        Task ReleaseLease(string nodeId);
    }
}