using System.Diagnostics;
using Microsoft.Extensions.Primitives;

namespace LogExporter.App.Helpers;

public class PollingChangeToken : IChangeToken
{
    private readonly Func<bool> _pollingAction;
    private readonly int _pollingPeriod;
    private readonly Stopwatch _stopwatch;
    private bool _lastResult;
    
    public bool HasChanged => HasChangedInner();
    
    public bool ActiveChangeCallbacks => false;

    public PollingChangeToken(Func<bool> pollingAction, int pollingPeriod = 100)
    {
        _pollingAction = pollingAction;
        _pollingPeriod = pollingPeriod;
        _stopwatch = new Stopwatch();
        _stopwatch.Start();
    }
    
    public IDisposable RegisterChangeCallback(Action<object?> callback, object? state)
    {
        throw new NotImplementedException();
    }
    
    private bool HasChangedInner()
    {
        if (_stopwatch.ElapsedMilliseconds < _pollingPeriod)
        {
            return _lastResult;
        }

        _lastResult = _pollingAction();

        if (_lastResult)
        {
            _stopwatch.Restart();
        }
        else
        {
            _stopwatch.Stop();
        }

        return _lastResult;
    }
}