﻿using System;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Topshelf.HostConfigurators;
using Topshelf.Hosts;
using Topshelf.Leader.InMemory;
using Topshelf.Leader.Tests.Services;
using Topshelf.Runtime.Windows;
using Topshelf.ServiceConfigurators;
using Xunit;

namespace Topshelf.Leader.Tests
{
    public class TopShelfLeaderExtensionShould
    {
        [Fact]
        public void use_the_in_memory_lease_manager_when_an_alternative_manager_isnt_configured()
        {
            var builder = new LeaderConfigurationBuilder<object>();
            builder.WhenStarted((o, token) => {  Console.WriteLine("Started"); return Task.CompletedTask; });

            var built = builder.Build();

            Assert.IsType<InMemoryLeaseManager>(built.LeaseManager);
        }

        [Fact]
        public void hook_into_the_before_shutdown_to_stop_the_runloop()
        {
            var testService = new TestServicewithStopSupport();
            var stopRequestedSource = new CancellationTokenSource();
            var host = BuildTestHost(
                testService,
                builder =>
                {
                    builder.WhenStarted(async (service, token) => await service.Start(token));
                    builder.WhenStopping(stopRequestedSource);
                });

            host.Run();

            Assert.True(stopRequestedSource.IsCancellationRequested);
        }

        [Fact]
        public void release_the_lease_when_the_service_is_shutting_down()
        {
            const string nodeId = "theNodeId";
            var manager = A.Fake<ILeaseManager>();
            var testService = new TestServicewithStopSupport();
            var stopRequestedSource = new CancellationTokenSource();
            var host = BuildTestHost(
                testService,
                builder =>
                {
                    builder.SetNodeId(nodeId);
                    builder.WhenStarted(async (service, token) => await service.Start(token));
                    builder.WhenStopping(stopRequestedSource);
                    builder.Lease(lcb => lcb.WithLeaseManager(manager));
                });

            host.Run();

            A.CallTo(() => manager.ReleaseLease(A<LeaseReleaseOptions>.Ignored)).MustHaveHappened(1, Times.Exactly);
        }

        [Fact]
        public void start_the_service_when_required()
        {
            var testService = new TestServicewithStopSupport();
            var host = BuildTestHost(testService);

            host.Run();

            Assert.True(testService.Started);
        }

        [Fact]
        public void stop_the_service_when_required()
        {
            var testService = new TestServicewithStopSupport();
            var host = BuildTestHost(testService);

            host.Run();

            Assert.True(testService.Stopped);
        }

        private static TestHost BuildTestHost(TestServicewithStopSupport servicewithStopSupport)
        {
            return BuildTestHost(
                servicewithStopSupport,
                builder =>
                {
                    builder.WhenStarted(async (srvc, token) => { await srvc.Start(token); });
                }
            );
        }

        private static TestHost BuildTestHost(TestServicewithStopSupport servicewithStopSupport, Action<LeaderConfigurationBuilder<TestServicewithStopSupport>> builder)
        {
            var serviceConfigurator = new DelegateServiceConfigurator<TestServicewithStopSupport>();
            serviceConfigurator.ConstructUsing(() => servicewithStopSupport);
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