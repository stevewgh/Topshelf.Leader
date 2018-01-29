using System;
using System.Threading;
using System.Threading.Tasks;

namespace Topshelf.Leader.ConsoleTest
{
    public class Program
    {
        static void Main(string[] args)
        {
            var rc = HostFactory.Run(x =>
            {
                x.Service<TheService>(s =>
                {
                    s.WhenStartedAsLeader(builder =>
                    {
                        builder.WhenStarted(async (service, token) =>
                        {
                            await service.Start(token);
                        });
                    });
                    s.ConstructUsing(name => new TheService());
                    s.WhenStopped(service => service.Stop());
                });

                x.OnException(Console.WriteLine);
            });
        }
    }

    public class TheService
    {
        public async Task Start(CancellationToken stopToken)
        {
            while (!stopToken.IsCancellationRequested)
            {
                Console.WriteLine("Doing work");
                await TaskExtension.DelayWithoutTimeoutException(TimeSpan.FromSeconds(1), stopToken);
            }
        }

        public void Stop()
        {
            Console.WriteLine("Stopping");
        }
    }

}
