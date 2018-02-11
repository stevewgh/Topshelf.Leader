using System;
using Xunit;

namespace Topshelf.Leader.Tests
{
    public class LeaseLengthCalculatorShould
    {
        private readonly TimeSpan aquireLeaseEvery = TimeSpan.FromMinutes(1);
        private readonly TimeSpan renewLeaseEvery = TimeSpan.FromSeconds(30);
        private readonly LeaseCriteria leaseCriteria;

        public LeaseLengthCalculatorShould()
        {
            leaseCriteria = new LeaseCriteria(renewLeaseEvery, aquireLeaseEvery);
        }

        [Fact]
        public void calculate_a_lease_length_greater_than_zero()
        {
            var sut = new LeaseLengthCalculator();

            var length = sut.Calculate(leaseCriteria);

            Assert.True(length > TimeSpan.Zero);
        }

        [Fact]
        public void calculate_a_lease_length_no_larger_than_the_lease_aquire_time()
        {
            var sut = new LeaseLengthCalculator();

            var length = sut.Calculate(leaseCriteria);

            Assert.True(length <= aquireLeaseEvery);
        }

        [Fact]
        public void calculate_a_lease_length_larger_than_the_lease_renewal_time()
        {
            var sut = new LeaseLengthCalculator();

            var length = sut.Calculate(leaseCriteria);

            Assert.True(length > renewLeaseEvery);
        }
    }
}