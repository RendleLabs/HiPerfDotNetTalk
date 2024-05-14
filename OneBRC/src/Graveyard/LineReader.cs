using System.Runtime.InteropServices;

namespace OneBRC;

public ref struct LineReader
{
    private const byte NewLine = (byte)'\n';
    private const byte CarriageReturn = (byte)'\r';
    private const int BufferSize = 32 + 1024;
    
    private byte[] _arrayA = new byte[BufferSize];
    private byte[] _arrayB = new byte[BufferSize];
    private bool _b;
    private Memory<byte> _buffer;
    private ReadOnlyMemory<byte> _current;

    private Stream? _stream;

    public LineReader(Stream stream)
    {
        _stream = stream;
    }

    public bool MoveNext()
    {
        if (_stream is null) return false;
        
        var buffer = _buffer;

        if (buffer.Length == 0)
        {
            buffer = _arrayA;
            _b = true;
            var length = _stream.Read(buffer.Span);
            if (length == 0) return false;
            buffer = _buffer = buffer.Slice(0, length);
        }
        
        retry:

        int newLineIndex = buffer.Span.IndexOf(NewLine);

        if (newLineIndex > -1)
        {
            _current = buffer.Slice(0, newLineIndex).TrimEnd(CarriageReturn);
            _buffer = buffer.Slice(newLineIndex + 1);
            return true;
        }

        if (buffer.Length > 0)
        {
            var next = (_b ? _arrayB : _arrayA).AsMemory();
            _b = !_b;
            buffer.Span.CopyTo(next.Span);
            var read = _stream.Read(next.Span.Slice(buffer.Length));
            if (read > 0)
            {
                buffer = _buffer = next.Slice(0, buffer.Length + read);
                goto retry;
            }

            _current = buffer;
            _stream = null;
            return true;
        }
        
        return false;
    }

    public ReadOnlyMemory<byte> Current
    {
        get
        {
            if (_current.Length == 0) throw new InvalidOperationException();
            return _current;
        }
    }
}