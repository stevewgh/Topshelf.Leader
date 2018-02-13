using System;

namespace Topshelf.Leader
{
    public class LeaseManagerConfiguration
    {
        internal Func<LeaseManagerConfiguration,ILeaseManager> LeaseManager { get; }
        public ILeaseLengthCalculator LeaseLengthCalculator { get; }
        public LeaseCriteria LeaseCriteria { get; }

        public LeaseManagerConfiguration(Func<LeaseManagerConfiguration,ILeaseManager> leaseManager, ILeaseLengthCalculator leaseLengthCalculator, LeaseCriteria leaseCriteria)
        {
            this.LeaseManager = leaseManager ?? throw new ArgumentNullException(nameof(leaseManager));
            this.LeaseLengthCalculator = leaseLengthCalculator ?? throw new ArgumentNullException(nameof(leaseLengthCalculator));
            LeaseCriteria = leaseCriteria;
        }
    }
}