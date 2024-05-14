using System.Text;
using Microsoft.Win32.SafeHandles;
using OneBRC;
using Shared;

namespace FileHandling;

public static class FileHandleRead
{
    private const int ChunkSize = 16 * 1024;
    private const byte NewLine = (byte)'\n';
    private const byte Semicolon = (byte)';';

    public static Dictionary<string, Counter> Process(string filePath)
    {
        using var handle = File.OpenHandle(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

        var length = RandomAccess.GetLength(handle);
        var offset = 0L;

        var dictionary = new Dictionary<long, Counter>();

        var bytes = new byte[ChunkSize];

        var span = bytes.AsSpan();
        var read = RandomAccess.Read(handle, span, offset);
        span = span.Slice(0, read);

        while (span.Length > 0)
        {
            int end = span.IndexOf(NewLine);
            if (end < 0)
            {
                // At end of file
                if (offset + read == length)
                {
                    ProcessLine(span, dictionary);
                    break;
                }

                span.CopyTo(bytes);
                var readSpan = bytes.AsSpan(span.Length);
                read = RandomAccess.Read(handle, readSpan, offset);
                span = bytes.AsSpan(0, read);
                continue;
            }

            ProcessLine(span.Slice(0, end), dictionary);
            span = span.Slice(end + 1);
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