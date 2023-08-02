namespace LogExporter.App.Sources.Cluster
{
    public record ClusterInfoBase
    {
        public string Id { get; init; } = default!;
        
        public string Name { get; init; } = default!;
    }
}