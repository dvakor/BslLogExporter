using BslLogExporter.Tests.Helpers;
using BslLogExporter.Tests.Stubs;
using LogExporter.App.History;
using LogExporter.App.Sources;
using LogExporter.App.Sources.Folder;

namespace BslLogExporter.Tests;

public class FolderSourceTests
{
    [Fact]
    public void Should_Read_Folder_Logs()
    {
        using var tempDir = GetLogsFolder();
        
        using var source = CreateSource(tempDir.FullName);
        
        var logs = source.Sources.Single().GetLogs(CancellationToken.None).ToList();

        Assert.True(logs.Any(), "Ожидалось, что будет прочтено некоторое количество логов");
    }

    [Fact]
    public async Task Should_Read_Folder_Logs_In_Live_Mode()
    {
        using var tempDir = GetLogsFolder();
        
        var expectedLiveFile = tempDir.FullPathTo("20230718000000.lgp");

        using var snapshot = CreateSource(tempDir.FullName, true);

        var source = snapshot.Sources.Single();
        
        source.ForwardTo(HistoryRecord.Empty);

        var logsSource = source.GetLogs(CancellationToken.None);
        
        var startReadingLogsCount = logsSource.ToList().Count;

        await TestUtils.AppendToFile(expectedLiveFile, TestUtils.TestLog);
        
        var logsCountAfterFileWasModified = logsSource.ToList().Count;

        Assert.StrictEqual(0, startReadingLogsCount);
        Assert.StrictEqual(1, logsCountAfterFileWasModified);
    }
    
    private static TempFolder GetLogsFolder()
    {
        return TempFolder.FromSourceFolder("./Files/ClusterLogs/0b196f3a-243a-45be-8b9c-7cf151acdf30/1Cv8Log");
    }
    
    private static LogSourcesSnapshot CreateSource(string folder, bool liveMode = false)
    {
        return SutFactory.CreateSource(new FolderArguments
        {
            FolderPath = folder,
            Name = "TestSource",
            LiveMode = liveMode
        }, () => new FolderSourceFactory());
    }
}