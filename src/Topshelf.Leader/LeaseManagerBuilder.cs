using System;
using Topshelf.Leader.InMemory;

namespace Topshelf.Leader
{
    public class LeaseManagerBuilder
    {
        internal static readonly TimeSpan DefaultTimeBetweenLeaseUpdates = TimeSpan.FromSeconds(5);
        internal static readonly TimeSpan DefaultTimeBetweenCheckingLeaderHealth = TimeSpan.FromSeconds(60);

        public string NodeId { get; }
        private Func<LeaseManagerConfiguration,ILeaseManager> managerFunc;
        private Func<ILeaseLengthCalculator> calculatorFunc;
        private TimeSpan timeBetweenRenewing = DefaultTimeBetweenLeaseUpdates;
        private TimeSpan timeBetweenAquiring = DefaultTimeBetweenCheckingLeaderHealth;

        private class FixedLengthCalculator : ILeaseLengthCalculator
        {
            private readonly TimeSpan fixedLength;

            public FixedLengthCalculator(TimeSpan fixedLength)
            {
                this.fixedLength = fixedLength;
            }

            public TimeSpan Calculate(LeaseCriteria leaseCriteria)
            {
                return fixedLength;
            }
        }

        public LeaseManagerBuilder(string nodeId)
        {
            NodeId = nodeId;
            managerFunc = (c) => new InMemoryLeaseManager(nodeId);
        }

        public LeaseManagerBuilder RenewLeaseEvery(TimeSpan time)
        {
            if (time <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(time), "Must be greater than zero.");
            }

            timeBetweenRenewing = time;

            return this;
        }

        public LeaseManagerBuilder AquireLeaseEvery(TimeSpan time)
        {
            if (time <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(time), "Must be greater than zero.");
            }

            timeBetweenAquiring = time;

            return this;
        }

        public LeaseManagerBuilder LeaseLength(Func<ILeaseLengthCalculator> calculator)
        {
            calculatorFunc = calculator ?? throw new ArgumentNullException(nameof(calculator));
            return this;
        }

        public LeaseManagerBuilder LeaseLength(TimeSpan length)
        {
            if (length <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "Must be greater than zero.");
            }
            calculatorFunc = () => new FixedLengthCalculator(length);
            return this;
        }

        public LeaseManagerBuilder Factory(Func<LeaseManagerConfiguration,ILeaseManager> manager)
        {
            managerFunc = manager ?? throw new ArgumentNullException(nameof(manager));
            return this;
        }

        public LeaseManagerConfiguration Build()
        {
            if (timeBetweenAquiring <= timeBetweenRenewing)
            {
                throw new HostConfigurationException($"{nameof(AquireLeaseEvery)} must be greater than {nameof(RenewLeaseEvery)}.");
            }

            var leaseCriteria = new LeaseCriteria(timeBetweenRenewing, timeBetweenAquiring);
            return new LeaseManagerConfiguration(managerFunc, calculatorFunc(), leaseCriteria);
        }
    }
}