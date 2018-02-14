namespace Topshelf.Leader.InMemory
{
    public static class LeaseConfigurationBuilderInMemoryExtensions
    {
        public static LeaseConfigurationBuilder WithInMemoryLeaseManager(this LeaseConfigurationBuilder builder)
        {
            return builder.WithLeaseManager(lc => new InMemoryLeaseManager(lc.NodeId));
        }
    }
}