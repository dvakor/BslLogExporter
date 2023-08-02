using System.Text.RegularExpressions;
using Ardalis.GuardClauses;
using LogExporter.App.Helpers;
using LogExporter.App.Sources.Folder;
using Microsoft.Extensions.Primitives;
using Polly;

namespace LogExporter.App.Sources.Cluster;

public class ClusterSourceFactory : AbstractSourceFactory<ClusterArguments>
{
    private const RegexOptions Options = RegexOptions.CultureInvariant | RegexOptions.IgnoreCase;
    private readonly ClusterDataReader _reader;
    private static readonly Policy Policy 
        = HelperMethods.CreateRetryPolicy<FileNotFoundException>(10, 1000); 
    
    public override string TypeName => "Cluster";

    public ClusterSourceFactory()
    {
        _reader = new ClusterDataReader();
    }
    
    protected override LogSourcesSnapshot CreateSources(ClusterArguments args)
    {
        Guard.Against.NullOrWhiteSpace(args.FolderPath);

        var infoBasesResult = Policy.Execute(() => _reader.GetInfoBases(args.FolderPath));
        
        var infobases = infoBasesResult
            .InfoBases
            .Where(x => string.IsNullOrWhiteSpace(args.Filter) 
                        || Regex.IsMatch(x.Name, args.Filter, Options))
            .ToList();

        var sources = new List<ILogSource>();

        var tokens = new List<IChangeToken>
        {
            infoBasesResult.ChangeToken
        };

        // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
        foreach (var infobase in infobases)
        {
            var sourceName = GetSourceName(args.NamePattern, infobase.Name);

            var logFolder = Path.Combine(args.FolderPath, infobase.Id, "1Cv8Log");

            if (!Directory.Exists(logFolder))
            {
                // Если ИБ была создана, а логи еще не появились,
                // то дождемся создания папки с логами и заного перезагрузим источники
                tokens.Add(new PollingChangeToken(() => Directory.Exists(logFolder), 500));
                continue;
            }
            
            var logSource = new FolderLogSource(sourceName, logFolder, args.LiveMode);
            
            sources.Add(logSource);
        }

        return new LogSourcesSnapshot
        {
            ChangeToken = new CompositeChangeToken(tokens),
            Sources = sources
        };
    }

    private static string GetSourceName(string? namePattern, string infobaseName)
    {
        return string.IsNullOrWhiteSpace(namePattern) 
            ? infobaseName 
            : namePattern.Replace("{ibName}", infobaseName, StringComparison.OrdinalIgnoreCase);
    }
}