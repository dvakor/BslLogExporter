using System.Runtime.CompilerServices;
using System.Threading.Channels;
using LogExporter.Core.LogReader;
using Microsoft.Extensions.Options;

namespace LogExporter.App.Processing;

public class LogsBuffer
{
    private readonly ProcessingConfiguration _options;
    private readonly Channel<SourceLog> _buffer;

    public int BufferSize => _options.BufferSize;

    public LogsBuffer(IOptions<ProcessingConfiguration> options)
    {
        _options = options.Value;
        
        _buffer = Channel.CreateBounded<SourceLog>(new BoundedChannelOptions(BufferSize)
        {
            SingleReader = false,
            SingleWriter = false,
            AllowSynchronousContinuations = true,
            FullMode = BoundedChannelFullMode.Wait
        });
    }
    
    public async ValueTask AddAsync(string source, LogEntry entry)
    {
        await _buffer.Writer.WriteAsync(new SourceLog
        {
            SourceName = source,
            Entry = entry
        });
    }

    public async IAsyncEnumerable<SourceLogPortion> GetLogsAsync([EnumeratorCancellation] CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(_options.BufferTimeoutSeconds));
            using var combined = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, token);

            var dict = await ReadLogsInnerAsync(combined.Token);

            var results = dict.Select(x => new SourceLogPortion
            {
                SourceName = x.Key,
                Entries = x.Value
            });

            foreach (var result in results)
            {
                yield return result;
            }
        }
    }

    private async Task<Dictionary<string, List<LogEntry>>> ReadLogsInnerAsync(CancellationToken token)
    {
        var dict = new Dictionary<string, List<LogEntry>>();
        
        try
        {
            var entriesCount = 0;

            await foreach (var log in _buffer.Reader.ReadAllAsync(token))
            {
                if (!dict.ContainsKey(log.SourceName))
                {
                    dict[log.SourceName] = new List<LogEntry>();
                }

                dict[log.SourceName].Add(log.Entry);

                entriesCount++;

                if (entriesCount >= BufferSize)
                {
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // NoOp
        }

        return dict;
    }

    private record SourceLog
    {
        public string SourceName { get; init; } = default!;
    
        public LogEntry Entry { get; init; } = default!;
    }
        
}