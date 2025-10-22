# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

BslLogExporter is a .NET 8.0 application that exports 1C (1С:Предприятие) logs to any destination using custom C# or OneScript scripts. The application supports hot-reload, real-time log streaming, cluster and folder-based log sources, and can run as a console application, Windows service, or in a Docker container.

## Solution Structure

The solution consists of 6 projects:

- **BslLogExporter.Core** - Low-level log parsing library
  - Parses 1C log files (*.lgp format)
  - Reads metadata (*.lgf format)
  - Implements bracket-based parser for 1C log format
  - Provides file/directory watchers

- **BslLogExporter.App** - Application logic and orchestration
  - Log sources management (Folder, Cluster)
  - Exporters management (plugin system)
  - Processing pipeline (buffering, history tracking)
  - Configuration hot-reload support

- **BslLogExporter.CsScript** - C# script exporter using dotnet-script
  - Compiles and executes .csx scripts
  - Provides context for accessing log entries in scripts

- **BslLogExporter.OScript** - OneScript exporter for 1C-style scripts
  - Executes .os scripts using OneScript runtime
  - Provides 1C-compatible API for log processing

- **BslLogExporter** - Main executable
  - Console/Windows Service entry point
  - Configures dependency injection
  - Sets up Serilog logging

- **BslLogExporter.Tests** - xUnit test project
  - Uses Moq for mocking

## Common Commands

### Build
```bash
dotnet build BslLogExporter.sln
```

### Build Release
```bash
dotnet build BslLogExporter.sln -c Release
```

### Run Tests
```bash
dotnet test BslLogExporter.Tests/BslLogExporter.Tests.csproj
```

### Run Single Test
```bash
dotnet test BslLogExporter.Tests/BslLogExporter.Tests.csproj --filter "FullyQualifiedName~TestMethodName"
```

### Run Application
```bash
dotnet run --project BslLogExporter/BslLogExporter.csproj
```

### Run with Custom Working Directory
```bash
dotnet run --project BslLogExporter/BslLogExporter.csproj -- /WorkingDir=/path/to/config
```

### Docker Build
```bash
docker-compose build
```

## Configuration

The application is configured via **AppConfiguration.json** (see examples/AppConfiguration.example.json for a complete example).

Configuration can also be provided via:
- Environment variables with prefix `BslLogExporter:`
- Command-line arguments (e.g., `/WorkingDir=C:\MyFolder`)

### Working Directory Priority
1. Command-line argument: `/WorkingDir=<path>`
2. Environment variable: `BslLogExporter:WorkingDir=<path>`
3. Current directory (default)

### Configuration File Loading Order
1. `appsettings.json` (from binary location)
2. `appsettings.<RunMode>.json` (ConsoleApp or WindowsService, from binary location)
3. `AppConfiguration.json` (from working directory, hot-reloadable)

### Key Configuration Sections

- **Processing** - Buffer size and timeout settings
- **History** - Export history tracking (resume from last position)
- **CsScript** - .NET target framework and cache directory for C# scripts
- **OneScript** - Path to OneScript libraries
- **Sources** - Log sources (Folder or Cluster type)
- **Exporters** - Export destinations with regex source filters

## Architecture

### Processing Pipeline

1. **Sources** → Read logs from folders or clusters
   - `FolderLogSource` - reads from a specific folder
   - `ClusterDataReader` - scans cluster folder for multiple infobases

2. **Buffer** → `LogsBuffer` accumulates entries

3. **Processor** → `LogsProcessor` dispatches to exporters

4. **Exporters** → Execute user scripts (C#/OneScript)
   - Each exporter has a `SourceFilter` (regex) to match log sources
   - Scripts receive batches of log entries

5. **History** → `LogHistoryStorage` tracks last processed position

### Hot-Reload
- Changes to `AppConfiguration.json` trigger reconfiguration
- Sources and exporters are validated and reloaded
- No application restart required

### Dependency Injection
All services are registered in `HostBuilderExtensions.SetupApplication()`:
- `LogSourcesManager` - manages all log sources
- `LogExportersManager` - manages all exporters
- `LogsBuffer` - thread-safe buffer
- `LogsProcessor` - orchestrates processing
- `ILogHistoryStorage` - persists export state

## Script Development

### C# Scripts (.csx)
Scripts are executed as top-level code with access to global variables:

- **Context** - `CsScriptContext` with properties:
  - `Entries` - collection of log entries
  - `SourceName` - name of the log source
- **Storage** - `Dictionary<string, object>` for sharing state between script executions
- **Log** - `ILogger` for script logging
- **Args** - `string[]` script arguments from configuration

Example (see examples/RabbitMqExporter.csx):
```csharp
#r "nuget: EasyNetQ, 7.5.3"
using EasyNetQ;

var bus = Storage.GetOrAdd<IBus>("bus",
    () => RabbitHutch.CreateBus("host=localhost"));

foreach (var entry in Context.Entries)
{
    // Process log entry
    Log.LogInformation("Processing {SourceName}", Context.SourceName);
}
```

### OneScript (.os)
Scripts are executed as top-level 1C code with access to global variables:

- **Контекст** (Context) - structure with properties:
  - `ИмяИсточника` (SourceName) - name of the log source
  - `Записи` (Entries) - array of log entries
- **Хранилище** (Storage) - Map for sharing state between executions
- **Лог** (Log) - logger for script logging

Example:
```bsl
Для Каждого Запись Из Контекст.Записи Цикл
    // Обработка записи лога
    Лог.Информация("Обработка источника: " + Контекст.ИмяИсточника);
КонецЦикла;
```

## Important Notes

- The working directory is where `AppConfiguration.json` is located and relative paths are resolved from it
- If C# scripts fail to compile with NU1100 error, place `NuGet.Config` in the working directory (see examples folder)
- LiveMode only works on first export (when no history exists); subsequent exports resume from last position
- Log reading continues even if a file from history no longer exists
