using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace BslLogExporter.Tests.Helpers;

public static class Extensions
{
    public static IConfigurationBuilder AddObject(this IConfigurationBuilder builder, object obj)
    {
        var stream = new MemoryStream();
        JsonSerializer.Serialize(stream, obj);
        stream.Seek(0, SeekOrigin.Begin);
        return builder.AddJsonStream(stream);
    }
}