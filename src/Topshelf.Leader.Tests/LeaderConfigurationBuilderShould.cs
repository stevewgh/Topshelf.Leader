using System;
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
        public void use_the_lease_renewal_time_that_is_provided()
        {
            var leaseUpdate = TimeSpan.FromDays(1);

            var builder = new LeaderConfigurationBuilder<object>();
            builder.WhenStarted((o, token) => Task.CompletedTask);
            builder.UpdateLeaseEvery(leaseUpdate);

            Assert.Equal(leaseUpdate, builder.Build().LeaseUpdateEvery);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void guard_against_invalid_lease_renewal_times(int seconds)
        {
            var leaseUpdate = TimeSpan.FromSeconds(seconds);

            var builder = new LeaderConfigurationBuilder<object>();

            Assert.Throws<ArgumentOutOfRangeException>(() => builder.UpdateLeaseEvery(leaseUpdate));
        }

        [Fact]
        public void use_the_health_check_time_that_is_provided()
        {
            var healthCheckEvery = TimeSpan.FromDays(1);

            var builder = new LeaderConfigurationBuilder<object>();
            builder.WhenStarted((o, token) => Task.CompletedTask);
            builder.AttemptToBeTheLeaderEvery(healthCheckEvery);

            Assert.Equal(healthCheckEvery, builder.Build().LeaderCheckEvery);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void guard_against_invalid_health_check_times(int seconds)
        {
            var healthCheckEvery = TimeSpan.FromSeconds(seconds);

            var builder = new LeaderConfigurationBuilder<object>();

            Assert.Throws<ArgumentOutOfRangeException>(() => builder.AttemptToBeTheLeaderEvery(healthCheckEvery));
        }

        [Fact]
        public void use_the_lock_manager_that_is_provided()
        {
            var manager = A.Fake<ILeadershipManager>();

            var builder = new LeaderConfigurationBuilder<object>();
            builder.WhenStarted((o, token) => Task.CompletedTask);
            builder.WithLeadershipManager(manager);

            Assert.Same(manager, builder.Build().LeadershipManager);
        }
    }
}