using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace OneBRC;

public class MemoryMappedFileStrategy
{
    private readonly int _threadCount;
    private readonly ParallelOptions _options;
    
    public MemoryMappedFileStrategy(int threadCount)
    {
        _threadCount = threadCount;
        _options = new() { MaxDegreeOfParallelism = threadCount };
    }
    
    public void Play(string filePath)
    {
        var size = new FileInfo(filePath).Length;
        
        using var mmf = MemoryMappedFile.CreateFromFile(filePath, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
        using var view = mmf.CreateViewAccessor(0, size, MemoryMappedFileAccess.Read);

        var offsets = MemoryMappedFileAnalyzer.GetOffsets(mmf, size, _threadCount);
        
        Play(view, offsets);
    }
    
    public void Play(MemoryMappedViewAccessor view, MemoryMappedFileOffset[] offsets)
    {
        var parsers = new MemoryParser[offsets.Length];
        
        for (int i = 0, l = offsets.Length; i < l; i++)
        {
            parsers[i] = new MemoryParser(view, offsets[i]);
        }

        Parallel.ForEach(parsers, _options, p => p.Run());
        
        var final = parsers[0].Dictionary;

        for (int i = 1; i < parsers.Length; i++)
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