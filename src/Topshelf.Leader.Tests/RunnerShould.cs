using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Topshelf.Leader.Tests.Services;
using Xunit;
using Xunit.Abstractions;

namespace Topshelf.Leader.Tests
{
    public class RunnerShould
    {
        ITestOutputHelper output;

        public RunnerShould(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public async Task stop_running_when_the_service_is_stopping()
        {
            var service = new TestService();
            var stopping = new CancellationTokenSource();
            var config = new LeaderConfigurationBuilder<TestService>()
                .WhenStopping(stopping)
                .WhenStarted(async (testService, token) => await testService.Start(token))
                .Build();

            var runner = new Runner<TestService>(service, config);
            var startTask = runner.Start();

            stopping.Cancel();
            await startTask;

            Assert.True(startTask.IsCompleted);
        }

        [Fact]
        public async Task should_block_indefinitely_if_a_leadership_lease_can_not_be_obtained()
        {
            var service = A.Fake<ITestService>();
            var manager = A.Fake<ILeaseManager>();
            A.CallTo(() => manager.AcquireLease(A<LeaseOptions>.Ignored, A<CancellationToken>.Ignored)).Returns(false);

            var config = new LeaderConfigurationBuilder<ITestService>()
                .WhenStopping(new CancellationTokenSource(1000))
                .Lease(lcb =>
                    {
                        lcb.RenewLeaseEvery(TimeSpan.FromMilliseconds(50))
                           .AquireLeaseEvery(TimeSpan.FromMilliseconds(100))
                           .WithLeaseManager(lc => manager);
                    })
                .WhenStarted(async (svc, token) =>
                {
                    await svc.Start(token);
                })
                .Build();

            var runner = new Runner<ITestService>(service, config);

            await runner.Start();

            A.CallTo(() => service.Start(A<CancellationToken>.Ignored)).MustHaveHappened(Repeated.Never);
            A.CallTo(manager).MustHaveHappened(Repeated.AtLeast.Once);
        }

        [Fact]
        public async Task should_start_if_a_leadership_lease_is_obtained()
        {
            var service = BuildBlockingTestService();
            var manager = A.Fake<ILeaseManager>();
            A.CallTo(() => manager.AcquireLease(A<LeaseOptions>.Ignored, A<CancellationToken>.Ignored)).Returns(Task.FromResult(true));
            A.CallTo(() => manager.RenewLease(A<LeaseOptions>.Ignored, A<CancellationToken>.Ignored)).Returns(Task.FromResult(true));

            var config = new LeaderConfigurationBuilder<ITestService>()
                .WhenStopping(new CancellationTokenSource(1000))
                .Lease(lcb => {lcb.RenewLeaseEvery(TimeSpan.FromMilliseconds(50)).WithLeaseManager(manager);})
                .WhenStarted(async (svc, token) =>
                {
                    await svc.Start(token);
                })
                .Build();

            var runner = new Runner<ITestService>(service, config);

            await runner.Start();

            A.CallTo(() => service.Start(A<CancellationToken>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async Task should_attempt_to_obtain_a_lease()
        {
            var service = BuildBlockingTestService();
            var manager = A.Fake<ILeaseManager>();
            A.CallTo(() => manager.AcquireLease(A<LeaseOptions>.Ignored, A<CancellationToken>.Ignored)).Returns(Task.FromResult(true));
            A.CallTo(() => manager.RenewLease(A<LeaseOptions>.Ignored, A<CancellationToken>.Ignored)).Returns(Task.FromResult(true));

            var config = new LeaderConfigurationBuilder<ITestService>()
                .WhenStopping(new CancellationTokenSource(1000))
                .Lease(lcb => { lcb.RenewLeaseEvery(TimeSpan.FromMilliseconds(50)).WithLeaseManager(manager); })
                .WhenStarted(async (svc, token) =>
                {
                    await svc.Start(token);
                })
                .Build();

            var runner = new Runner<ITestService>(service, config);

            await runner.Start();
            A.CallTo(() => manager.AcquireLease(A<LeaseOptions>.Ignored, A<CancellationToken>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task should_invoke_the_heartbeat_at_set_intervals(bool isLeader)
        {
            var service = BuildBlockingTestService();
            var manager = A.Fake<ILeaseManager>();
            const int milliSecondsBeforeServiceStops = 2000;
            const int milliSecondsBetweenHeartBeats = 200;
            var invocationTimes = new List<DateTime>();
            A.CallTo(() => manager.AcquireLease(A<LeaseOptions>.Ignored, A<CancellationToken>.Ignored)).Returns(Task.FromResult(isLeader));
            A.CallTo(() => manager.RenewLease(A<LeaseOptions>.Ignored, A<CancellationToken>.Ignored)).Returns(Task.FromResult(isLeader));

            var config = new LeaderConfigurationBuilder<ITestService>()
                .WhenStopping(new CancellationTokenSource(milliSecondsBeforeServiceStops))
                .Lease(lcb => lcb.WithLeaseManager(manager))
                .WhenStarted(async (svc, token) =>
                {
                    await svc.Start(token);
                })
                .WithHeartBeat(
                    TimeSpan.FromMilliseconds(milliSecondsBetweenHeartBeats), 
                    (b, token) => 
                    {
                        invocationTimes.Add(DateTime.UtcNow);
                        return Task.CompletedTask;
                    })
                .Build();

            var runner = new Runner<ITestService>(service, config);

            await runner.Start();

            for (var i = 0; i < invocationTimes.Count; i++)
            {
                if (i == 0)
                {
                    this.output.WriteLine($"Invocation time: {invocationTimes[i]:HH:mm:ss.fff}");
                    continue;
                }

                var invocationTime = invocationTimes[i];
                var millisecondsDifferenceBetweenInvocations = invocationTime.Subtract(invocationTimes[i - 1]).TotalMilliseconds;
                this.output.WriteLine($"Invocation time: {invocationTime:HH:mm:ss.fff}.  Difference between previous invocation (ms): {millisecondsDifferenceBetweenInvocations}.");
                Assert.InRange(
                    millisecondsDifferenceBetweenInvocations,
                    milliSecondsBetweenHeartBeats * 0.9,
                    milliSecondsBetweenHeartBeats * 1.1);
            }

            var expectedNumberOfInvocations = milliSecondsBeforeServiceStops / milliSecondsBetweenHeartBeats;
            this.output.WriteLine($"Actual no. of invocations: {invocationTimes.Count}. Expected no. of invocations: {expectedNumberOfInvocations}.");
            Assert.Equal(invocationTimes.Count, expectedNumberOfInvocations);
        }

        [Fact]
        public async Task should_attempt_to_obtain_a_lease_again_if_the_lease_could_not_be_renewed()
        {
            var service = BuildBlockingTestService();
            var manager = A.Fake<ILeaseManager>();
            A.CallTo(() => manager.AcquireLease(A<LeaseOptions>.Ignored, A<CancellationToken>.Ignored)).Returns(Task.FromResult(true));
            A.CallTo(() => manager.RenewLease(A<LeaseOptions>.Ignored, A<CancellationToken>.Ignored)).Returns(Task.FromResult(false));

            var config = new LeaderConfigurationBuilder<ITestService>()
                .WhenStopping(new CancellationTokenSource(1000))
                .Lease(lcb => { lcb.RenewLeaseEvery(TimeSpan.FromMilliseconds(50)).WithLeaseManager(manager); })
                .WhenStarted(async (svc, token) => { await svc.Start(token); })
                .Build();

            var runner = new Runner<ITestService>(service, config);

            await runner.Start();
            A.CallTo(() => manager.AcquireLease(A<LeaseOptions>.Ignored, A<CancellationToken>.Ignored)).MustHaveHappened(Repeated.AtLeast.Twice);
        }

        [Fact]
        public async Task should_attempt_to_renew_a_lease_once_it_has_obtained_one()
        {
            var service = BuildBlockingTestService();
            var manager = A.Fake<ILeaseManager>();
            A.CallTo(() => manager.AcquireLease(A<LeaseOptions>.Ignored, A<CancellationToken>.Ignored)).Returns(Task.FromResult(true));
            A.CallTo(() => manager.RenewLease(A<LeaseOptions>.Ignored, A<CancellationToken>.Ignored)).Returns(Task.FromResult(true));

            var config = new LeaderConfigurationBuilder<ITestService>()
                .WhenStopping(new CancellationTokenSource(1000))
                .Lease(lcb => { lcb.RenewLeaseEvery(TimeSpan.FromMilliseconds(50)).WithLeaseManager(manager);})
                .WhenStarted(async (svc, token) =>
                {
                    await svc.Start(token);
                })
                .Build();

            var runner = new Runner<ITestService>(service, config);

            await runner.Start();
            A.CallTo(() => manager.RenewLease(A<LeaseOptions>.Ignored, A<CancellationToken>.Ignored)).MustHaveHappened(Repeated.AtLeast.Twice);
        }

        [Fact]
        public async Task should_bubble_exceptions_in_the_service()
        {
            var exception = new Exception();
            var service = BuildBadTestService(exception);
            var manager = A.Fake<ILeaseManager>();
            A.CallTo(() => manager.AcquireLease(A<LeaseOptions>.Ignored, A<CancellationToken>.Ignored)).Returns(Task.FromResult(true));

            var config = new LeaderConfigurationBuilder<ITestService>()
                .WhenStarted(async (svc, token) =>
                {
                    await svc.Start(token);
                })
                .Lease(lcb => lcb.WithLeaseManager(manager))
                .Build();

            var thrownException = await Assert.ThrowsAsync<AggregateException>(async () => await new Runner<ITestService>(service, config).Start());
            Assert.Contains(exception, thrownException.InnerExceptions);
        }

        [Fact]
        public async Task should_bubble_exceptions_in_the_leadershipmanager()
        {
            var exception = new Exception();
            var service = BuildBlockingTestService();
            var manager = A.Fake<ILeaseManager>();
            A.CallTo(() => manager.AcquireLease(A<LeaseOptions>.Ignored, A<CancellationToken>.Ignored)).Returns(Task.FromResult(true));
            A.CallTo(() => manager.RenewLease(A<LeaseOptions>.Ignored, A<CancellationToken>.Ignored)).ThrowsAsync(exception);

            var config = new LeaderConfigurationBuilder<ITestService>()
                .WhenStarted(async (svc, token) =>
                {
                    await svc.Start(token);
                })
                .Lease(lcb => lcb.WithLeaseManager(manager))
                .Build();

            var thrownException = await Assert.ThrowsAsync<AggregateException>(async () => await new Runner<ITestService>(service, config).Start());
            Assert.Same(exception, thrownException.InnerExceptions.First());
        }

        [Fact]
        public async Task should_bubble_exceptions_in_the_heartbeat()
        {
            var exception = new Exception("Error on heartbeat");
            var service = BuildBlockingTestService();
            var manager = A.Fake<ILeaseManager>();
            A.CallTo(() => manager.AcquireLease(A<LeaseOptions>.Ignored, A<CancellationToken>.Ignored)).Returns(Task.FromResult(true));
            A.CallTo(() => manager.RenewLease(A<LeaseOptions>.Ignored, A<CancellationToken>.Ignored)).Returns(Task.FromResult(true));

            var config = new LeaderConfigurationBuilder<ITestService>()
                .WhenStarted(async (svc, token) =>
                {
                    await svc.Start(token);
                })
                .Lease(lcb => lcb.WithLeaseManager(manager))
                .WithHeartBeat(TimeSpan.Zero, (b, token) => throw exception)
                .Build();

            var thrownException = await Assert.ThrowsAsync<AggregateException>(async () => await new Runner<ITestService>(service, config).Start());
            Assert.Same(exception, thrownException.InnerExceptions.First());
        }

        [Fact]
        public async Task should_bubble_exceptions_if_the_service_and_leadershipmanager_throw_exceptions()
        {
            var serviceException = new Exception("Service stopped working");
            var leadershipManagerException = new Exception("Leadership manager stopped working");
            var service = BuildBadTestService(serviceException);
            var manager = A.Fake<ILeaseManager>();
            A.CallTo(() => manager.AcquireLease(A<LeaseOptions>.Ignored, A<CancellationToken>.Ignored)).Returns(Task.FromResult(true));
            A.CallTo(() => manager.RenewLease(A<LeaseOptions>.Ignored, A<CancellationToken>.Ignored)).ThrowsAsync(leadershipManagerException);

            var config = new LeaderConfigurationBuilder<ITestService>()
                .WhenStarted(async (svc, token) =>
                {
                    await svc.Start(token);
                })
                .Lease(lcb => lcb.WithLeaseManager(manager))
                .Build();

            var thrownException = await Assert.ThrowsAsync<AggregateException>(async () => await new Runner<ITestService>(service, config).Start());
            Assert.Contains(serviceException, thrownException.InnerExceptions);
            Assert.Contains(leadershipManagerException, thrownException.InnerExceptions);
        }

        [Fact]
        public async Task should_set_the_cancellation_token_when_unhandled_exceptions_occur_in_the_service()
        {
            var exception = new Exception();
            var service = BuildBadTestService(exception);
            var manager = A.Fake<ILeaseManager>();
            A.CallTo(() => manager.AcquireLease(A<LeaseOptions>.Ignored, A<CancellationToken>.Ignored)).Returns(Task.FromResult(true));

            var serviceStopping = new CancellationTokenSource();
            var config = new LeaderConfigurationBuilder<ITestService>()
                .WhenStopping(serviceStopping)
                .WhenStarted(async (svc, token) =>
                {
                    await svc.Start(token);
                })
                .Lease(lcb => lcb.WithLeaseManager(manager))
                .Build();

            await Assert.ThrowsAnyAsync<AggregateException>(async () => await new Runner<ITestService>(service, config).Start());
            Assert.True(serviceStopping.IsCancellationRequested);
        }

        [Fact]
        public async Task should_set_the_cancellation_token_when_unhandled_exceptions_occur_in_the_heartbeat()
        {
            var exception = new Exception("Error in heartbeat");
            var service = BuildBlockingTestService();
            var manager = A.Fake<ILeaseManager>();
            A.CallTo(() => manager.AcquireLease(A<LeaseOptions>.Ignored, A<CancellationToken>.Ignored)).Returns(Task.FromResult(true));

            var serviceStopping = new CancellationTokenSource();
            var config = new LeaderConfigurationBuilder<ITestService>()
                .WhenStopping(serviceStopping)
                .WhenStarted(async (svc, token) =>
                {
                    await svc.Start(token);
                })
                .Lease(lcb => lcb.WithLeaseManager(manager))
                .WithHeartBeat(TimeSpan.Zero, (b, token) => throw exception)
                .Build();

            await Assert.ThrowsAnyAsync<AggregateException>(async () => await new Runner<ITestService>(service, config).Start());
            Assert.True(serviceStopping.IsCancellationRequested);
        }

        private static ITestService BuildBlockingTestService()
        {
            var service = A.Fake<ITestService>();
            CancellationToken noLongerLeaderCancellationToken;
            A.CallTo(() => service.Start(A<CancellationToken>.Ignored))
                .Invokes(call => noLongerLeaderCancellationToken = call.Arguments.Get<CancellationToken>(0))
                .Returns(Task.Delay(10000, noLongerLeaderCancellationToken));
            return service;
        }

        private static ITestService BuildBadTestService(Exception exception)
        {
            var service = A.Fake<ITestService>();
            A.CallTo(() => service.Start(A<CancellationToken>.Ignored)).ThrowsAsync(exception);
            return service;
        }
    }
}