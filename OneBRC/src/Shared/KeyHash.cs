using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OneBRC;

public static class KeyHash
{
    private const byte Semicolon = (byte)';';
    private const byte NewLine = (byte)'\n';
    
    public static long LongKey(in ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length >= 8)
        {
            return MemoryMarshal.Read<long>(bytes);
        }

        Span<byte> span = stackalloc byte[8];
        span.Clear();
        bytes.CopyTo(span);
        return MemoryMarshal.Read<long>(span);
    }
    
    public static long FastKey(in ReadOnlySpan<byte> bytes)
    {
        long key = 0;
        int shift = 64;
        for (int i = 0, l = bytes.Length; i < l; i++)
        {
            shift -= 8;
            if (shift == 0)
            {
                key += bytes[i];
                break;
            }
            long v = bytes[i];
            key += v << shift;
        }

        return key;
    }
    
    public static unsafe long FastKey(ref byte* pointer)
    {
        long key = 0;
        int shift = 64;

        while (*pointer != Semicolon)
        {
            long b = *pointer;
            shift -= 8;
            if (shift == 0) break;
            key += b << shift;
            pointer++;
        }

        while (*pointer != Semicolon)
        {
            pointer++;
        }
        
        return key;
    }
    
    public static unsafe int FasterKey(ref byte* pointer)
    {
        int key = 1430287;
        
        while (*pointer != Semicolon)
        {
            key = key * 7302013 ^ *pointer;
            ++pointer;
        }
        
        return key;
    }
}