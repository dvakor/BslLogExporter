namespace LogExporter.App.Helpers;

public static class Extensions
{
    public static async Task WaitForCancellation(this CancellationToken token)
    {
        var tcs = new TaskCompletionSource();
        await using (_ = token.Register(_ => tcs.SetResult(), null))
        {
            await tcs.Task;
        }
    }
}