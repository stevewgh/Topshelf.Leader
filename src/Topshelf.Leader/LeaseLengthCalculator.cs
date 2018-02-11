using System;

namespace Topshelf.Leader
{
    public class LeaseLengthCalculator
    {
        public virtual TimeSpan Calculate(LeaseCriteria leaseCriteria)
        {
            return TimeSpan.FromSeconds((leaseCriteria.RenewLeaseEvery.TotalSeconds +
                                  leaseCriteria.AquireLeaseEvery.TotalSeconds) / 2);
        }
    }
}
