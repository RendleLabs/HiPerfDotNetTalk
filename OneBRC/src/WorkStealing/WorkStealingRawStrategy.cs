using System.Collections.Concurrent;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using Shared;

namespace WorkStealing;

public class WorkStealingRawStrategy : IDisposable
{
    private const int ChunkSize = 1024 * 1024;
    private const byte NewLine = (byte)'\n';
    
    private readonly MemoryMappedFile _memoryMappedFile;
    private readonly long _fileSize;
    private readonly int _threadCount;
    private readonly BlockingCollection<WorkStealingChunk> _chunks = new();

    public WorkStealingRawStrategy(string filePath, int threadCount = 0)
    {
        _memoryMappedFile = MemoryMappedFile.CreateFromFile(filePath, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
        _fileSize = new FileInfo(filePath).Length;
        _threadCount = threadCount > 0 ? threadCount : Environment.ProcessorCount;
    }

    public WorkStealingRawStrategy(MemoryMappedFile memoryMappedFile, long fileSize, int threadCount)
    {
        _memoryMappedFile = memoryMappedFile;
        _fileSize = fileSize;
        _threadCount = threadCount;
    }

    public void Dispose() => _memoryMappedFile.Dispose();

    public unsafe ICollection<ValueCounter> Run()
    {
        using var view = _memoryMappedFile.CreateViewAccessor(0L, _fileSize, MemoryMappedFileAccess.Read);
        byte* pointer = null;
        view.SafeMemoryMappedViewHandle.AcquirePointer(ref pointer);
        long offset = 0L;
        while (offset + ChunkSize < _fileSize)
        {
            var p = pointer + offset;
            var span = new Span<byte>(p + ChunkSize, 128);
            var newLine = span.IndexOf(NewLine);
            var length = ChunkSize + newLine + 1;
            _chunks.Add(new WorkStealingChunk(p, length));
            offset += length;
        }

        {
            var length = _fileSize - offset;
            if (length > 0)
            {
                var p = pointer + offset;
                _chunks.Add(new WorkStealingChunk(p, (int)length));
            }
        }
        
        WorkStealingRawParser[] parsers = new WorkStealingRawParser[_threadCount];
        for (int i = 0; i < _threadCount; i++)
        {
            parsers[i] = new WorkStealingRawParser(_chunks);
        }
        
        Parallel.ForEach(parsers, new ParallelOptions { MaxDegreeOfParallelism = _threadCount }, p => p.Run());
        
        var final = parsers[0].Dictionary;

        for (int i = 1; i < _threadCount; i++)
        {
            var parser = parsers[i];
            foreach (var (key, counter) in parser.Dictionary)
            {
                ref var current = ref CollectionsMarshal.GetValueRefOrAddDefault(final, key, out _);
                current.Combine(counter);
            }
        }

        return final.Values;
    }
}