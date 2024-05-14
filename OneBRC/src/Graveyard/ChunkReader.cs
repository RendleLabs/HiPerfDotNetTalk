using Microsoft.Win32.SafeHandles;

namespace OneBRC;

public class ChunkReader
{
    private readonly SafeFileHandle _handle;
    private readonly long _endPosition;
    private readonly byte[] _buffer;
    private long _offset;

    public ChunkReader(SafeFileHandle handle, long startPosition, long endPosition, int bufferSize)
    {
        _handle = handle;
        _offset = startPosition;
        _endPosition = endPosition > 0 ? endPosition : RandomAccess.GetLength(handle);
        _buffer = GC.AllocateUninitializedArray<byte>(bufferSize, true);
    }

    public ReadOnlySpan<byte> ReadNext() => ReadNext(ReadOnlySpan<byte>.Empty);

    public ReadOnlySpan<byte> ReadNext(ReadOnlySpan<byte> remainder)
    {
        if (_offset >= _endPosition) return ReadOnlySpan<byte>.Empty;
        
        int remainderLength = remainder.Length;
        
        Span<byte> span = _buffer;
        if (remainderLength > 0)
        {
            remainder.CopyTo(_buffer);
            span = span.Slice(remainderLength);
        }

        if (_offset + _buffer.Length > _endPosition)
        {
            int remaining = (int)(_endPosition - _offset);
            span = span[..remaining];
        }

        int read = RandomAccess.Read(_handle, span, _offset);

        _offset += read;
        
        return _buffer.AsSpan().Slice(0, remainderLength + read);
    }
}