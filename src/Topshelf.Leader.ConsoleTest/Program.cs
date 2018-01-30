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
                        builder.CheckHealthOfLeaderEvery(TimeSpan.FromSeconds(10));
                        builder.WhenLeaderIsElected(iamLeader =>
                        {
                            if (iamLeader)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.BackgroundColor = ConsoleColor.White;
                            }
                            Console.WriteLine($"Leader election took place: {iamLeader}");
                            Console.ForegroundColor = ConsoleColor.Gray;
                            Console.BackgroundColor = ConsoleColor.Black;
                        });
                        builder.WithLockManager(new FlipFlopLockManager());
                        builder.WhenStarted(async (service, token) =>
                        {
                            await service.Start(token);
                        });
                    });
                    s.ConstructUsing(name => new TheService());
                    s.WhenStopped(service => service.Stop());
                });

                x.OnException(ex=>
                {
                    Console.WriteLine(ex);
                    Console.ReadLine();
                });
            });
            Environment.ExitCode = (int)rc;
        }
    }

    public class FlipFlopLockManager : ILockManager
    {
        public async Task<bool> AcquireLock(string nodeId, CancellationToken token)
        {
            return DateTime.Now.Second > 30;
        }

        public async Task<bool> RenewLock(string nodeId, CancellationToken token)
        {
            return DateTime.Now.Second > 30;
        }
    }

    public class TheService
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