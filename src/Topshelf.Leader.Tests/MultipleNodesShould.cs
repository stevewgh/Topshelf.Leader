using System;
using System.Threading.Tasks;
using Topshelf.Leader.InMemory;
using Xunit;

namespace Topshelf.Leader.Tests
{
    public class MultipleNodesShould
    {
        [Fact]
        public async Task have_one_instance_running_at_any_one_time()
        {
            const string node1 = "node1";
            const string node2 = "node2";

            var manager = new InMemoryLeadershipManager(node1);

            BuildSubject(node1, manager, out var service1, out var node1Runner);
            BuildSubject(node2, manager, out var service2, out var node2Runner);

            //  start the runners async
            node1Runner.Start();
            node2Runner.Start();

            //  give time for them to start
            await Task.Delay(500);

            //  
            Assert.True(service1.Started);
            Assert.False(service2.Started);

            manager.AssignLeader(node2);

            await Task.Delay(500);

            Assert.False(service1.Started);
            Assert.True(service2.Started);
        }

        private static void BuildSubject(string nodeid, ILeadershipManager manager, out TestService service, out Runner<TestService> runner)
        {
            var config = new LeaderConfigurationBuilder<TestService>()
                .SetNodeId(nodeid)
                .AttemptToBeTheLeaderEvery(TimeSpan.FromMilliseconds(100))
                .UpdateLeaseEvery(TimeSpan.FromMilliseconds(50))
                .WhenStarted(async (s, token) => await s.Start(token))
                .WithLeadershipManager(manager);

            service = new TestService();
            runner = new Runner<TestService>(service, config.Build());
        }
    }
}
