using System.Text;

namespace LogExporter.Core.Brackets
{
    public sealed class FileReader : IDisposable
    {
        private const int BufferSize = 4096;
        
        private readonly Stream _stream;
        private readonly StreamReader _streamReader;
        private static readonly Encoding FileEncoding = Encoding.UTF8;

        public long CurrentPosition { get; private set; }

        public long Length => _stream.Length;
        
        public FileReader(string file)
        {
            _stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            _streamReader = new StreamReader(_stream, FileEncoding, false, BufferSize);
            CurrentPosition = FileEncoding.GetPreamble().Length;
        }
        
        public void SetPosition(long position)
        {
            CurrentPosition = position;
            _stream.Seek(CurrentPosition, SeekOrigin.Begin);
            _streamReader.DiscardBufferedData();
        }

        public char? GetNextChar()
        {
            bool endOfStream;

            try
            {
                endOfStream = _streamReader.EndOfStream;
            }
            catch (IOException)
            {
                // Ошибка CRC и др.
                return null;
            }
            
            if (endOfStream)
            {
                return null;
            }
            
            Span<char> buffer = stackalloc char[1];
            
            _streamReader.Read(buffer);
            
            CurrentPosition += FileEncoding.GetByteCount(buffer);
            
            return buffer[0];
        }

        public void Dispose()
        {
            _stream.Dispose();
            _streamReader.Dispose();
        }
    }
}