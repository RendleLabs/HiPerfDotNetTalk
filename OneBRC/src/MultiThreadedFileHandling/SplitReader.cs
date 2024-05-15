using System.Runtime.CompilerServices;
using System.Text;
using OneBRC;
using Shared;

namespace MultiThreadedFileHandling;

public class SplitReader
{
    private const byte Semicolon = (byte)';';
    
    private readonly Dictionary<long, Counter> _dictionary = new();
    private readonly string _filePath;
    private readonly int _chunkSize;
    private readonly long _startPosition;
    private readonly long _endPosition;
    private int _rows;

    public SplitReader(string filePath, int chunkSize, long startPosition, long endPosition)
    {
        _filePath = filePath;
        _chunkSize = chunkSize;
        _startPosition = startPosition;
        _endPosition = endPosition;
    }

    public void Run()
    {
        using var handle = File.OpenHandle(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

        var reader = new ChunkReader(handle, _startPosition, _endPosition, _chunkSize);

        ReadOnlySpan<byte> chunk;
        
        ReadOnlySpan<byte> rest = ReadOnlySpan<byte>.Empty;

        while ((chunk = reader.ReadNext(rest)).Length > 0)
        {
            var enumerator = new LineEnumerator(chunk);

            while (enumerator.MoveNext())
            {
                ++_rows;
                ParseLine(enumerator.Current);
            }

            rest = enumerator.Rest;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ParseLine(ReadOnlySpan<byte> line)
    {
        var span = line;
        int scIndex = span.IndexOf(Semicolon);
        if (scIndex < 0) return;
        int value = FastParse.FastParseIntFromFloat(span[(scIndex + 1)..]);
        
        var key = line[..scIndex];
        var longKey = KeyHash.LongKey(key);

        if (!_dictionary.TryGetValue(longKey, out var counter))
        {
            _dictionary[longKey] = counter = new(Encoding.UTF8.GetString(key));
        }

        counter.Record(value);
    }

    public Dictionary<long, Counter> Dictionary => _dictionary;

    public int RowsProcessed => _rows;
}