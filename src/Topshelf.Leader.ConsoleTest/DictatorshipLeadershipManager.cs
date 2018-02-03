using System;
using System.Threading;
using System.Threading.Tasks;

namespace Topshelf.Leader.ConsoleTest
{
    public class DictatorshipLeadershipManager : ILeadershipManager
    {
        public async Task<bool> AcquireLock(string nodeId, CancellationToken token)
        {
            await Task.Delay(1000, token);
            return true;
        }

        public async Task<bool> RenewLock(string nodeId, CancellationToken token)
        {
            await Task.Delay(1000, token);
            return true;
        }
    }
}
