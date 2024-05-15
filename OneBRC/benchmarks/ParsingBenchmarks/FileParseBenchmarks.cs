using System.Runtime.InteropServices;
using System.Text;
using BenchmarkDotNet.Attributes;
using GenerateData;
using MemoryMappedRead;
using MemoryMappedReadValueCounter;
using MultiThreadedFileHandling;
using OneBRC;
using StringParsing;
using Utf8Stream;
using WorkStealing;

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
        try
        {
            File.Delete(FilePath);
        }
        catch (Exception ex)
        {
            // Ignore
        }
    }

    // [Benchmark(Baseline = true)]
    // public int Utf8StreamCheatNames()
    // {
    //     using var stream = File.OpenRead(FilePath);
    //     return Utf8StreamReadCheatNames.Process(stream).Count;
    // }
    //
    // [Benchmark]
    // public int Utf8StreamChunk()
    // {
    //     using var stream = File.OpenRead(FilePath);
    //     return Utf8StreamReadCheatNames.Process(stream).Count;
    // }
    //
    // [Benchmark]
    // public int MemoryMappedFile()
    // {
    //     var target = new MemoryMappedFileStrategy();
    //     return target.Play(FilePath).Count;
    // }
    //
    [Benchmark(Baseline = true)]
    public int MemoryMappedFileWithValueCounter()
    {
        var target = new MemoryMappedFileStrategyWithValueCounter();
        return target.Play(FilePath).Count;
    }

    [Benchmark]
    public int WorkStealing()
    {
        var target = new WorkStealingRawStrategy(FilePath);
        return target.Run().Count;
    }

    private static byte[] GenerateData()
    {
        var random = new Random(42);
        var builder = new StringBuilder();
        foreach (var station in Stations.Randomize(10000))
        {
            var value = (random.NextSingle() - 0.5f) * 20;
            builder.Append($"{station};{value:F3}\n");
        }

        return Encoding.UTF8.GetBytes(builder.ToString());
    }
}