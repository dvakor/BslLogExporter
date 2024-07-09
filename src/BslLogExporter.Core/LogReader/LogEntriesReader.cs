using LogExporter.Core.Brackets;
using LogExporter.Core.Brackets.Values;
using LogExporter.Core.Extensions;
using LogExporter.Core.Metadata;
using LogExporter.Core.Metadata.Elements;
using LogExporter.Core.Metadata.Elements.Abstraction;

namespace LogExporter.Core.LogReader
{
    public sealed class LogEntriesReader : BracketsFileReader
    {
        private readonly MetadataReader _metadataReader;
        
        public LogEntriesReader(string filePath, MetadataReader metadataReader) : base(filePath)
        {
            _metadataReader = metadataReader;
        }

        public BslLogEntry? GetNextEntry(CancellationToken cancellationToken = default)
        {
            var node = ReadNext(cancellationToken);

            if (node == null)
            {
                return null;
            }

            try
            {
                return ParseLogItem(node, cancellationToken);
            }
            catch (Exception e)
            {
                throw new LogEntryParseException(node, FileName, e);
            }
        }

        private BslLogEntry ParseLogItem(BracketsNodeValue node, CancellationToken cancellationToken)
        {
            var item = new BslLogEntry
            {
                FileName = FileName,
                DateTime = node.Value(0).ToDateTime(),
                TransactionStatus = node.Value(1).ToTransactionStatus(),
                TransactionDateTime = node.Node(2).Value(0).ToDateTime(),
                TransactionNumber = node.Node(2).Value(1).ToBase16Number(),
                Connection = node.Value(6).ToLong(),
                Severity = node.Value(8).ToSeverity(),
                Comment = node.Value(9).Value.Trim(),
                Data = node.Node(11).ToAdditionalData(),
                DataPresentation = node.Value(12).Value.Trim(),
                Session = node.Value(16).ToLong(),
                Position = node.EndPosition
            };

            FromMetadata<UserMetadata>(node.Value(3), md =>
            {
                item.User = md.Name;
                item.UserUuid = md.Id;
            }, cancellationToken);

            FromMetadata<ComputerMetadata>(node.Value(4), md => 
                { item.Computer = md.Name; },
                cancellationToken);

            FromMetadata<ApplicationMetadata>(node.Value(5), md =>
                {
                    item.Application = md.Id;
                    item.ApplicationPresentation = md.Name;
                },
                cancellationToken);

            FromMetadata<EventMetadata>(node.Value(7), md =>
                {
                    item.Event = md.Id;
                    item.EventPresentation = md.Name;
                },
                cancellationToken);

            FromMetadata<ObjectMetadata>(node.Value(10), md =>
            {
                item.MetadataUuid = md.Id;
                item.Metadata = md.Name;
            }, cancellationToken);

            FromMetadata<ServerMetadata>(node.Value(13), md => 
                { item.Server = md.Name; },
                cancellationToken);

            FromMetadata<MainPortMetadata>(node.Value(14), md => 
                { item.MainPort = md.Port; },
                cancellationToken);

            FromMetadata<AddPortMetadata>(node.Value(15), md => 
                { item.AddPort = md.Port; },
                cancellationToken);
            
            return item;
        }

        private void FromMetadata<T>(BracketsStringValue value, Action<T> action, CancellationToken token) where T : IMetadataElement
        {
            var md = _metadataReader.FindMetadata<T>(value, token);

            if (md == null)
            {
                return;
            }

            action(md);
        }
    }
}