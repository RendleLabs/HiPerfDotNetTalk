using System.IO.MemoryMappedFiles;
using System.Text;
using MemoryMappedRead;

namespace FileHandling.Tests;

public class MemoryMappedReadTests
{
    [Fact]
    public void BasicTest()
    {
        var filePath = CreateFile();
        var target = new MemoryMappedFileStrategy();
        var actual = FileHandleRead.Process(filePath);
        Assert.NotEmpty(actual);
        File.Delete(filePath);
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
        var builder = new StringBuilder("Belgrade;11.000");

        for (int i = 0; i < 999; i++)
        {
            var city = cities[random.Next(4)];
            var value = (random.NextSingle() - 0.5f) * 20;
            builder.Append($"\n{city};{value:F3}");
        }

        return Encoding.UTF8.GetBytes(builder.ToString());
    }
}