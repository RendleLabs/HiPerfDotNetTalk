namespace OneBRC;

public static class MemoryReader
{
    private const byte Nul = 0;
    private const byte NewLine = (byte)'\n';
    
    public static bool ReadNextLine(ref ReadOnlySpan<byte> buffer, ref ReadOnlySpan<byte> line)
    {
        var nl = buffer.IndexOf(NewLine);
        if (nl < 0)
        {
            if (buffer.Length == 0) return false;

            var nul = buffer.IndexOf(Nul);

            switch (nul)
            {
                case 0:
                    line = buffer = ReadOnlySpan<byte>.Empty;
                    return false;
                case -1:
                    line = buffer;
                    buffer = ReadOnlySpan<byte>.Empty;
                    return true;
                default:
                    line = buffer[..nul];
                    buffer = ReadOnlySpan<byte>.Empty;
                    return true;
            }
        }
        line = buffer[..nl];
        buffer = buffer[(nl + 1)..];
        return true;
    }
}