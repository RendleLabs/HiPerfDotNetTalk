using System.IO.MemoryMappedFiles;

namespace MemoryMappedRead;

public static class MemoryMappedFileAnalyzer
{
    private const byte NewLine = (byte)'\n';
    private const int Megabyte = 1024 * 1024;

    public static unsafe MemoryMappedFileOffset[] GetOffsets(MemoryMappedFile mmf, long size, int threadCount)
    {
        using var accessor = mmf.CreateViewAccessor(0, size, MemoryMappedFileAccess.Read);
        byte* pointer = null;
        accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref pointer);

        try
        {
            var offsets = new List<MemoryMappedFileOffset>();
            var roughChunkSize = size / threadCount;

            var over = roughChunkSize % Megabyte;
        
            var actualChunkSize = (roughChunkSize - over) + Megabyte;

            long offset = 0;
        
            while (offset + actualChunkSize < size)
            {
                var span = new Span<byte>(pointer + offset, 1024);
                int newline = span.IndexOf(NewLine);
                offsets.Add(new MemoryMappedFileOffset(offset, actualChunkSize + newline));
                offset += actualChunkSize + newline + 1;
            }
        
            offsets.Add(new MemoryMappedFileOffset(offset, size - offset));

            return offsets.ToArray();
        }
        finally
        {
            accessor.SafeMemoryMappedViewHandle.ReleasePointer();
        }
    }
}