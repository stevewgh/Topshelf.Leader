using System;
using FakeItEasy;
using Topshelf.Leader.InMemory;
using Xunit;

namespace Topshelf.Leader.Tests
{
    public class LeaseConfigurationBuilderShould
    {
        private const string NodeId = "Node1";

        [Fact]
        public void use_the_lease_calculator_that_is_provided()
        {
            var fakeCalculator = A.Fake<ILeaseLengthCalculator>();
            var builder = new LeaseConfigurationBuilder(NodeId);
            builder.LeaseLength(() => fakeCalculator);
            var config = builder.Build();

            Assert.Same(fakeCalculator, config.LeaseLengthCalculator);
        }

        [Fact]
        public void provide_the_lease_calculator_to_the_lease_manager()
        {
            var fakeCalculator = A.Fake<ILeaseLengthCalculator>();
            var lcb = new LeaderConfigurationBuilder<object>();
            ILeaseLengthCalculator calculatorProvided = null;

            lcb.WhenStarted((o, token) => throw new Exception())
                .Lease(cb =>
                {
                    cb.WithLeaseManager(lc =>
                    {
                        calculatorProvided = lc.LeaseLengthCalculator;
                        return new InMemoryLeaseManager(NodeId);
                    }).LeaseLength(() => fakeCalculator);
                });

            lcb.Build();

            Assert.Same(fakeCalculator, calculatorProvided);
        }

        [Fact]
        public void use_the_lease_manager_that_is_provided()
        {
            var fakeManager = A.Fake<ILeaseManager>();
            var builder = new LeaseConfigurationBuilder(NodeId);
            builder.WithLeaseManager(configuration => fakeManager);
            var config = builder.Build();

            Assert.Same(fakeManager, config.LeaseManager(config));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void prevent_a_lease_from_being_aquired_more_frequently_than_it_is_renewed(int aquireEveryDays)
        {
            var builder = new LeaseConfigurationBuilder(NodeId);
            builder.RenewLeaseEvery(TimeSpan.FromDays(2));
            builder.AquireLeaseEvery(TimeSpan.FromDays(aquireEveryDays));

            Assert.Throws<HostConfigurationException>(() => builder.Build());
        }

        [Fact]
        public void use_the_lease_renewal_time_that_is_provided()
        {
            var leaseUpdate = TimeSpan.FromDays(1);

            var builder = new LeaseConfigurationBuilder(NodeId);
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

            var builder = new LeaseConfigurationBuilder(NodeId);

            Assert.Throws<ArgumentOutOfRangeException>(() => builder.RenewLeaseEvery(leaseUpdate));
        }

        [Fact]
        public void use_the_aquire_lease_time_that_is_provided()
        {
            var healthCheckEvery = TimeSpan.FromDays(1);

            var builder = new LeaseConfigurationBuilder(NodeId);
            builder.AquireLeaseEvery(healthCheckEvery);

            Assert.Equal(healthCheckEvery, builder.Build().LeaseCriteria.AquireLeaseEvery);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void guard_against_invalid_health_check_times(int seconds)
        {
            var healthCheckEvery = TimeSpan.FromSeconds(seconds);

            var builder = new LeaseConfigurationBuilder(NodeId);

            Assert.Throws<ArgumentOutOfRangeException>(() => builder.AquireLeaseEvery(healthCheckEvery));
        }
    }
}