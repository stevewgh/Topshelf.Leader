using System;

namespace Topshelf.Leader
{
    public class LeaseCriteria
    {
        public TimeSpan AquireLeaseEvery { get; }
        public TimeSpan RenewLeaseEvery { get; }

        public LeaseCriteria(TimeSpan renewLeaseEvery, TimeSpan aquireLeaseEvery)
        {
            if (renewLeaseEvery <= TimeSpan.Zero)
            {
                throw new ArgumentException("Must be greater than zero.", nameof(renewLeaseEvery));
            }

            if (aquireLeaseEvery <= TimeSpan.Zero)
            {
                throw new ArgumentException("Must be greater than zero.", nameof(aquireLeaseEvery));
            }

            RenewLeaseEvery = renewLeaseEvery;
            AquireLeaseEvery = aquireLeaseEvery;
        }
    }
}