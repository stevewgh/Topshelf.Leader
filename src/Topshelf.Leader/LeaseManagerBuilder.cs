using System;
using Topshelf.Leader.InMemory;

namespace Topshelf.Leader
{
    public class LeaseManagerBuilder
    {
        public string NodeId { get; }
        private readonly LeaseCriteria criteria;
        private Func<LeaseCriteria, ILeaseManager> managerFunc;

        public LeaseManagerBuilder(string nodeId, LeaseCriteria criteria)
        {
            NodeId = nodeId;
            this.criteria = criteria;
            managerFunc = c => new InMemoryLeaseManager(nodeId);
        }

        public LeaseManagerBuilder Factory(Func<LeaseCriteria,ILeaseManager> manager)
        {
            managerFunc = manager ?? throw new ArgumentNullException(nameof(manager));
            return this;
        }

        public ILeaseManager Build()
        {
            return managerFunc(criteria);
        }
    }
}