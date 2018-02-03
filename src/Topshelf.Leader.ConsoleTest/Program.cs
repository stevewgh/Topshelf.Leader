using System;

namespace Topshelf.Leader.ConsoleTest
{
    public class Program
    {
        static void Main(string[] args)
        {
            var rc = HostFactory.Run(x =>
            {
                var svc = new BadService();
                var leadershipManager = new BadLeadershipManager(5);

                x.Service<BadService>(s =>
                {
                    s.WhenStartedAsLeader(builder =>
                    {
                        builder.AttemptToBeTheLeaderEvery(TimeSpan.FromSeconds(2));
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
                        builder.WithLeadershipManager(leadershipManager);
                        builder.WhenStarted(async (service, token) =>
                        {
                            await service.Start(token);
                        });
                    });
                    s.ConstructUsing(name => svc);
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
}