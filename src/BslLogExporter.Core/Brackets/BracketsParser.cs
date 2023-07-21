using System.Buffers;
using LogExporter.Core.Brackets.Values;

namespace LogExporter.Core.Brackets
{
    public sealed class BracketsParser : IDisposable
    {
        private const char LeftBracket = '{';
        private const char RightBracket = '}';
        private const char Comma = ',';
        private const char Quote = '"';
        private const char UnicodeEntityStart = '\\';

        private readonly FileReader _reader;

        private char? _currentChar;

        public long CurrentPosition => _reader.CurrentPosition;

        public long Length => _reader.Length;
        
        public BracketsParser(string file)
        {
            _reader = new FileReader(file);
        }
        
        public void SetPosition(long position)
        {
            _reader.SetPosition(position);
        }
        
        public BracketsNodeValue? GetNextItem(CancellationToken token = default)
        {
            var nodes = new Stack<BracketsNodeValue>();
            var chars = new Queue<char>();
            
            while (!token.IsCancellationRequested)
            {
                switch (_currentChar)
                {
                    case LeftBracket:
                    {
                        chars.Clear();
                        nodes.Push(new BracketsNodeValue
                        {
                            StartPosition = _reader.CurrentPosition - 1
                        });
                        break;
                    }
                    case RightBracket when nodes.Count == 1:
                    {
                        var lastNode = nodes.Pop();
                        lastNode.EndPosition = _reader.CurrentPosition;

                        var lastStr = GetString(chars);
                        if (!string.IsNullOrEmpty(lastStr))
                        {
                            lastNode.AddStringValue(lastStr);
                        }
                        
                        return lastNode;
                    }
                    case RightBracket when nodes.Count > 1:
                    {
                        var child = nodes.Pop();
                        child.EndPosition = _reader.CurrentPosition;
                        
                        var lastStr = GetString(chars);
                        if (!string.IsNullOrEmpty(lastStr))
                        {
                            child.AddStringValue(lastStr);
                        }
                        
                        nodes.Peek().AddNodeValue(child); 
                        chars.Clear();
                        break;
                    }
                    case Quote when nodes.Count > 0:
                    {
                        ParseMultilineString(chars);
                        nodes.Peek().AddStringValue(GetString(chars));
                        chars.Clear();
                        continue;
                    }
                    case Comma when nodes.Count > 0:
                    {
                        if (chars.Count > 0)
                        {
                            nodes.Peek().AddStringValue(GetString(chars));    
                        }
                        chars.Clear();
                        break;
                    }
                    case not null:
                    {
                        chars.Enqueue(_currentChar.Value);
                        break;
                    }
                }

                if (NextChar() is null)
                {
                    break;
                } 
            }

            return null;
        }

        private void ParseMultilineString(Queue<char> chars)
        {
            var counter = 1;

            var nextInc = -1;

            NextChar(); // Пропуск первой кавычки
            
            while (_currentChar is not null)
            {
                if (_currentChar == Quote)
                {
                    counter += nextInc;
                    nextInc = -nextInc;
                }

                if (counter != 0)
                {
                    chars.Enqueue(_currentChar!.Value);
                    NextChar();
                    continue;
                };

                NextChar();
                
                if (_currentChar.Value != Quote)
                {
                    // Сущность юникода
                    if (_currentChar.Value == UnicodeEntityStart)
                    {
                        // Сбрасываем счетчик кавычек,
                        // тк последовательность "\ не должна учитываться
                        counter = 1;
                        nextInc = -1;
                        
                        chars.Enqueue(_currentChar!.Value);

                        for (var i = 0; i < 4; i++)
                        {
                            chars.Enqueue(NextChar()!.Value);
                        }
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    chars.Enqueue(_currentChar!.Value);
                }
            }
        }
        
        private char? NextChar()
        {
            _currentChar = _reader.GetNextChar();
            return _currentChar;
        }
        
        private static string GetString(Queue<char> chars)
        {
            var charsCount = chars.Count;

            var charsArr = ArrayPool<char>.Shared.Rent(charsCount);
            
            try
            {
                chars.CopyTo(charsArr, 0);
                
                var str = new string(charsArr, 0, charsCount);

                return charsCount > 1 ? str.Trim() : str;
            }
            finally
            {
                ArrayPool<char>.Shared.Return(charsArr);
            }
        }
        
        public void Dispose()
        {
            _reader.Dispose();
        }
    }
}