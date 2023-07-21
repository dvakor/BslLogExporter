using LogExporter.App.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace BslLogExporter.Tests.Host;

using LogExporter;
using Microsoft.Extensions.Hosting;

public sealed class TestHost : IAsyncDisposable
{
    private readonly IHost _host;

    private TestHost(Action<HostApplicationBuilder>? builderSetup, Action<IHost>? hostSetup)
    {
        var builder = Host.CreateApplicationBuilder(Array.Empty<string>());
        
        builder.SetupApplication();

        builderSetup?.Invoke(builder);
        
        _host = builder.Build();
        
        hostSetup?.Invoke(_host);
    }

    public IServiceProvider Services => _host.Services;

    public async Task StartHost()
    {
        await _host.StartAsync();

        var lifeTime = _host.Services.GetRequiredService<IHostApplicationLifetime>();
        await lifeTime.ApplicationStarted.WaitForCancellation();
    }

    public async Task StopHost()
    {
        await _host.StopAsync();
        await _host.WaitForShutdownAsync();
    }

    public static TestHost CreateHostAsync(
        Action<HostApplicationBuilder>? builderSetup = default,
        Action<IHost>? hostSetup = default)
    {
        var host = new TestHost(builderSetup, hostSetup);
        return host;
    }

    public async ValueTask DisposeAsync()
    {
        await _host.StopAsync();
        await _host.WaitForShutdownAsync();
        _host.Dispose();
    }
}