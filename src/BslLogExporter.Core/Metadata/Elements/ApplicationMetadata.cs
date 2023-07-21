using LogExporter.Core.Brackets.Values;
using LogExporter.Core.Extensions;
using LogExporter.Core.Metadata.Elements.Abstraction;

namespace LogExporter.Core.Metadata.Elements
{
    public class ApplicationMetadata : AbstractMetadata
    {
        public override ObjectType MetadataType => ObjectType.Application;
        
        public string Name { get; }
        
        public string Id { get; }
        
        public ApplicationMetadata(BracketsNodeValue nodeValue) : base(nodeValue)
        {
            Id = nodeValue.Value(1).Value.ToUpper();
            
            Name = Id switch
            {
                "1CV8" => "Толстый клиент",
                "1CV8C" => "Тонкий клиент",
                "WEBCLIENT" => "Веб-клиент",
                "DESIGNER" => "Конфигуратор",
                "COMCONNECTION" => "Внешнее соединение (COM, обычное)",
                "WSCONNECTION" => "Сессия web-сервиса",
                "BACKGROUNDJOB" => "Фоновое задание",
                "SYSTEMBACKGROUNDJOB" => "Системное фоновое задание",
                "SRVRCONSOLE" => "Консоль кластера",
                "COMCONSOLE" => "Внешнее соединение (COM, административное)",
                "JOBSCHEDULER" => "Планировщик заданий",
                "DEBUGGER" => "Отладчик",
                "RAS" => "Сервер администрирования",
                _ => $"Unknown ({nodeValue.Value(1).Value})"
            };
        }
    }
}