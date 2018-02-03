using System.Threading;
using System.Threading.Tasks;

namespace Topshelf.Leader.Tests
{
    public interface ITestService
    {
        Task StartWithNoLoop(CancellationToken stopToken);
        Task Start(CancellationToken stopToken);
        void Stop();
        bool Started { get; }
        bool Stopped { get; }
    }
}