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

#pragma warning disable CA1822
        // ReSharper disable once MemberCanBeMadeStatic.Global
        public ClusterInfoBasesSnapshot GetInfoBases(string folder)
#pragma warning restore CA1822
        {
            Guard.Against.NullOrWhiteSpace(folder);

            var pathToFile = Path.Combine(folder, FileName);

            var infoBases = ReadInfoBases(pathToFile);
            
            var fileWatcher = new FileWatcher(pathToFile);
            
            var result = new ClusterInfoBasesSnapshot
            {
                ChangeToken = new PollingChangeToken(() 
                    => InfoBaseListChanged(fileWatcher, pathToFile, infoBases), 1000),
                InfoBases = infoBases
            };
            
            return result;
        }

        private static List<ClusterInfoBase> ReadInfoBases(string pathToFile)
        {
            using var reader = new BracketsParser(pathToFile);
            
            var node = reader.GetNextItem();

            var infoBases = new List<ClusterInfoBase>();
            
            if (node == null)
            {
                return infoBases;
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
            
            return infoBases;
        }

        private static bool InfoBaseListChanged(
            FileWatcher fileWatcher,
            string pathToFile,
            ICollection<ClusterInfoBase> ibSnapshot)
        {
            if (fileWatcher.DetectChanges() == FileChange.None)
            {
                return false;
            }

            var actualIb = ReadInfoBases(pathToFile);

            return actualIb.DiffersFrom(ibSnapshot);
        }
    }
}