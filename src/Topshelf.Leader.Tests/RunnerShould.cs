using System;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Xunit;

namespace Topshelf.Leader.Tests
{
    public class RunnerShould
    {
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
            var manager = A.Fake<ILeadershipManager>();
            A.CallTo(() => manager.AcquireLock(A<string>.Ignored, A<CancellationToken>.Ignored)).Returns(false);

            var config = new LeaderConfigurationBuilder<ITestService>()
                .WhenStopping(new CancellationTokenSource(1000))
                .AttemptToBeTheLeaderEvery(TimeSpan.FromMilliseconds(100))
                .WhenStarted(async (svc, token) =>
                {
                    await service.Start(token);
                })
                .WithLeadershipManager(manager)
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
            var manager = A.Fake<ILeadershipManager>();
            A.CallTo(() => manager.AcquireLock(A<string>.Ignored, A<CancellationToken>.Ignored)).Returns(Task.FromResult(true));
            A.CallTo(() => manager.RenewLock(A<string>.Ignored, A<CancellationToken>.Ignored)).Returns(Task.FromResult(true));

            var config = new LeaderConfigurationBuilder<object>()
                .WhenStopping(new CancellationTokenSource(1000))
                .UpdateLeaseEvery(TimeSpan.FromMilliseconds(50))
                .WhenStarted(async (svc, token) =>
                {
                    await service.Start(token);
                })
                .WithLeadershipManager(manager)
                .Build();

            var runner = new Runner<object>(new object(), config);

            await runner.Start();

            A.CallTo(() => service.Start(A<CancellationToken>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
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

        [Fact]
        public async Task should_attempt_to_obtain_a_lease()
        {
            var service = BuildBlockingTestService();
            var manager = A.Fake<ILeadershipManager>();
            A.CallTo(() => manager.AcquireLock(A<string>.Ignored, A<CancellationToken>.Ignored)).Returns(Task.FromResult(true));
            A.CallTo(() => manager.RenewLock(A<string>.Ignored, A<CancellationToken>.Ignored)).Returns(Task.FromResult(true));

            var config = new LeaderConfigurationBuilder<object>()
                .WhenStopping(new CancellationTokenSource(1000))
                .UpdateLeaseEvery(TimeSpan.FromMilliseconds(50))
                .WhenStarted(async (svc, token) =>
                {
                    await service.Start(token);
                })
                .WithLeadershipManager(manager)
                .Build();

            var runner = new Runner<object>(new object(), config);

            await runner.Start();
            A.CallTo(() => manager.AcquireLock(A<string>.Ignored, A<CancellationToken>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async Task should_attempt_to_obtain_a_lease_again_if_the_lease_could_not_be_renewed()
        {
            var service = BuildBlockingTestService();
            var manager = A.Fake<ILeadershipManager>();
            A.CallTo(() => manager.AcquireLock(A<string>.Ignored, A<CancellationToken>.Ignored)).Returns(Task.FromResult(true));
            A.CallTo(() => manager.RenewLock(A<string>.Ignored, A<CancellationToken>.Ignored)).Returns(Task.FromResult(false));

            var config = new LeaderConfigurationBuilder<object>()
                .WhenStopping(new CancellationTokenSource(1000))
                .UpdateLeaseEvery(TimeSpan.FromMilliseconds(50))
                .WhenStarted(async (svc, token) =>
                {
                    await service.Start(token);
                })
                .WithLeadershipManager(manager)
                .Build();

            var runner = new Runner<object>(new object(), config);

            await runner.Start();
            A.CallTo(() => manager.AcquireLock(A<string>.Ignored, A<CancellationToken>.Ignored)).MustHaveHappened(Repeated.AtLeast.Twice);
        }

        [Fact]
        public async Task should_attempt_to_renew_a_lease_once_it_has_obtained_one()
        {

        }
    }
}