using Microsoft.Extensions.Primitives;

namespace LogExporter.App.Sources.Cluster;

public class ClusterInfoBasesSnapshot
{
    public IReadOnlyCollection<ClusterInfoBase> InfoBases { get; init; } = default!;
    
    public IChangeToken ChangeToken { get; init; } = default!;
}