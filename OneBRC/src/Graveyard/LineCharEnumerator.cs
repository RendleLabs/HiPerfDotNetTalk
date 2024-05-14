namespace OneBRC;

public ref struct LineCharEnumerator
{
    private const char NewLine = '\n';
    private const char CarriageReturn = '\r';
    private static readonly char[] TrimCharacters = [NewLine, CarriageReturn];
    private ReadOnlyMemory<char> _memory;
    private string _string;
    private int _start, _end = -1;

    public LineCharEnumerator(ReadOnlyMemory<char> memory)
    {
        _memory = memory;
        _string = new string(memory.Span);
    }

    public bool MoveNext()
    {
        _start = _end + 1;
        if (_start >= _memory.Length) return false;
        _memory = _memory.Slice(_start);
        if ((_end = _memory.Span.IndexOf(NewLine)) < 0) return false;
        Current = _memory.Slice(0, _end);
        Rest = _memory.Slice(_end + 1);
        return true;
    }

    public ReadOnlyMemory<char> Current { get; private set; }

    public ReadOnlyMemory<char> Rest { get; private set; }
}