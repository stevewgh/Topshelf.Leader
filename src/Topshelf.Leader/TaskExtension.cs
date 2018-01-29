using System;
using System.Threading;
using System.Threading.Tasks;

namespace Topshelf.Leader
{
    public static class TaskExtension
    {
        public static async Task DelayWithoutTimeoutException(TimeSpan timeout, CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(timeout, cancellationToken);
            }
            catch (TaskCanceledException)
            {
            }
        }
    }
}