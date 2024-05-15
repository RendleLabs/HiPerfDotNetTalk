namespace MemoryMappedRead;

public readonly struct MemoryMappedFileOffset
{
    public readonly long Offset;
    public readonly long Length;

    public MemoryMappedFileOffset(long offset, long length)
    {
        Offset = offset;
        Length = length;
    }
}