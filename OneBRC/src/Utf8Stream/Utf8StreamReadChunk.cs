using System.Text;
using OneBRC;
using Shared;

namespace Utf8Stream;

public static class Utf8StreamReadChunk
{
    private const int ChunkSize = 16 * 1024;
    private const byte NewLine = (byte)'\n';
    private const byte Semicolon = (byte)';';

    public static Dictionary<string, Counter> Process(Stream stream)
    {
        if (!stream.CanSeek) throw new ArgumentException("Stream must support Seek", nameof(stream));
        
        var dictionary = new Dictionary<long, Counter>();
        
        var bytes = new byte[ChunkSize];

        while (true)
        {
            int read = stream.Read(bytes, 0, ChunkSize);
            if (read == 0) break;
            var span = bytes.AsSpan(0, read);

            while (span.Length > 0)
            {
                int end = span.IndexOf(NewLine);
                if (end < 0)
                {
                    // At end of stream
                    if (stream.Position == stream.Length)
                    {
                        ProcessLine(span, dictionary);
                        break;
                    }
                    
                    span.CopyTo(bytes);
                    read = stream.Read(bytes, span.Length, ChunkSize - span.Length);
                    span = bytes.AsSpan(0, span.Length + read);
                    continue;
                }
                
                ProcessLine(span.Slice(0, end), dictionary);
                span = span.Slice(end + 1);
            }
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
        var name = Encoding.UTF8.GetString(nameBytes);
        var valueBytes = bytes[(split + 1)..];
        var value = FastParse.FastParseFloat(valueBytes);

        var key = KeyHash.LongKey(nameBytes);
        if (!dictionary.TryGetValue(key, out var counter))
        {
            dictionary[key] = counter = new Counter(name);
        }
        counter.Record(value);
    }
}