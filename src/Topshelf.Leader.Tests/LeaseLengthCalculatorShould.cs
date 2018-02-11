using System;
using Xunit;

namespace Topshelf.Leader.Tests
{
    public class LeaseLengthCalculatorShould
    {
        private readonly TimeSpan tryToAquireLeaseEvery = TimeSpan.FromMinutes(1);
        private readonly TimeSpan leaseRenewalEvery = TimeSpan.FromSeconds(30);

        [Fact]
        public void calculate_a_lease_length_no_larger_than_the_lease_aquire_time()
        {
            var sut = new LeaseLengthCalculator();

            var length = sut.Calculate(leaseRenewalEvery, tryToAquireLeaseEvery);

            Assert.True(length <= tryToAquireLeaseEvery);
        }

        [Fact]
        public void calculate_a_lease_length_larger_than_the_lease_renewal_time()
        {
            var sut = new LeaseLengthCalculator();

            var length = sut.Calculate(leaseRenewalEvery, tryToAquireLeaseEvery);

            Assert.True(length > leaseRenewalEvery);
        }
    }
}