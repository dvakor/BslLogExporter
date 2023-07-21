using Ardalis.GuardClauses;
using Microsoft.Extensions.FileProviders;

namespace LogExporter.App.Sources.Folder;

public class FolderSourceFactory : AbstractSourceFactory<FolderArguments>
{
    public override string TypeName => "Folder";

    protected override LogSourcesSnapshot CreateSources(FolderArguments args)
    {
        Guard.Against.NullOrWhiteSpace(args.Name);
        Guard.Against.NullOrWhiteSpace(args.FolderPath);

        return new LogSourcesSnapshot
        {
            ChangeToken = NullChangeToken.Singleton,
            Sources = new List<ILogSource>
            {
                new FolderLogSource(args.Name, args.FolderPath, args.LiveMode)
            }
        };
    }
}