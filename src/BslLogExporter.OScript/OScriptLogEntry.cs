using LogExporter.Core.LogReader;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace BslLogExporter.OScript;

[ContextClass("ЗаписьЛога")]
public class OScriptLogEntry : AutoContext<OScriptLogEntry>
{
    private readonly LogEntry _entry;

    public OScriptLogEntry(LogEntry entry)
    {
        _entry = entry;
    }
    
    [ContextProperty(nameof(FileName))]
    public IValue FileName => ValueFactory.Create(_entry.FileName);

    [ContextProperty(nameof(Position))]
    public IValue Position => ValueFactory.Create(_entry.Position);

    [ContextProperty(nameof(DateTime))] 
    public IValue DateTime => ValueFactory.Create(_entry.DateTime);

    [ContextProperty(nameof(TransactionStatus))]
    public IValue TransactionStatus => ValueFactory.Create(_entry.TransactionStatus);

    [ContextProperty(nameof(TransactionDateTime))]
    public IValue TransactionDateTime => ValueFactory.Create(_entry.TransactionDateTime);

    [ContextProperty(nameof(TransactionDateTime))]
    public IValue TransactionNumber => ValueFactory.Create(_entry.TransactionNumber);

    [ContextProperty(nameof(UserUuid))]
    public IValue UserUuid => ValueFactory.Create(_entry.UserUuid);

    [ContextProperty(nameof(User))]
    public IValue User => ValueFactory.Create(_entry.User);

    [ContextProperty(nameof(Computer))]
    public IValue Computer => ValueFactory.Create(_entry.Computer);

    [ContextProperty(nameof(Application))]
    public IValue Application => ValueFactory.Create(_entry.Application);

    [ContextProperty(nameof(ApplicationPresentation))]
    public IValue ApplicationPresentation => ValueFactory.Create(_entry.ApplicationPresentation);

    [ContextProperty(nameof(Connection))]
    public IValue Connection => ValueFactory.Create(_entry.Connection);

    [ContextProperty(nameof(EventPresentation))]
    public IValue EventPresentation => ValueFactory.Create(_entry.EventPresentation);

    [ContextProperty(nameof(Event))]
    public IValue Event => ValueFactory.Create(_entry.Event);

    [ContextProperty(nameof(Severity))]
    public IValue Severity => ValueFactory.Create(_entry.Severity);

    [ContextProperty(nameof(Comment))]
    public IValue Comment => ValueFactory.Create(_entry.Comment);

    [ContextProperty(nameof(MetadataUuid))]
    public IValue MetadataUuid => ValueFactory.Create(_entry.MetadataUuid);

    [ContextProperty(nameof(Metadata))]
    public IValue Metadata => ValueFactory.Create(_entry.Metadata);

    [ContextProperty(nameof(Data))]
    public IValue Data => ValueFactory.Create(_entry.Data);

    [ContextProperty(nameof(DataPresentation))]
    public IValue DataPresentation => ValueFactory.Create(_entry.DataPresentation);

    [ContextProperty(nameof(Server))]
    public IValue Server => ValueFactory.Create(_entry.Server);

    [ContextProperty(nameof(MainPort))]
    public IValue MainPort => ValueFactory.Create(_entry.MainPort);

    [ContextProperty(nameof(AddPort))]
    public IValue AddPort => ValueFactory.Create(_entry.AddPort);

    [ContextProperty(nameof(Session))]
    public IValue Session => ValueFactory.Create(_entry.Session);
}