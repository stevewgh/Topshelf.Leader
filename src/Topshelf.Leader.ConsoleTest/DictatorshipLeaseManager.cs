using System;
using System.Threading;
using System.Threading.Tasks;

namespace Topshelf.Leader.ConsoleTest
{
    public class DictatorshipLeaseManager : ILeaseManager
    {
        public async Task<bool> AcquireLease(string nodeId, CancellationToken token)
        {
            await Task.Delay(1000, token);
            return true;
        }

        public async Task<bool> RenewLease(string nodeId, CancellationToken token)
        {
            await Task.Delay(1000, token);
            return true;
        }

        public Task ReleaseLease(string nodeId)
        {
            return Task.FromResult(true);
        }
    }
}
