namespace LogExporter.App.Sources.Cluster;

public class ClusterArguments
{
    public string? NamePattern { get; init; }
    
    public string? Filter { get; init; }
    
    public string FolderPath { get; init; }
    
    public bool LiveMode { get; init; }
}