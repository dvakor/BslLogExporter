using ScriptEngine.Environment;

namespace BslLogExporter.OScript;

public class OScriptCodeSource : ICodeSource
{
    public OScriptCodeSource(string file)
    {
        SourceDescription = file;
    }

    public string Code
    {
        get
        {
            using var reader = new StreamReader(File.OpenRead(SourceDescription));
            return reader.ReadToEnd();
        }
    }

    public string SourceDescription { get; }
}