using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using Shared;

namespace MemoryMappedReadValueCounter;

public class MemoryMappedFileStrategyWithValueCounter
{
    private readonly int _threadCount;
    private readonly ParallelOptions _options;
    
    public MemoryMappedFileStrategyWithValueCounter(int threadCount = 0)
    {
        _threadCount = threadCount > 0 ? threadCount : Environment.ProcessorCount;
        _options = new() { MaxDegreeOfParallelism = _threadCount };
    }
    
    public ICollection<ValueCounter> Play(string filePath)
    {
        var size = new FileInfo(filePath).Length;
        
        using var mmf = MemoryMappedFile.CreateFromFile(filePath, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
        using var view = mmf.CreateViewAccessor(0, size, MemoryMappedFileAccess.Read);

        var offsets = MemoryMappedFileAnalyzer.GetOffsets(mmf, size, _threadCount);
        
        return Play(view, offsets);
    }
    
    public ICollection<ValueCounter> Play(MemoryMappedViewAccessor view, MemoryMappedFileOffset[] offsets)
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

        return final.Values;
    }
}