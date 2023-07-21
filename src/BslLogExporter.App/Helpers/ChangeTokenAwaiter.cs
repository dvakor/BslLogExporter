using Microsoft.Extensions.Primitives;

namespace LogExporter.App.Helpers;

public sealed class ChangeTokenAwaiter : IAsyncDisposable
{
    private const int DefaultPollInterval = 100;
    
    private readonly CancellationTokenSource _cts;
    private readonly Task _watcher;

    private ChangeTokenAwaiter(IChangeToken changeToken, int pollInterval, CancellationToken token = default)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(token);
        
        _watcher = Task.Run(async () =>
        {
            try
            {
                while (!changeToken.HasChanged && !_cts.IsCancellationRequested)
                {
                    await Task.Delay(pollInterval, token);
                }
            }
            finally
            {
                _cts.Cancel();
            }
        }, token);
    }

    public static async Task WaitForTokenChange(IChangeToken changeToken, 
        int pollInterval = DefaultPollInterval, CancellationToken token = default)
    {
        await using var tokenAwaiter = Create(changeToken, pollInterval, token);
        await tokenAwaiter.Wait();
    }

    public static ChangeTokenAwaiter Create(IChangeToken changeToken, 
        int pollInterval = DefaultPollInterval, CancellationToken token = default)
    {
        return new ChangeTokenAwaiter(changeToken, pollInterval, token);
    }

    public CancellationToken Token => _cts.Token;

    public Task Wait()
    {
        return _cts.Token.WaitForCancellation();
    }
    
    public async ValueTask DisposeAsync()
    {
        _cts.Cancel();
        
        try
        {
            await _watcher;
        }
        catch
        {
            //NoOp
        }
        
        _watcher.Dispose();
        _cts.Dispose();
    }
}