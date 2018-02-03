using System;
using System.Threading;
using System.Threading.Tasks;

namespace Topshelf.Leader.Tests.Services
{
    public class TestService
    {
        public bool Started { get; private set; }

        public async Task StartWithNoLoop(CancellationToken stopToken)
        {
            Started = true;
            Console.WriteLine($"Doing work {DateTime.Now}");
            await Task.Delay(TimeSpan.FromMilliseconds(100), stopToken);
        }

        public async Task Start(CancellationToken stopToken)
        {
            Started = true;
            while (!stopToken.IsCancellationRequested)
            {
                Console.WriteLine($"Doing work {DateTime.Now}");
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), stopToken);
                }
                catch (TaskCanceledException)
                {
                    Started = false;
                    throw;
                }
            }
        }
    }
}