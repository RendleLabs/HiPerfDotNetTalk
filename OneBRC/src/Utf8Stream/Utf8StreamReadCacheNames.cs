using System.Text;
using CommunityToolkit.HighPerformance.Buffers;
using Shared;

namespace Utf8Stream;

public static class Utf8StreamReadCacheNames
{
    private const byte NewLine = (byte)'\n';
    private const byte Semicolon = (byte)';';
    
    private static readonly StringPool Pool = StringPool.Shared;

    public static Dictionary<string, Counter> Process(Stream stream)
    {
        if (!stream.CanSeek) throw new ArgumentException("Stream must support Seek", nameof(stream));
        
        var dictionary = new Dictionary<string, Counter>();
        
        var bytes = new byte[80];

        long position = 0;

        while (true)
        {
            int read = stream.Read(bytes, 0, 80);
            if (read == 0) break;
            int end = Array.IndexOf(bytes, NewLine);
            if (end < 0)
            {
                ProcessLine(bytes.AsSpan(0, read), dictionary);
                break;
            }

            if (end == 0)
            {
                stream.Seek(1, SeekOrigin.Current);
                continue;
            }

            ProcessLine(bytes.AsSpan(0, end), dictionary);

            position += end + 1;
            stream.Seek(position, SeekOrigin.Begin);
        }

        return dictionary;
    }

    private static void ProcessLine(ReadOnlySpan<byte> bytes, Dictionary<string, Counter> dictionary)
    {
        int split = bytes.IndexOf(Semicolon);
        var nameBytes = bytes[..split];
        var name = Pool.GetOrAdd(nameBytes, Encoding.UTF8);
        var valueBytes = bytes[(split + 1)..];
        var value = FastParse.FastParseFloat(valueBytes);
        if (!dictionary.TryGetValue(name, out var counter))
        {
            dictionary[name] = counter = new Counter(name);
        }
        counter.Record(value);
    }
}