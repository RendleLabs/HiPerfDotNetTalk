namespace OneBRC;

public ref struct LineEnumerator
{
    private const byte NewLine = (byte)'\n';
    private const byte CarriageReturn = (byte)'\r';
    private static readonly byte[] TrimCharacters = [NewLine, CarriageReturn];
    private readonly ReadOnlySpan<byte> _span;
    private int _start;

    public LineEnumerator(ReadOnlySpan<byte> span)
    {
        _span = span;
    }

    public bool MoveNext()
    {
        if (_start >= _span.Length) return false;
        var span = _span[_start..];
        int end = span.IndexOf(NewLine);
        if (end < 0)
        {
            Rest = span;
            return false;
        }

        Current = span[..end].TrimEnd(NewLine);
        _start += end + 1;
        return true;
    }

    public ReadOnlySpan<byte> Current;

    public ReadOnlySpan<byte> Rest;
}