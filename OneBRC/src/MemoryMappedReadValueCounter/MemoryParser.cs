using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Text;
using OneBRC;
using Shared;

namespace MemoryMappedReadValueCounter;

internal class MemoryParser
{
    private const byte NewLine = (byte)'\n';
    private const byte Semicolon = (byte)';';

    private readonly Dictionary<long, ValueCounter> _dictionary = new();
    private readonly MemoryMappedViewAccessor _viewAccessor;
    private readonly MemoryMappedFileOffset _fileOffset;

    public MemoryParser(MemoryMappedViewAccessor viewAccessor, MemoryMappedFileOffset fileOffset)
    {
        _viewAccessor = viewAccessor;
        _fileOffset = fileOffset;
    }

    public Dictionary<long, ValueCounter> Dictionary => _dictionary;

    public unsafe void Run()
    {
        byte* pointer = null;
        var viewHandle = _viewAccessor.SafeMemoryMappedViewHandle;
        viewHandle.AcquirePointer(ref pointer);
        pointer += _fileOffset.Offset;

        const int chunkSize = 1024 * 1024 * 16;

        long remaining = _fileOffset.Length;

        while (remaining > 0)
        {
            var spanLength = (int)Math.Min(remaining, chunkSize);
            ReadOnlySpan<byte> span = new Span<byte>(pointer, spanLength);
            if (span.Length > remaining)
            {
                span = span[..(int)remaining];
                remaining = 0L;
            }

            Run(ref span, _dictionary);

            int advance = chunkSize - span.Length;
            remaining -= advance;
            pointer += advance;
        }
        
        viewHandle.ReleasePointer();
    }

    private static void Run(ref ReadOnlySpan<byte> memory, Dictionary<long, ValueCounter> dictionary)
    {
        int newline;

        while ((newline = memory.IndexOf(NewLine)) > -1)
        {
            var line = memory[..newline];
            int sc = line.IndexOf(Semicolon);
            if (sc < 1)
            {
                memory = memory[(newline + 1)..];
                continue;
            }
            
            var number = line[(sc + 1)..];
            var value = FastParse.FastParseIntFromFloat(number);
            
            var name = line[..sc];
            var key = KeyHash.FastKey(name);
            
            ref var counter = ref CollectionsMarshal.GetValueRefOrAddDefault(dictionary, key, out bool exists);
            
            if (!exists)
            {
                counter.SetName(Encoding.UTF8.GetString(name));
            }
            
            counter.Record(value);
            memory = memory[(newline + 1)..];
        }
    }
}