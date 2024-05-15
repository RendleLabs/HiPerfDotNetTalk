using Shared;

namespace MultiThreadedFileHandling;

public class MultiThreadedFileHandleRead
{
    private readonly int _chunkSize;
    private readonly int _threadCount;

    public MultiThreadedFileHandleRead(int chunkSize, int threadCount = 0)
    {
        _chunkSize = chunkSize;
        _threadCount = threadCount > 0 ? threadCount : Environment.ProcessorCount;
    }

    public ICollection<Counter> Run(string filePath)
    {
        var offsets = FileAnalyzer.FindOffsets(filePath, _threadCount);
        var splitReaders = new SplitReader[offsets.Length];

        for (int i = 0; i < offsets.Length - 1; i++)
        {
            splitReaders[i] = new SplitReader(filePath, _chunkSize, offsets[i], offsets[i + 1]);
        }

        splitReaders[^1] = new SplitReader(filePath, _chunkSize, offsets[^1], -1);

        Parallel.ForEach(splitReaders, new ParallelOptions
        {
            MaxDegreeOfParallelism = _threadCount,
        }, reader => reader.Run());

        var final = splitReaders[0].Dictionary;

        for (int i = 1; i < splitReaders.Length; i++)
        {
            var splitReader = splitReaders[i];
            foreach (var (key, counter) in splitReader.Dictionary)
            {
                if (final.TryGetValue(key, out var finalCounter))
                {
                    finalCounter.Combine(counter);
                }
            }
        }

        return final.Values;
    }
}