namespace LogExporter.Core.LogReader
{
    public class LogEntry
    {
        private static readonly DateTime DefaultDate = new(1970, 1, 1);
        
        public string FileName { get; set; } = string.Empty;
        
        public long Position { get; set; }

        public DateTime DateTime { get; set; } = DefaultDate;
        
        public string TransactionStatus { get; set; } = string.Empty;

        public DateTime TransactionDateTime { get; set; } = DefaultDate;
        
        public long TransactionNumber { get; set; }
        
        public string UserUuid { get; set; } = string.Empty;
        
        public string User { get; set; } = string.Empty;
        
        public string Computer { get; set; } = string.Empty;
        
        public string Application { get; set; } = string.Empty;
        
        public string ApplicationPresentation { get; set; } = string.Empty;
        
        public long Connection { get; set; }
        
        public string EventPresentation { get; set; } = string.Empty;
        
        public string Event { get; set; } = string.Empty;
        
        public string Severity { get; set; } = string.Empty;
        
        public string Comment { get; set; } = string.Empty;
        
        public string MetadataUuid { get; set; } = string.Empty;
        
        public string Metadata { get; set; } = string.Empty;
        
        public string Data { get; set; } = string.Empty;
        
        public string DataPresentation { get; set; } = string.Empty;
        
        public string Server { get; set; } = string.Empty;
        
        public int MainPort { get; set; }
        
        public int AddPort { get; set; }
        
        public long Session { get; set; }
    }
}