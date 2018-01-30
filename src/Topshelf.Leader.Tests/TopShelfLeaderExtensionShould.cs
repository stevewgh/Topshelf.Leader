using System;
using System.Threading;
using Topshelf.HostConfigurators;
using Topshelf.Hosts;
using Topshelf.Runtime.Windows;
using Topshelf.ServiceConfigurators;
using Xunit;

namespace Topshelf.Leader.Tests
{
    public class TopShelfLeaderExtensionShould
    {
        [Fact]
        public void hook_into_the_before_shutdown_to_stop_the_runloop()
        {
            var testService = new TestService();
            var stopRequestedSource = new CancellationTokenSource();
            var host = BuildTestHost(
                testService,
                builder =>
                {
                    builder.WhenStarted(async (service, token) => await service.Start(token));
                    builder.WhenServiceIsStopping(stopRequestedSource);
                });

            host.Run();

            Assert.True(stopRequestedSource.IsCancellationRequested);
        }

        [Fact]
        public void start_the_service_when_required()
        {
            var testService = new TestService();
            var stopRequestedSource = new CancellationTokenSource();
            var host = BuildTestHost(
                testService,
                builder =>
                {
                    builder.WhenStarted(async (service, token) => await service.Start(token));
                    builder.WhenServiceIsStopping(stopRequestedSource);
                });

            host.Run();

            Assert.True(testService.Started);
        }

        [Fact]
        public void stop_the_service_when_required()
        {
            var testService = new TestService();
            var stopRequestedSource = new CancellationTokenSource();
            var host = BuildTestHost(
                testService,
                builder =>
                {
                    builder.WhenStarted(async (service, token) => await service.Start(token));
                    builder.WhenServiceIsStopping(stopRequestedSource);
                });

            host.Run();

            Assert.True(testService.Stopped);
        }

        private static TestHost BuildTestHost(TestService service, Action<LeaderConfigurationBuilder<TestService>> builder)
        {
            var serviceConfigurator = new DelegateServiceConfigurator<TestService>();
            serviceConfigurator.ConstructUsing(() => service);
            serviceConfigurator.WhenStartedAsLeader(builder);
            serviceConfigurator.WhenStopped((testService, control) =>
            {
                testService.Stop();
                return true;
            });

            var host = new TestHost(
                new WindowsHostSettings(),
                new WindowsHostEnvironment(new HostConfiguratorImpl()),
                serviceConfigurator.Build().Build(new WindowsHostSettings()));
            return host;
        }
    }
}