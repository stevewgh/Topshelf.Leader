using System;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Xunit;

namespace Topshelf.Leader.Tests
{
    public class LeaderConfigurationBuilderShould
    {
        [Fact]
        public void prevent_a_leader_configuration_which_doesnt_handle_starting_up()
        {
            var builder = new LeaderConfigurationBuilder<object>();
            Assert.Throws<HostConfigurationException>(() => builder.Build());
        }

        [Fact]
        public void set_a_unique_nodeid_if_one_isnt_provided()
        {
            var builder = new LeaderConfigurationBuilder<object>();
            builder.WhenStarted((o, token) => Task.CompletedTask);
            var firstId = builder.Build().NodeId;

            builder = new LeaderConfigurationBuilder<object>();
            builder.WhenStarted((o, token) => Task.CompletedTask);
            var secondId = builder.Build().NodeId;

            Assert.NotStrictEqual(firstId, secondId);
        }

        [Fact]
        public void use_the_nodeid_that_is_provided()
        {
            const string nodeid = "testvalue";

            var builder = new LeaderConfigurationBuilder<object>();
            builder.WhenStarted((o, token) => Task.CompletedTask);
            builder.SetNodeId(nodeid);

            Assert.Equal(nodeid, builder.Build().NodeId);
        }

        [Fact]
        public void use_the_lease_manager_that_is_provided()
        {
            var manager = A.Fake<ILeaseManager>();

            var builder = new LeaderConfigurationBuilder<object>();
            builder.WhenStarted((o, token) => Task.CompletedTask);
            builder.Lease(managerBuilder => managerBuilder.WithLeaseManager((c) => manager));

            Assert.Same(manager, builder.Build().LeaseManager);
        }

        [Fact]
        public void use_the_hearbeat_that_was_provided()
        {
            var heartBeatInterval = TimeSpan.Zero;
            var onHeartBeat = new Func<bool, CancellationToken, Task>((isActive, token) => Task.CompletedTask);

            var builder = new LeaderConfigurationBuilder<object>();
            builder.WhenStarted((o, token) => Task.CompletedTask);
            builder.WithHeartBeat(heartBeatInterval, onHeartBeat);

            var config = builder.Build();

            Assert.Equal(heartBeatInterval, config.HeartBeatInterval);
            Assert.Equal(onHeartBeat, config.OnHeartBeat);
        }
    }
}