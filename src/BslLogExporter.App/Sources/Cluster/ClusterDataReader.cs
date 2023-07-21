using Ardalis.GuardClauses;
using LogExporter.App.Helpers;
using LogExporter.Core.Brackets;
using LogExporter.Core.Extensions;
using LogExporter.Core.Watchers;

namespace LogExporter.App.Sources.Cluster
{
    public sealed class ClusterDataReader
    {
        private const string FileName = "1CV8Clst.lst";
        
        public ClusterInfoBases GetInfoBases(string folder)
        {
            Guard.Against.NullOrWhiteSpace(folder);

            var pathToFile = Path.Combine(folder, FileName);
            
            using var reader = new BracketsParser(pathToFile);

            var node = reader.GetNextItem();

            var infoBases = new List<ClusterInfoBase>();

            var fileWatcher = new FileWatcher(pathToFile);
            
            var result = new ClusterInfoBases
            {
                ChangeToken = new PollingChangeToken(() 
                    => fileWatcher.DetectChanges() != FileChange.None),
                InfoBases = infoBases
            };

            if (node == null)
            {
                return result;
            }

            var ibNodes = node.Node(2);

            for (var i = 1; i < ibNodes.Count; i++)
            {
                var ibNode = ibNodes.Node(i);
                
                infoBases.Add(new ClusterInfoBase
                {
                    Id = ibNode.Value(0).Value,
                    Name = ibNode.Value(1).Value
                });
            }

            return result;
        }
    }
}