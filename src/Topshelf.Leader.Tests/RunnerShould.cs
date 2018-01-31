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
            var manager = A.Fake<ILeadershipManager>();
            A.CallTo(() => manager.AcquireLock(A<string>.Ignored, A<CancellationToken>.Ignored)).Returns(false);
            var started = false;

            var config = new LeaderConfigurationBuilder<object>()
                .WhenStopping(new CancellationTokenSource(1000))
                .AttemptToBeTheLeaderEvery(TimeSpan.FromMilliseconds(100))
                .WhenStarted((svc, token) =>
                {
                    started = true;
                    return Task.CompletedTask;
                })
                .WithLeadershipManager(manager)
                .Build();

            var runner = new Runner<object>(new object(), config);

            await runner.Start();

            Assert.False(started);
            A.CallTo(manager).MustHaveHappened(Repeated.AtLeast.Once);
        }

        [Fact]
        public async Task should_start_if_a_leadership_lease_is_obtained()
        {
            var manager = A.Fake<ILeadershipManager>();
            A.CallTo(() => manager.AcquireLock(A<string>.Ignored, A<CancellationToken>.Ignored)).Returns(true);
            A.CallTo(() => manager.RenewLock(A<string>.Ignored, A<CancellationToken>.Ignored)).Returns(true);
            var started = false;

            var config = new LeaderConfigurationBuilder<object>()
                .WhenStopping(new CancellationTokenSource(1000))
                .UpdateLeaseEvery(TimeSpan.FromMilliseconds(50))
                .WhenStarted((svc, token) =>
                {
                    started = true;
                    return Task.CompletedTask;
                })
                .WithLeadershipManager(manager)
                .Build();

            var runner = new Runner<object>(new object(), config);

            await runner.Start();

            Assert.True(started);
            A.CallTo(() => manager.AcquireLock(A<string>.Ignored, A<CancellationToken>.Ignored)).MustHaveHappened(Repeated.NoMoreThan.Once);
            A.CallTo(() => manager.RenewLock(A<string>.Ignored, A<CancellationToken>.Ignored)).MustHaveHappened(Repeated.AtLeast.Twice);
        }
    }
}