using System.Diagnostics.CodeAnalysis;
using BslLogExporter.Tests.Helpers;
using BslLogExporter.Tests.Stubs;
using LogExporter.App.Helpers;
using LogExporter.App.History;
using LogExporter.App.Sources;
using LogExporter.App.Sources.Cluster;

namespace BslLogExporter.Tests;

public class ClusterSourceTests
{
    [Fact]
    public void Should_Read_Cluster_Logs()
    {
        using var tempFolder = GetLogsFolder();
        using var source = CreateSource(tempFolder.FullName, "TestIb_2");

        var logs = source.Sources.Single().GetLogs(CancellationToken.None).ToList();

        Assert.True(logs.Any(), "Ожидалось, что будет прочтено некоторое количество логов");
    }
    
    [Fact]
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public async Task Should_Read_Cluster_Logs_In_Live_Mode()
    {
        using var tempFolder = GetLogsFolder();
        var expectedLiveFile = tempFolder.FullPathTo("0b196f3a-243a-45be-8b9c-7cf151acdf30/1Cv8Log/20230718000000.lgp");
        using var snapshot = CreateSource(tempFolder.FullName, "TestIb_1", true);
        var logSource = snapshot.Sources.Single();
        logSource.ForwardTo(HistoryRecord.Empty);
        var entries = logSource.GetLogs(CancellationToken.None);
        
        var startReadingLogsCount = entries.ToList().Count;
        await TestUtils.AppendToFile(expectedLiveFile, TestUtils.TestLog);
        var logsCountAfterFileWasModified = entries.ToList().Count;

        Assert.Equal("Test:TestIb_1", logSource.Name);
        Assert.StrictEqual(0, startReadingLogsCount);
        Assert.StrictEqual(1, logsCountAfterFileWasModified);
    }

    [Fact]
    public async Task Should_Create_Source_After_New_Ib_Created()
    {
        using var tempFolder = GetLogsFolder();
        
        using var initial = CreateSource(tempFolder.FullName, "TestIb_3");

        tempFolder.CopyChild(
            "0b196f3a-243a-45be-8b9c-7cf151acdf30",
            "fc6bb85f-3b1a-4328-9f84-5b02e31b4c70");

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

        await ChangeTokenAwaiter.WaitForTokenChange(initial.ChangeToken, token: cts.Token);

        using var actual = CreateSource(tempFolder.FullName, "TestIb_3");
        
        Assert.StrictEqual(0, initial.Sources.Count);
        Assert.StrictEqual(1, actual.Sources.Count);
    }

    [Fact]
    public async Task Should_Get_Actual_Sources_When_InfoBases_Changed()
    {
        using var tempFolder = GetLogsFolder();
        var listFile = tempFolder.FullPathTo("1CV8Clst.lst");
        await TestUtils.ReplaceInFile(listFile, 
            "fc6bb85f-3b1a-4328-9f84-5b02e31b4c70", 
            "0b196f3a-243a-45be-8b9c-7cf151acdf30");
        
        using var initialSource = CreateSource(tempFolder.FullName);
        using var initialFilteredSource = CreateSource(tempFolder.FullName, "TestIb_2");

        await TestUtils.ReplaceInFile(listFile, "TestIb_3", "TestIb_4");
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        await ChangeTokenAwaiter.WaitForTokenChange(initialSource.ChangeToken, token: cts.Token);
        
        using var actualSource = CreateSource(tempFolder.FullName);
        using var actualFilteredSource = CreateSource(tempFolder.FullName, "TestIb_3");

        Assert.StrictEqual(3, initialSource.Sources.Count);
        Assert.StrictEqual(1, initialFilteredSource.Sources.Count);
        Assert.True(initialSource.ChangeToken.HasChanged);
        Assert.StrictEqual(3, actualSource.Sources.Count);
        Assert.StrictEqual(0, actualFilteredSource.Sources.Count);
    }

    private static TempFolder GetLogsFolder()
    {
        return TempFolder.FromSourceFolder("./Files/ClusterLogs");
    }

    private static LogSourcesSnapshot CreateSource(string logsFolder, string filter = "", bool liveMode = false)
    {
        return SutFactory.CreateSource(new ClusterArguments
        {
            FolderPath = logsFolder,
            Filter = filter,
            LiveMode = liveMode,
            NamePattern = "Test:{IbName}"
        }, () => new ClusterSourceFactory());
    }
}