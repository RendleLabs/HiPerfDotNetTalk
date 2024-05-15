using System.IO.MemoryMappedFiles;
using System.Text;
using WorkStealing;

namespace FileHandling.Tests;

public class WorkStealingTests
{
    [Fact]
    public void BasicTest()
    {
        var filePath = CreateFile();
        var size = new FileInfo(filePath).Length;
        var mmf = MemoryMappedFile.CreateFromFile(filePath, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
        var wss = new WorkStealingRawStrategy(mmf, size, Environment.ProcessorCount * 2);
        var actual = wss.Run();
        Assert.NotEmpty(actual);
        mmf.Dispose();
    }

    private static string CreateFile()
    {
        var filePath = Path.GetTempFileName();
        using var stream = File.Create(filePath);
        stream.Write(GenerateData());
        return filePath;
    }
    
    private static byte[] GenerateData()
    {
        var random = new Random(42);
        string[] cities = ["Belgrade", "Novi Sad", "Niš", "Kragujevac"];
        var builder = new StringBuilder("Belgrade;11.000\n");

        for (int i = 0; i < 9999; i++)
        {
            var city = cities[random.Next(4)];
            var value = (random.NextSingle() - 0.5f) * 20;
            builder.Append($"{city};{value:F3}\n");
        }

        return Encoding.UTF8.GetBytes(builder.ToString());
    }
}