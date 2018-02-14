using System;

namespace Topshelf.Leader
{
    public class LeaseConfiguration
    {
        public string NodeId { get; }

        public ILeaseLengthCalculator LeaseLengthCalculator { get; }

        public LeaseCriteria LeaseCriteria { get; }

        public LeaseConfiguration(string nodeId, ILeaseLengthCalculator leaseLengthCalculator, LeaseCriteria leaseCriteria)
        {
            if (string.IsNullOrEmpty(nodeId))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(nodeId));
            }

            NodeId = nodeId;
            LeaseLengthCalculator = leaseLengthCalculator ?? throw new ArgumentNullException(nameof(leaseLengthCalculator));
            LeaseCriteria = leaseCriteria;
        }
    }
}