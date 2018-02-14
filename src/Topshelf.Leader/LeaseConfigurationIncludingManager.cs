using System;

namespace Topshelf.Leader
{
    public class LeaseConfigurationIncludingManager : LeaseConfiguration
    {
        public Func<LeaseConfiguration, ILeaseManager> LeaseManager { get; }

        public LeaseConfigurationIncludingManager(string nodeId, Func<LeaseConfiguration, ILeaseManager> leaseManager, ILeaseLengthCalculator leaseLengthCalculator, LeaseCriteria leaseCriteria) 
            : base(nodeId, leaseLengthCalculator, leaseCriteria)
        {
            LeaseManager = leaseManager;
        }
    }
}