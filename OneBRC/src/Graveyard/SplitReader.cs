using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace OneBRC;

public class SplitReader
{
    private const byte Semicolon = (byte)';';
    
    private readonly Dictionary<long, ValueCounter> _dictionary = new();
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
        float value = FastParse.FastParseFloat(span[(scIndex + 1)..]);
        // if (!float.TryParse(span[(scIndex + 1)..], out float value)) return;
        
        var key = line[..scIndex];
        var longKey = KeyHash.LongKey(key);

        ref var counter = ref CollectionsMarshal.GetValueRefOrAddDefault(_dictionary, longKey, out bool existed);

        if (!existed)
        {
            var name = Encoding.UTF8.GetString(key);
            counter.SetName(name);
        }

        counter.Record(value);
    }

    private static long LongKey(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length >= 8)
        {
            return MemoryMarshal.Read<long>(bytes);
        }

        Span<byte> span = stackalloc byte[8];
        bytes.CopyTo(span);
        return MemoryMarshal.Read<long>(span);
    }

    public Dictionary<long, ValueCounter> Dictionary => _dictionary;

    public int RowsProcessed => _rows;
}