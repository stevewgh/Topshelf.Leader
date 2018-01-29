using Xunit;

namespace Topshelf.Leader.Tests
{
    public class LeaderConfigurationBuilderShould
    {
        [Fact]
        public void prevent_a_leader_configuration_which_doesnt_handle_starting_up()
        {
            var builder = new LeaderConfigurationBuilder<object>();
            Assert.Throws<HostConfigurationException>(() => builder.Build());
        }
    }
}
