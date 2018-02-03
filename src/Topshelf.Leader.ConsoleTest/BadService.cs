using System;
using System.Threading;
using System.Threading.Tasks;

namespace Topshelf.Leader.ConsoleTest
{
    public class BadService
    {
        public async Task Start(CancellationToken stopToken)
        {
            Console.WriteLine($"Bad Service Doing work {DateTime.Now}");
            await Task.Delay(TimeSpan.FromSeconds(1), stopToken);
            throw new Exception("Ooops!");
        }

        public void Stop()
        {
            Console.WriteLine("Bad Service Stopping");
        }
    }
}