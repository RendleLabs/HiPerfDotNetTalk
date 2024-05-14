using System.Text;
using BenchmarkDotNet.Attributes;
using StringParsing;
using Utf8Stream;

namespace ParsingBenchmarks;

[MemoryDiagnoser, DisassemblyDiagnoser]
public class FileParseBenchmarks
{
    private static readonly string FilePath = Path.GetTempFileName();

    [GlobalSetup]
    public void GlobalSetup()
    {
        using var stream = File.Create(FilePath);
        stream.Write(GenerateData());
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        File.Delete(FilePath);
    }

    [Benchmark(Baseline = true)]
    public int Utf8StreamCheatNames()
    {
        using var stream = File.OpenRead(FilePath);
        return Utf8StreamReadCheatNames.Process(stream).Count;
    }

    [Benchmark]
    public int Utf8StreamChunk()
    {
        using var stream = File.OpenRead(FilePath);
        return Utf8StreamReadCheatNames.Process(stream).Count;
    }

    private static byte[] GenerateData()
    {
        var random = new Random(42);
        string[] cities = ["Belgrade", "Novi Sad", "Niš", "Kragujevac"];
        var builder = new StringBuilder("Belgrade;11.000");

        for (int i = 0; i < 9999; i++)
        {
            var city = cities[random.Next(4)];
            var value = (random.NextSingle() - 0.5f) * 20;
            builder.Append($"\n{city};{value:F3}");
        }

        return Encoding.UTF8.GetBytes(builder.ToString());
    }
}