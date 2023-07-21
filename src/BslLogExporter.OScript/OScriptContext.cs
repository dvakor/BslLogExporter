using ScriptEngine.HostedScript.Library;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
#pragma warning disable CS8618

namespace BslLogExporter.OScript;

[ContextClass("КонтекстВыполнения")]
public class OScriptContext : AutoContext<OScriptContext>
{
    [ContextProperty("Записи")]
    public FixedArrayImpl Entries { get; set; }
    
    [ContextProperty("ИмяИсточника")]
    public IValue SourceName { get; set; }
}