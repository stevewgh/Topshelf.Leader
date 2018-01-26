using System.Threading.Tasks;

namespace Topshelf.Leader.HighAvailability
{
    public class LeaderManager : ILeaderManager
    {
        private int counter;

        public Task<bool> AmITheLeader(string uniqueid)
        {
            counter++;
            if (counter >= 10 && counter < 20)
            {
                return Task.FromResult(true);
            }

            if (counter >= 30 && counter < 40)
            {
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
    }
}