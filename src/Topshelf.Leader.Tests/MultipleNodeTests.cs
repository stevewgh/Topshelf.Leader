using System;
using System.Threading.Tasks;
using Topshelf.Leader.InMemory;
using Topshelf.Leader.Tests.Services;
using Xunit;

namespace Topshelf.Leader.Tests
{
    public class MultipleNodeTests
    {
        const string node1 = "node1";
        const string node2 = "node2";

        [Fact]
        public async Task only_one_node_should_be_running_at_any_one_time()
        {
            var manager = new InMemoryLeadershipManager(node1);

            BuildSubject(node1, manager, out var service1, out var node1Runner);
            BuildSubject(node2, manager, out var service2, out var node2Runner);

            //  start the runners async
#pragma warning disable 4014
            node1Runner.Start();
            node2Runner.Start();
#pragma warning restore 4014

            await Task.Delay(500);

            Assert.True(service1.Started);
            Assert.False(service2.Started);

            manager.AssignLeader(node2);
            await Task.Delay(500);

            Assert.False(service1.Started);
            Assert.True(service2.Started);
        }

        [Fact]
        public async Task should_be_notified_when_the_leadership_changes()
        {
            var manager = new InMemoryLeadershipManager(node1);
            var node1Elected = false;
            var node2Elected = false;

            BuildSubject(node1, manager, b =>{if(!node1Elected) node1Elected = b;}, out var _, out var node1Runner);
            BuildSubject(node2, manager, b => { if (!node2Elected) node2Elected = b; }, out var _, out var node2Runner);

            //  start the runners async
#pragma warning disable 4014
            node1Runner.Start();
            node2Runner.Start();
#pragma warning restore 4014

            await Task.Delay(500);

            manager.AssignLeader(node2);
            await Task.Delay(500);

            Assert.True(node1Elected);
            Assert.True(node2Elected);
        }

        private static void BuildSubject(string nodeid, ILeadershipManager manager, out TestService servicewithStopSupport, out Runner<TestService> runner)
        {
            BuildSubject(nodeid, manager, b => { }, out servicewithStopSupport, out runner);
        }

        private static void BuildSubject(string nodeid, ILeadershipManager manager, Action<bool> whenLeaderElected, out TestService servicewithStopSupport, out Runner<TestService> runner)
        {
            var config = new LeaderConfigurationBuilder<TestService>()
                .SetNodeId(nodeid)
                .AttemptToBeTheLeaderEvery(TimeSpan.FromMilliseconds(100))
                .UpdateLeaseEvery(TimeSpan.FromMilliseconds(50))
                .WhenStarted(async (s, token) => await s.Start(token))
                .WhenLeaderIsElected(whenLeaderElected)
                .WithLeadershipManager(manager);

            servicewithStopSupport = new TestService();
            runner = new Runner<TestService>(servicewithStopSupport, config.Build());
        }

    }
}
