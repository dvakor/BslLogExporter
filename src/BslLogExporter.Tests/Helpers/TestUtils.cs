using System.IO;
using System.Threading.Tasks;

namespace BslLogExporter.Tests.Helpers;

public static class TestUtils
{
    public const string TestLog = 
        @"{20230718082407,N,{0,0},1,1,1,3572,1,I,"""",0, {""U""},"""",1,1,0,1,0,{0}},";
    public static async Task AppendToFile(string file, string content)
    {
        await using var sr = GetFileWriter(file, FileMode.Append);

        await sr.WriteLineAsync(content);
    }

    public static async Task ReplaceInFile(string file, string oldString, string newString)
    {
        var content = await File.ReadAllTextAsync(file);

        var newContent = content.Replace(oldString, newString);
        
        await using var sr = GetFileWriter(file, FileMode.Truncate);
        await sr.WriteLineAsync(newContent);
    }

    private static StreamWriter GetFileWriter(string file, FileMode mode)
    {
        var stream = File.Open(file, mode, FileAccess.Write, FileShare.ReadWrite);
        return new StreamWriter(stream);
    }
}