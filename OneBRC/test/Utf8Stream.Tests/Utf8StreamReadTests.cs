using System.Text;

namespace Utf8Stream.Tests;

public class Utf8StreamReadTests
{
    [Fact]
    public void Basic()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(SampleData.ReplaceLineEndings("\n")));
        var actual = Utf8StreamRead.Process(stream);
        Assert.Equal(3, actual.Count);
    }

    [Fact]
    public void Generated()
    {
        using var stream = new MemoryStream(GenerateData());
        var actual = Utf8StreamRead.Process(stream);
        Assert.Equal(4, actual.Count);
    }
    
    private const string SampleData = """
                                      London;11.000
                                      Brighton;13.200
                                      Edinburgh;9.000
                                      London;12.500
                                      Brighton;14.900
                                      Edinburgh;10.100
                                      """;
    

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