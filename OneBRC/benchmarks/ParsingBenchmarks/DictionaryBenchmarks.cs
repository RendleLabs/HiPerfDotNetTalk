using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using Shared;

namespace ParsingBenchmarks;

public class DictionaryBenchmarks
{
    private static readonly long[] Keys = Enumerable.Range(1000, 10000).Select(n => (long)n).ToArray();

    [Benchmark(Baseline = true)]
    public int TryGet()
    {
        var dict = new Dictionary<long, Counter>();

        foreach (var key in Keys)
        {
            if (!dict.TryGetValue(key, out var counter))
            {
                dict[key] = counter = new(key.ToString());
            }
        }

        return dict.Count;
    }

    [Benchmark]
    public int CollectionMarshal()
    {
        var dict = new Dictionary<long, ValueCounter>();

        foreach (var key in Keys)
        {
            ref var valueCounter = ref CollectionsMarshal.GetValueRefOrAddDefault(dict, key, out bool existed);
            if (!existed)
            {
                valueCounter.SetName(key.ToString());
            }
        }

        return dict.Count;
    }

    private class Counter
    {
        public Counter(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }

    private struct ValueCounter
    {
        public string Name { get; private set; }

        public void SetName(string name) => Name = name;
    }
}