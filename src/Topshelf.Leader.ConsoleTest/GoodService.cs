using System;
using System.Threading;
using System.Threading.Tasks;

namespace Topshelf.Leader.ConsoleTest
{
    public class GoodService
    {
        public async Task Start(CancellationToken stopToken)
        {
            while (!stopToken.IsCancellationRequested)
            {
                Console.WriteLine($"Doing work {DateTime.Now}");
                await Task.Delay(TimeSpan.FromSeconds(1), stopToken);
            }
        }

        public void Stop()
        {
            Console.WriteLine("Stopping");
        }
    }
}