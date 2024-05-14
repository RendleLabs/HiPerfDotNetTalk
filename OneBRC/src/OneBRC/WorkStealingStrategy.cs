using System.Collections.Concurrent;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace OneBRC;

public class WorkStealingStrategy
{
    private const int ChunkSize = 1024 * 1024 * 32;
    private const byte NewLine = (byte)'\n';
    
    private readonly MemoryMappedFile _memoryMappedFile;
    private readonly long _fileSize;
    private readonly int _threadCount;
    private readonly BlockingCollection<WorkStealingChunk> _chunks = new();

    public WorkStealingStrategy(MemoryMappedFile memoryMappedFile, long fileSize, int threadCount)
    {
        _memoryMappedFile = memoryMappedFile;
        _fileSize = fileSize;
        _threadCount = threadCount;
    }

    public unsafe void Run()
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
            var p = pointer + offset;
            var length = _fileSize - offset;
            _chunks.Add(new WorkStealingChunk(p, (int)length));
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

        foreach (var counter in final.Values.OrderBy(v => v.Name))
        {
            Console.WriteLine($"{counter.Name} : {counter.Min:F1}/{counter.Mean:F1}/{counter.Max:F1}");
        }
    }
}