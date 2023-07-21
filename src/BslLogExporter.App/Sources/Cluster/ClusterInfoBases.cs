using Microsoft.Extensions.Primitives;

namespace LogExporter.App.Sources.Cluster;

public class ClusterInfoBases
{
    public IChangeToken ChangeToken { get; init; }
    
    public IReadOnlyCollection<ClusterInfoBase> InfoBases { get; init; }
}