using System;

namespace Topshelf.Leader
{
    public interface ILeaseLengthCalculator
    {
        TimeSpan Calculate(LeaseCriteria leaseCriteria);
    }

    public class LeaseLengthCalculator : ILeaseLengthCalculator
    {
        public virtual TimeSpan Calculate(LeaseCriteria leaseCriteria)
        {
            return TimeSpan.FromSeconds((leaseCriteria.RenewLeaseEvery.TotalSeconds +
                                  leaseCriteria.AquireLeaseEvery.TotalSeconds) / 2);
        }
    }
}
