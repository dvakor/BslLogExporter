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

    [Fact]
    public async Task Should_Handle_Log_Rotation_When_Old_File_Missing()
    {
        using var tempDir = GetLogsFolder();

        var oldLogFile = "20220718000000.lgp";

        using var initialSnapshot = CreateSource(tempDir.FullName);
        var initialSource = initialSnapshot.Sources.Single();
        var allLogsFromFirstFile = initialSource.GetLogs(CancellationToken.None).ToList();

        var historyRecord = new HistoryRecord { File = oldLogFile, Position = allLogsFromFirstFile.Count };

        File.Delete(tempDir.FullPathTo(oldLogFile));

        await Task.Delay(100);

        using var newSnapshot = CreateSource(tempDir.FullName);
        var newSource = newSnapshot.Sources.Single();

        newSource.ForwardTo(historyRecord);

        var logsFromNewFile = newSource.GetLogs(CancellationToken.None).ToList();

        Assert.True(logsFromNewFile.Any(), "Ожидалось, что после ротации будут прочитаны логи из нового файла");
    }

    [Fact]
    public async Task Should_Detect_New_File_After_Rotation()
    {
        using var tempDir = TempFolder.FromSourceFolder("./Files/ClusterLogs/0b196f3a-243a-45be-8b9c-7cf151acdf30/1Cv8Log");

        var oldLogFile = "20220718000000.lgp";
        var newLogFile = "20250101000000.lgp";
        var newLogPath = tempDir.FullPathTo(newLogFile);

        var historyRecord = new HistoryRecord { File = oldLogFile, Position = 1000000 };

        File.Copy(tempDir.FullPathTo("20230718000000.lgp"), newLogPath);

        await Task.Delay(1500);

        using var snapshot = CreateSource(tempDir.FullName);
        var source = snapshot.Sources.Single();

        source.ForwardTo(historyRecord);

        var logsAfterRotation = source.GetLogs(CancellationToken.None).Take(10).ToList();

        Assert.True(logsAfterRotation.Any(), "Ожидалось, что после ротации будут обнаружены и прочитаны файлы");
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