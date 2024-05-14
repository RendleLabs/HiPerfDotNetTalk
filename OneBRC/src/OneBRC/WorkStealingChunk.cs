namespace OneBRC;

public unsafe struct WorkStealingChunk
{
    public readonly byte* Pointer;
    public readonly int Length;

    public WorkStealingChunk(byte* pointer, int length)
    {
        Pointer = pointer;
        Length = length;
    }
}