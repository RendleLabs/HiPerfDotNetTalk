using System.Runtime.InteropServices;

namespace OneBRC;

public static class KeyHash
{
    public static long LongKey(ReadOnlySpan<byte> bytes)
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
    
    public static long FastKey(ReadOnlySpan<byte> bytes)
    {
        long key = 0;
        int shift = 64;
        for (int i = 0, l = bytes.Length; i < l && shift > 0; i++)
        {
            shift -= 8;
            long v = bytes[i];
            key += v << shift;
        }

        return key;
    }
}