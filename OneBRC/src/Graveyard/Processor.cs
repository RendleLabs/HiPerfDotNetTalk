using System.Runtime.InteropServices;

namespace OneBRC;

public class Processor
{
    private readonly int _chunkSize;
    private readonly int _threadCount;

    public Processor(int chunkSize, int threadCount = 0)
    {
        _chunkSize = chunkSize;
        _threadCount = threadCount > 0 ? threadCount : Environment.ProcessorCount;
    }

    public void Run(string[] args)
    {
        var filePath = Path.GetFullPath(args[0]);

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