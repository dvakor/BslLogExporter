using Microsoft.Extensions.Logging;
using ScriptEngine;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace BslLogExporter.OScript;

[ContextClass("Скрипт")]
public sealed class OScriptInstance : ScriptDrivenObject, IDisposable
{
    private static readonly ContextPropertyMapper<OScriptInstance> OwnProperties = new();

    public OScriptInstance(
        CompilerService compiler,
        OScriptCodeSource source,
        ILoggerFactory loggerFactory) : base(CompileSources(compiler, source))
    {
        Log = new OScriptLogWrapper(loggerFactory);
    }

    [ContextProperty("Контекст")]
    public OScriptContext Context { get; set; } = default!;

    [ContextProperty("Хранилище")] 
    public MapImpl Storage { get; } = new();
    
    [ContextProperty("Лог")]
    public OScriptLogWrapper Log { get; }

    protected override int GetOwnMethodCount()
    {
        return 0;
    }

    protected override int GetOwnVariableCount()
    {
        return OwnProperties.Count;
    }

    protected override void UpdateState()
    {
            
    }

    protected override int FindOwnProperty(string name)
    {
        return OwnProperties.FindProperty(name);
    }

    protected override string GetOwnPropName(int index)
    {
        return OwnProperties.GetProperty(index).Name;
    }

    protected override IValue GetOwnPropValue(int index)
    {
        return OwnProperties.GetProperty(index).Getter(this);
    }
    
    protected override void SetOwnPropValue(int index, IValue val)
    {
        OwnProperties.GetProperty(index).Setter(this, val);
    }

    protected override bool IsOwnPropReadable(int index)
    {
        return OwnProperties.GetProperty(index).CanRead;
    }

    protected override bool IsOwnPropWritable(int index)
    {
        return OwnProperties.GetProperty(index).CanWrite;
    }

    public void Dispose()
    {
        foreach (var item in Storage)
        {
            if (item.Value is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
    
    private static LoadedModule CompileSources(CompilerService compiler, OScriptCodeSource source)
    {
        for (var i = 0; i < OwnProperties.Count; i++)
        {
            var propInfo = OwnProperties.GetPropertyInfo(i);
            
            compiler.DefineVariable(
                propInfo.Identifier, 
                propInfo.Alias, propInfo.Type);
        }
        
        var module = new LoadedModule(compiler.Compile(source));

        return module;
    }
}