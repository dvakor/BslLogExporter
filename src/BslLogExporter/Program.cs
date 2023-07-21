using LogExporter;
using LogExporter.App.Exporters;
using LogExporter.App.Sources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateApplicationBuilder(args);

host.SetupEnvironmentAndConfiguration(args);
host.SetupLogging();
host.SetupApplication();

var app = host.Build();

app.Services
    .GetRequiredService<LogExportersManager>()
    .Validate();

app.Services
    .GetRequiredService<LogSourcesManager>()
    .Validate();

await app.RunAsync();