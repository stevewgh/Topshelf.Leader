using System.Threading.Tasks;

namespace Topshelf.Leader.HighAvailability
{
    public interface ILeaderManager
    {
        Task<bool> AmITheLeader(string uniqueid);
    }
}