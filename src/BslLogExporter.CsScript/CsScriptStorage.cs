using System.Diagnostics.CodeAnalysis;

namespace BslLogExporter.CsScript;

public sealed class CsScriptStorage : Dictionary<string, object>, IDisposable
{
    public T Get<T>(string key)
    {
        return (T)this[key];
    }

    public void Set(string key, object value)
    {
        if (ContainsKey(key))
        {
            this[key] = value;
        }
        else
        {
            Add(key, value);
        }
    }

    public bool TryGet<T>(string key, [NotNullWhen(true)] out T? value)
    {
        if (ContainsKey(key))
        {
            value = Get<T>(key)!;
            return true;
        }

        value = default;
        return false;
    }

    public T GetOrAdd<T>(string key, Func<T> factory)
    {
        if (TryGet<T>(key, out var value))
        {
            return value;
        }
        
        value = factory();
        
        Set(key, value!);

        return value;
    }

    public void Dispose()
    {
        foreach (var (_, value) in this)
        {
            if (value is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}