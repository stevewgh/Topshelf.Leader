using System;
using Topshelf.Leader.InMemory;

namespace Topshelf.Leader.ConsoleTest
{
    public class Program
    {
        static void Main(string[] args)
        {
            var rc = HostFactory.Run(x =>
            {
                var svc = new BadService();

                x.Service<BadService>(s =>
                {
                    s.WhenStartedAsLeader(builder =>
                    {
                        builder.RenewLeaseEvery(TimeSpan.FromSeconds(2));
                        builder.AquireLeaseEvery(TimeSpan.FromSeconds(5));
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
                        builder.WhenStarted(async (service, token) =>
                        {
                            await service.Start(token);
                        });
                        builder.WithLeaseManager(managerBuilder =>
                        {
                            managerBuilder.Factory(criteria => new InMemoryLeaseManager(managerBuilder.NodeId));
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