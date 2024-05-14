using System.Text;
using BenchmarkDotNet.Attributes;
using Shared;

namespace OneBRC.Benchmarks;

public class FastParseBenchmarks
{
    private static byte[][] Data;
    private static byte[][] DataWithNewlines;

    [GlobalSetup]
    public void GenerateData()
    {
        Data = GenerateData("").ToArray();
        DataWithNewlines = GenerateData("\n").ToArray();
    }

    private static IEnumerable<byte[]> GenerateData(string suffix)
    {
        for (int i = 0; i < 1024; i++)
        {
            var value = (Random.Shared.NextSingle() - 0.5f) * 50f;
            yield return Encoding.UTF8.GetBytes(value.ToString("F3") + suffix);
        }
    }

    [Benchmark(Baseline = true)]
    public double ParseFloat()
    {
        double result = 0d;
        var data = Data;
        foreach (var line in data)
        {
            result += FastParse.FastParseFloat(line);
        }

        return result;
    }

    [Benchmark]
    public long ParseLong()
    {
        long result = 0L;
        var data = Data;
        foreach (var line in data)
        {
            result += FastParse.FastParseIntFromFloat(line);
        }

        return result;
    }
}