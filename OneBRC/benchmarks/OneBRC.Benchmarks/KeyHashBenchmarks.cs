using System.Text;
using BenchmarkDotNet.Attributes;

namespace OneBRC.Benchmarks;

public class KeyHashBenchmarks
{
    private static readonly byte[][] Data = Stations.All().Select(s => Encoding.UTF8.GetBytes($"{s};")).ToArray();

    [Benchmark(Baseline = true)]
    public long WithMemoryMarshal()
    {
        long n = 0;
        foreach (var chunk in Data)
        {
            unchecked
            {
                n += KeyHash.LongKey(chunk);
            }
        }

        return n;
    }
    
    [Benchmark]
    public long WithMaths()
    {
        long n = 0;
        foreach (var chunk in Data)
        {
            unchecked
            {
                n += KeyHash.FastKey(chunk);
            }
        }

        return n;
    }
    
    [Benchmark]
    public unsafe long WithUnsafe()
    {
        long n = 0;
        foreach (var chunk in Data)
        {
            fixed (byte* p = chunk)
            {
                var c = p;
                unchecked
                {
                    n += KeyHash.FastKey(ref c);
                }
            }
        }
    
        return n;
    }
    
    
    [Benchmark]
    public unsafe long WithHashing()
    {
        long n = 0;
        foreach (var chunk in Data)
        {
            fixed (byte* p = chunk)
            {
                var c = p;
                unchecked
                {
                    n += KeyHash.FasterKey(ref c);
                }
            }
        }
    
        return n;
    }
}