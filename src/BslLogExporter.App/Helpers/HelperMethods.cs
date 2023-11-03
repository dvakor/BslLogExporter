using Polly;

namespace LogExporter.App.Helpers;

public static class HelperMethods
{
    public static async Task WaitForCancellation(this CancellationToken token)
    {
        var tcs = new TaskCompletionSource();
        await using (_ = token.Register(_ => tcs.SetResult(), null))
        {
            await tcs.Task;
        }
    }
    
    public static Policy CreateRetryPolicy<TException>(int retryTimes, int multiplier = 100) where TException : Exception
    {
        return Policy
            .Handle<TException>()
            .WaitAndRetry(retryTimes, n
                => TimeSpan.FromMilliseconds(multiplier * n));
    }
    public static AsyncPolicy CreateRetryAsyncPolicy<TException>(int retryTimes, int multiplier = 100) where TException : Exception
    {
        return Policy
            .Handle<TException>()
            .WaitAndRetryAsync(retryTimes, n
                => TimeSpan.FromMilliseconds(multiplier * n));
    }

    public static AsyncPolicy CreateInfinityRetryPolicy<TException>(int multiplier, Action<Exception, TimeSpan> onRetry) where TException : Exception
    {
        return Policy
            .Handle<TException>()
            .WaitAndRetryForeverAsync(n 
                => TimeSpan.FromMilliseconds(multiplier * n), onRetry);
    }
}