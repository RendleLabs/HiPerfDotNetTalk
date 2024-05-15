using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Text;
using OneBRC;
using Shared;

namespace WorkStealing;

public class WorkStealingRawParser
{
    private const byte Semicolon = (byte)';';
    
    private readonly BlockingCollection<WorkStealingChunk> _chunks;
    private readonly Dictionary<long, ValueCounter> _dictionary = new();

    public WorkStealingRawParser(BlockingCollection<WorkStealingChunk> chunks)
    {
        _chunks = chunks;
    }

    public Dictionary<long, ValueCounter> Dictionary => _dictionary;

    public unsafe void Run()
    {
        var threadId = Thread.CurrentThread.ManagedThreadId;
        while (_chunks.TryTake(out var chunk))
        {
            Run(chunk.Pointer, chunk.Length, _dictionary);
        }
    }
    
    private static unsafe void Run(byte* pointer, long length, Dictionary<long, ValueCounter> dictionary)
    {
        byte* end = pointer + length;

        while (pointer < end)
        {
            var nameStart = pointer;
            
            var key = KeyHash.FastKey(ref pointer);
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