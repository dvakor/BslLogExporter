#r "nuget: EasyNetQ, 7.5.3"

#nullable enable

using EasyNetQ;
using EasyNetQ.Topology;

const string exchangeName = "log-entries";

var bus = Storage.GetOrAdd<IBus>("bus", 
    () => RabbitHutch.CreateBus("host=localhost"));

var exchange = await Storage.GetOrAddAsync<Exchange>("exchange",
    () => bus.Advanced.ExchangeDeclareAsync(exchangeName, "fanout"));

foreach (var entry in Context.Entries)
{
   var rmqEntry = new RmqLogEntry
   {
      ServiceName = Context.SourceName,
      EnvironmentName = "Production",
      Message = entry.Comment,
      Level = MapLogLevel(entry.Severity),
      TimeStamp = new DateTimeOffset(entry.DateTime),
      Extras = new Dictionary<string, object>
      {
         {"FileName", entry.FileName},
         {"User", entry.User},
         {"UserUuid", entry.UserUuid},
         {"Computer", entry.Computer},
         {"Application", entry.ApplicationPresentation},
         {"Event", entry.EventPresentation},
         {"Metadata", entry.Metadata},
         {"MetadataUuid", entry.MetadataUuid},
         {"Data", entry.Data},
         {"DataPresentation", entry.DataPresentation}
      }
   };

   await bus.Advanced.PublishAsync(
    exchange, string.Empty, 
    true, new Message<RmqLogEntry>(rmqEntry));
}

static LogEntryLevel MapLogLevel(string severity)
{
   return severity switch
   {
      "Информация" => LogEntryLevel.Information,
      "Ошибка" => LogEntryLevel.Error,
      "Предупреждение" => LogEntryLevel.Warning,
      "Примечание" => LogEntryLevel.Debug,
      _ => LogEntryLevel.Information
   };
}

public class RmqLogEntry
{
   public string? ServiceName { get; init; }

   public string? EnvironmentName { get; init; }

   public string? Message { get; init; }

   public LogEntryLevel? Level { get; init; }

   public object? Extras { get; init; }

   public DateTimeOffset TimeStamp { get; init; }
}

public enum LogEntryLevel
{
   Debug,
   Information,
   Warning,
   Error
}
