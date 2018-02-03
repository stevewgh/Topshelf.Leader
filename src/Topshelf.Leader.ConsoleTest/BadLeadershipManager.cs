using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Topshelf.Leader.ConsoleTest
{
    public class BadLeadershipManager : ILeadershipManager
    {
        private readonly int secondsInEachState;
        private readonly Stopwatch timeThatWeHaveBeenLeader = new Stopwatch();

        public BadLeadershipManager(int secondsInEachState)
        {
            this.secondsInEachState = secondsInEachState;
            timeThatWeHaveBeenLeader.Start();
        }

        public async Task<bool> AcquireLock(string nodeId, CancellationToken token)
        {
            LeaderSwapOverIfRequired();
            await Task.Delay(500, token);
            return true;
        }

        public async Task<bool> RenewLock(string nodeId, CancellationToken token)
        {
            LeaderSwapOverIfRequired();

            await Task.Delay(500, token);
            return true;
        }

        private void LeaderSwapOverIfRequired()
        {
            if (timeThatWeHaveBeenLeader.Elapsed.TotalSeconds >= secondsInEachState)
            {
                throw new Exception("Leadership Manager Failed...");
            }
        }
    }
}