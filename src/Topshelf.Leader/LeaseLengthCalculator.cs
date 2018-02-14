using System;

namespace Topshelf.Leader
{
    public interface ILeaseLengthCalculator
    {
        TimeSpan Calculate();
    }

    public class LeaseLengthCalculator : ILeaseLengthCalculator
    {
        protected readonly LeaseCriteria LeaseCriteria;

        public LeaseLengthCalculator(LeaseCriteria leaseCriteria)
        {
            this.LeaseCriteria = leaseCriteria;
        }

        public virtual TimeSpan Calculate()
        {
            return TimeSpan.FromSeconds((LeaseCriteria.RenewLeaseEvery.TotalSeconds +
                                  LeaseCriteria.AquireLeaseEvery.TotalSeconds) / 2);
        }
    }
}
