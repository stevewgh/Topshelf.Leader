﻿using System;
using System.Threading.Tasks;
using Topshelf.Leader.InMemory;

namespace Topshelf.Leader.ConsoleTest
{
    public class Program
    {
        static void Main(string[] args)
        {
            var rc = HostFactory.Run(x =>
            {
                var svc = new GoodService();

                x.Service<GoodService>(s =>
                {
                    s.WhenStartedAsLeader(builder =>
                    {
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

                        builder.Lease(lcb =>
                        {
                            lcb.RenewLeaseEvery(TimeSpan.FromSeconds(2));
                            lcb.AquireLeaseEvery(TimeSpan.FromSeconds(5));
                            lcb.WithInMemoryLeaseManager();
                        });

                        builder.WithHeartBeat(TimeSpan.FromSeconds(5), (b, token) =>
                        {
                            Console.WriteLine($"Heartbeat [IsLeader: {b}] {DateTime.UtcNow:HH:mm:ss.fff}");
                            return Task.CompletedTask;
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