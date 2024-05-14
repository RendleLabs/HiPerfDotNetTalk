using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Text;
using Shared;

namespace OneBRC;

public class WorkStealingRawParser
{
    private const byte Semicolon = (byte)';';
    
    private readonly BlockingCollection<WorkStealingChunk> _chunks;
    private readonly Dictionary<int, ValueCounter> _dictionary = new();

    public WorkStealingRawParser(BlockingCollection<WorkStealingChunk> chunks)
    {
        _chunks = chunks;
    }

    public Dictionary<int, ValueCounter> Dictionary => _dictionary;

    public unsafe void Run()
    {
        while (_chunks.TryTake(out var chunk))
        {
            var span = new ReadOnlySpan<byte>(chunk.Pointer, chunk.Length);
            Run(chunk.Pointer, chunk.Length, _dictionary);
        }
    }
    
    private static unsafe void Run(byte* pointer, long length, Dictionary<int, ValueCounter> dictionary)
    {
        byte* end = pointer + length;

        while (pointer < end)
        {
            var nameStart = pointer;
            
            var key = KeyHash.FasterKey(ref pointer);
            pointer++; // Semicolon
            var value = FastParse.FastParseIntFromFloat(ref pointer);
            pointer++; // NewLine
            
            ref var counter = ref CollectionsMarshal.GetValueRefOrAddDefault(dictionary, key, out bool exists);

            if (!exists)
            {
                counter.Initialize(value, GetName(nameStart));
            }
            else
            {
                counter.Record(value);
            }
        }
    }

    private static unsafe string GetName(byte* pointer)
    {
        for (int i = 1; i < 128; i++)
        {
            if (*(pointer + i) == Semicolon)
            {
                return Encoding.UTF8.GetString(pointer, i);
            }
        }

        throw new InvalidOperationException();
    }
}