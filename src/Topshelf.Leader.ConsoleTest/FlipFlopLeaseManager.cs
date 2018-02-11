using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Topshelf.Leader.ConsoleTest
{
    public class FlipFlopLeaseManager : ILeaseManager
    {
        private bool weAreTheLeader;
        private readonly int secondsInEachState;
        private readonly Stopwatch timeThatWeHaveBeenLeader = new Stopwatch();
        private readonly Stopwatch timeThatWeHaveNotBeenLeader = new Stopwatch();

        public FlipFlopLeaseManager(bool weAreTheLeader, int secondsInEachState)
        {
            this.weAreTheLeader = weAreTheLeader;
            this.secondsInEachState = secondsInEachState;
            if (weAreTheLeader)
            {
                timeThatWeHaveBeenLeader.Start();
            }
            else
            {
                timeThatWeHaveNotBeenLeader.Start();
            }
        }

        public async Task<bool> AcquireLease(LeaseOptions options, CancellationToken token)
        {
            LeaderSwapOverIfRequired();
            await Task.Delay(500, token);
            return weAreTheLeader;
        }

        public async Task<bool> RenewLease(LeaseOptions options, CancellationToken token)
        {
            LeaderSwapOverIfRequired();

            await Task.Delay(500, token);
            return weAreTheLeader;
        }

        public Task ReleaseLease(LeaseReleaseOptions options)
        {
            if (weAreTheLeader)
            {
                LeaderSwapOverIfRequired();
            }

            return Task.FromResult(true);
        }

        private void LeaderSwapOverIfRequired()
        {
            if (weAreTheLeader && timeThatWeHaveBeenLeader.Elapsed.TotalSeconds >= secondsInEachState)
            {
                weAreTheLeader = false;
                timeThatWeHaveBeenLeader.Reset();
                timeThatWeHaveNotBeenLeader.Restart();
            }
            else if (!weAreTheLeader && timeThatWeHaveNotBeenLeader.Elapsed.TotalSeconds >= secondsInEachState)
            {
                weAreTheLeader = true;
                timeThatWeHaveNotBeenLeader.Reset();
                timeThatWeHaveBeenLeader.Restart();
            }
        }
    }
}