using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Topshelf.Leader.ConsoleTest
{
    public class BadLeaseManager : ILeaseManager
    {
        private readonly int secondsInEachState;
        private readonly Stopwatch timeThatWeHaveBeenLeader = new Stopwatch();

        public BadLeaseManager(int secondsInEachState)
        {
            this.secondsInEachState = secondsInEachState;
            timeThatWeHaveBeenLeader.Start();
        }

        public async Task<bool> AcquireLease(LeaseOptions options, CancellationToken token)
        {
            LeaderSwapOverIfRequired();
            await Task.Delay(500, token);
            return true;
        }

        public async Task<bool> RenewLease(LeaseOptions options, CancellationToken token)
        {
            LeaderSwapOverIfRequired();

            await Task.Delay(500, token);
            return true;
        }

        public Task ReleaseLease(LeaseReleaseOptions options)
        {
            return Task.FromResult(true);
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