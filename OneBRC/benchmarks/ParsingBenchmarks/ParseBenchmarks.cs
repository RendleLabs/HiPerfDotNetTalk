using System.Text;
using BenchmarkDotNet.Attributes;
using StringParsing;
using Utf8Stream;

namespace ParsingBenchmarks;

[MemoryDiagnoser, DisassemblyDiagnoser]
public class ParseBenchmarks
{
    private static readonly MemoryStream Data = new(GenerateData());

    // [Benchmark(Baseline = true)]
    public int Baseline()
    {
        Data.Position = 0;
        return StringRead.Process(Data).Count;
    }

    // [Benchmark]
    public int Utf8Stream()
    {
        Data.Position = 0;
        return Utf8StreamRead.Process(Data).Count;
    }

    // [Benchmark]
    public int Utf8StreamCacheNames()
    {
        Data.Position = 0;
        return Utf8StreamReadCacheNames.Process(Data).Count;
    }

    [Benchmark(Baseline = true)]
    public int Utf8StreamCheatNames()
    {
        Data.Position = 0;
        return Utf8StreamReadCheatNames.Process(Data).Count;
    }

    [Benchmark]
    public int Utf8StreamChunk()
    {
        Data.Position = 0;
        return Utf8StreamReadCheatNames.Process(Data).Count;
    }

    private static byte[] GenerateData()
    {
        var random = new Random(42);
        string[] cities = ["Belgrade", "Novi Sad", "Niš", "Kragujevac"];
        var builder = new StringBuilder("Belgrade;11.000");

        for (int i = 0; i < 99999; i++)
        {
            var city = cities[random.Next(4)];
            var value = (random.NextSingle() - 0.5f) * 20;
            builder.Append($"\n{city};{value:F3}");
        }

        return Encoding.UTF8.GetBytes(builder.ToString());
    }
}