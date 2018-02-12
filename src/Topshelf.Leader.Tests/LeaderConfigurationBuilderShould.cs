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

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void prevent_a_lease_from_being_aquired_more_frequently_than_it_is_renewed(int aquireEveryDays)
        {
            var builder = new LeaderConfigurationBuilder<object>();
            builder.WhenStarted((o, token) => Task.CompletedTask);
            builder.RenewLeaseEvery(TimeSpan.FromDays(2));
            builder.AquireLeaseEvery(TimeSpan.FromDays(aquireEveryDays));

            Assert.Throws<HostConfigurationException>(() => builder.Build());
        }

        [Fact]
        public void use_the_lease_renewal_time_that_is_provided()
        {
            var leaseUpdate = TimeSpan.FromDays(1);

            var builder = new LeaderConfigurationBuilder<object>();
            builder.WhenStarted((o, token) => Task.CompletedTask);
            builder.RenewLeaseEvery(leaseUpdate);
            builder.AquireLeaseEvery(TimeSpan.FromDays(2));

            Assert.Equal(leaseUpdate, builder.Build().LeaseCriteria.RenewLeaseEvery);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void guard_against_invalid_lease_renewal_times(int seconds)
        {
            var leaseUpdate = TimeSpan.FromSeconds(seconds);

            var builder = new LeaderConfigurationBuilder<object>();

            Assert.Throws<ArgumentOutOfRangeException>(() => builder.RenewLeaseEvery(leaseUpdate));
        }

        [Fact]
        public void use_the_aquire_lease_time_that_is_provided()
        {
            var healthCheckEvery = TimeSpan.FromDays(1);

            var builder = new LeaderConfigurationBuilder<object>();
            builder.WhenStarted((o, token) => Task.CompletedTask);
            builder.AquireLeaseEvery(healthCheckEvery);

            Assert.Equal(healthCheckEvery, builder.Build().LeaseCriteria.AquireLeaseEvery);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void guard_against_invalid_health_check_times(int seconds)
        {
            var healthCheckEvery = TimeSpan.FromSeconds(seconds);

            var builder = new LeaderConfigurationBuilder<object>();

            Assert.Throws<ArgumentOutOfRangeException>(() => builder.AquireLeaseEvery(healthCheckEvery));
        }

        [Fact]
        public void use_the_lease_manager_that_is_provided()
        {
            var manager = A.Fake<ILeaseManager>();

            var builder = new LeaderConfigurationBuilder<object>();
            builder.WhenStarted((o, token) => Task.CompletedTask);
            builder.WithLeaseManager(managerBuilder => managerBuilder.Factory(criteria => manager));

            Assert.Same(manager, builder.Build().LeaseManager);
        }
    }
}