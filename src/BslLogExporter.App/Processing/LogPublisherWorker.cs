using LogExporter.App.Helpers;
using LogExporter.App.Sources;

namespace LogExporter.App.Processing;

public class LogPublisherWorker
{
    private readonly ILogSource _source;
    private readonly LogsBuffer _buffer;

    public LogPublisherWorker(ILogSource source, LogsBuffer buffer)
    {
        _source = source;
        _buffer = buffer;
    }

    public async Task Start(CancellationToken token)
    {
        var throttler = new Throttler(3, TimeSpan.FromSeconds(1));
        
        while (!token.IsCancellationRequested)
        {
            var readCount = 0;
            
            foreach (var logEntry in _source.GetLogs(token))
            {
                readCount += 1;

                await _buffer.AddAsync(_source.Name, logEntry);

                if (readCount >= _buffer.BufferSize)
                {
                    break;
                }
            }

            await throttler.Throttle(token);
        }
    }
}