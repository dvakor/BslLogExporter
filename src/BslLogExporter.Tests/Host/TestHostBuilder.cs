using System;
using System.Collections.Generic;
using Microsoft.Extensions.Hosting;

namespace BslLogExporter.Tests.Host;

public class TestHostBuilder
{
    private List<Action<HostApplicationBuilder>> _builderActions = new();
    private List<Action<IHost>> _hostActions = new();

    public static TestHostBuilder New() => new();

    public TestHostBuilder AddBuilderAction(Action<HostApplicationBuilder> action)
    {
        _builderActions.Add(action);
        return this;
    }
    
    public TestHostBuilder AddHostAction(Action<IHost> action)
    {
        _hostActions.Add(action);
        return this;
    }

    public TestHost Build()
    {
        return TestHost.CreateHostAsync(builder =>
        {
            foreach (var action in _builderActions)
            {
                action.Invoke(builder);
            }
        }, host =>
        {
            foreach (var action in _hostActions)
            {
                action.Invoke(host);
            }
        });
    }
}