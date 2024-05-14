using System.Text;
using CommunityToolkit.HighPerformance.Buffers;
using OneBRC;
using Shared;

namespace Utf8Stream;

public static class Utf8StreamReadCheatNames
{
    private const byte NewLine = (byte)'\n';
    private const byte Semicolon = (byte)';';
    
    public static Dictionary<string, Counter> Process(Stream stream)
    {
        if (!stream.CanSeek) throw new ArgumentException("Stream must support Seek", nameof(stream));
        
        var dictionary = new Dictionary<long, Counter>();
        
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

        var result = new Dictionary<string, Counter>();
        
        foreach (var value in dictionary.Values)
        {
            result[value.City] = value;
        }

        return result;
    }

    private static void ProcessLine(ReadOnlySpan<byte> bytes, Dictionary<long, Counter> dictionary)
    {
        int split = bytes.IndexOf(Semicolon);
        var nameBytes = bytes[..split];
        var key = KeyHash.LongKey(in nameBytes);
        var valueBytes = bytes[(split + 1)..];
        var value = FastParse.FastParseFloat(valueBytes);
        if (!dictionary.TryGetValue(key, out var counter))
        {
            var name = Encoding.UTF8.GetString(nameBytes);
            dictionary[key] = counter = new Counter(name);
        }
        counter.Record(value);
    }
}