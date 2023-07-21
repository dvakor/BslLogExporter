namespace LogExporter.App.Helpers;

public sealed class Throttler
{
    private readonly int _maxCalls;
    private readonly TimeSpan _period;

    private DateTime _currentPeriod;
    private int _currentCallTimes;
    
    public Throttler(int maxCalls, TimeSpan period)
    {
        _maxCalls = maxCalls;
        _period = period;
        _currentPeriod = DateTime.Now;
    }

    public async ValueTask Throttle(CancellationToken token)
    {
        var now = DateTime.Now;

        if (_currentPeriod < now)
        {
            _currentPeriod = now.Add(_period);
            _currentCallTimes = 1;
            
            return;
        }
        
        _currentCallTimes += 1;

        if (_currentCallTimes >= _maxCalls)
        {
            var randomFactor = TimeSpan.FromMilliseconds(Random.Shared.Next(100, 1000));
            var waitPeriod = (_currentPeriod - now).Add(randomFactor);

            try
            {
                await Task.Delay(waitPeriod, token);
            }
            catch
            {
                // NoOp
            }
        }
    }
}