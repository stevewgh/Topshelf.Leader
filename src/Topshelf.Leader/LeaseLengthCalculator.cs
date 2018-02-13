using System;

namespace Topshelf.Leader
{
    public interface ILeaseLengthCalculator
    {
        TimeSpan Calculate();
    }

    public class LeaseLengthCalculator : ILeaseLengthCalculator
    {
        private readonly LeaseCriteria leaseCriteria;

        public LeaseLengthCalculator(LeaseCriteria leaseCriteria)
        {
            this.leaseCriteria = leaseCriteria;
        }

        public virtual TimeSpan Calculate()
        {
            return TimeSpan.FromSeconds((leaseCriteria.RenewLeaseEvery.TotalSeconds +
                                  leaseCriteria.AquireLeaseEvery.TotalSeconds) / 2);
        }
    }
}
