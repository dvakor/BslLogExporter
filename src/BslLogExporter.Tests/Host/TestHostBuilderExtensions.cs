using System;
using BslLogExporter.Tests.Helpers;
using BslLogExporter.Tests.Stubs;
using BslLogExporter.Tests.Stubs.Exporters;
using BslLogExporter.Tests.Stubs.History;
using BslLogExporter.Tests.Stubs.Sources;
using LogExporter.App.Exporters;
using LogExporter.App.History;
using LogExporter.App.Sources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace BslLogExporter.Tests.Host;

public static class TestHostBuilderExtensions
{
    public static TestHostBuilder WithFakeLogsSource(this TestHostBuilder builder, 
        Action<FakeSourceSettings> settings)
    {
        return builder.AddBuilderAction(applicationBuilder =>
        {
            applicationBuilder.Services.Configure(settings);
            applicationBuilder.Services.AddSingleton<ILogSourceFactory, FakeLogsSourceFactory>();
        });
    }

    public static TestHostBuilder WithFakeLogsExporter(this TestHostBuilder builder)
    {
        return builder.AddBuilderAction(applicationBuilder =>
        {
            applicationBuilder.Services.AddSingleton<ILogExporterFactory, FakeExporterFactory>();
        });
    }

    public static TestHostBuilder WithInMemoryHistoryStorage(this TestHostBuilder builder)
    {
        return builder
            .AddBuilderAction(applicationBuilder =>
            {
                applicationBuilder.Services.AddSingleton<ILogHistoryStorage, InMemoryLogHistoryStorage>();
            });
    }

    public static TestHostBuilder WithTestOutputLogger(this TestHostBuilder builder, ITestOutputHelper helper)
    {
        return builder
            .WithSettings(new
            {
                Logging = new
                {
                    LogLevel = new
                    {
                        Default = "Debug"
                    }
                }
            })
            .AddBuilderAction(applicationBuilder =>
            {
                applicationBuilder.Logging.ClearProviders();
                applicationBuilder.Logging.Services.AddSingleton<ILoggerProvider>(x
                    => TestOutputLoggerFactory.CreateProvider(helper));
            });
    }

    public static TestHostBuilder WithSettings(this TestHostBuilder builder, object settings)
    {
        return builder.AddBuilderAction(applicationBuilder =>
        {
            applicationBuilder.Configuration.AddObject(settings);
        });
    }
}