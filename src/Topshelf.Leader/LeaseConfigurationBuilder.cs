using System;
using Topshelf.Leader.InMemory;

namespace Topshelf.Leader
{
    public class LeaseConfigurationBuilder
    {
        internal static readonly TimeSpan DefaultTimeBetweenRenewing = TimeSpan.FromSeconds(5);
        internal static readonly TimeSpan DefaultTimeBetweenAquiring = TimeSpan.FromSeconds(60);

        public string NodeId { get; }
        private Func<LeaseConfiguration,ILeaseManager> managerFunc;
        private Func<ILeaseLengthCalculator> calculatorFunc = () => new StubLeaseLengthCalculator(DefaultTimeBetweenRenewing);
        private TimeSpan timeBetweenRenewing = DefaultTimeBetweenRenewing;
        private TimeSpan timeBetweenAquiring = DefaultTimeBetweenAquiring;

        public LeaseConfigurationBuilder(string nodeId)
        {
            NodeId = nodeId;
            managerFunc = c => new InMemoryLeaseManager(nodeId);
        }

        public LeaseConfigurationBuilder RenewLeaseEvery(TimeSpan time)
        {
            if (time <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(time), "Must be greater than zero.");
            }

            timeBetweenRenewing = time;

            return this;
        }

        public LeaseConfigurationBuilder AquireLeaseEvery(TimeSpan time)
        {
            if (time <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(time), "Must be greater than zero.");
            }

            timeBetweenAquiring = time;

            return this;
        }

        public LeaseConfigurationBuilder LeaseLength(Func<ILeaseLengthCalculator> calculator)
        {
            calculatorFunc = calculator ?? throw new ArgumentNullException(nameof(calculator));
            return this;
        }

        public LeaseConfigurationBuilder LeaseLength(TimeSpan length)
        {
            if (length <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "Must be greater than zero.");
            }
            calculatorFunc = () => new StubLeaseLengthCalculator(length);
            return this;
        }

        public LeaseConfigurationBuilder WithLeaseManager(Func<LeaseConfiguration,ILeaseManager> manager)
        {
            managerFunc = manager ?? throw new ArgumentNullException(nameof(manager));
            return this;
        }

        public LeaseConfigurationBuilder WithLeaseManager(ILeaseManager manager)
        {
            if (manager == null)
            {
                throw new ArgumentNullException(nameof(manager));
            }

            managerFunc = c => manager;
            return this;
        }

        public LeaseConfiguration Build()
        {
            if (timeBetweenAquiring <= timeBetweenRenewing)
            {
                throw new HostConfigurationException($"{nameof(AquireLeaseEvery)} must be greater than {nameof(RenewLeaseEvery)}.");
            }

            var leaseCriteria = new LeaseCriteria(timeBetweenRenewing, timeBetweenAquiring);
            return new LeaseConfiguration(NodeId, managerFunc, calculatorFunc(), leaseCriteria);
        }
    }
}