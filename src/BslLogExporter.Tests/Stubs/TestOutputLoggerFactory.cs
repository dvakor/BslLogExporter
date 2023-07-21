using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace BslLogExporter.Tests.Stubs;

public class TestOutputLoggerFactory
{
    public static ILoggerFactory Create(ITestOutputHelper helper)
    {
        return LoggerFactory.Create(builder => builder.AddProvider(CreateProvider(helper)));
    }

    public static ILoggerProvider CreateProvider(ITestOutputHelper helper)
    {
        return new TestOutputLoggerProvider(helper);
    }
    
    private class TestOutputLoggerProvider : ILoggerProvider
    {
        private readonly ITestOutputHelper _helper;

        public TestOutputLoggerProvider(ITestOutputHelper helper)
        {
            _helper = helper;
        }

        public void Dispose()
        {
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new TestOutputLogger(categoryName, _helper);
        }
    }
    
    private class TestOutputLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly ITestOutputHelper _helper;

        public TestOutputLogger(string categoryName, ITestOutputHelper helper)
        {
            _categoryName = categoryName;
            _helper = helper;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var str = formatter(state, exception);
            
            if (string.IsNullOrEmpty(str))
                return;
            
            var interpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 2);
            interpolatedStringHandler.AppendFormatted<LogLevel>(logLevel);
            interpolatedStringHandler.AppendLiteral(": ");
            interpolatedStringHandler.AppendFormatted(str);
            
            var message = interpolatedStringHandler.ToStringAndClear();
            
            if (exception != null)
                message = message + Environment.NewLine + Environment.NewLine + exception;
            
            _helper.WriteLine(message);
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return NullScope.Instance;
        }
    }
    
    private sealed class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new NullScope();

        private NullScope()
        {
        }
        
        public void Dispose()
        {
        }
    }
}