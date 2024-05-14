using System.Text;
using Shared;

namespace OneBRC.Tests;

public class FastParseTests
{
    [Theory]
    [InlineData("1.000", 1f)]
    [InlineData("42.000", 42f)]
    [InlineData("-42.000", -42f)]
    [InlineData("42.100", 42.1f)]
    [InlineData("42.235", 42.235f)]
    [InlineData("-42.235", -42.235f)]
    public void ParseFloat(string input, float expected)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var actual = FastParse.FastParseFloat(bytes);
        Assert.Equal(expected, actual);
    }
    
    [Theory]
    [InlineData("1.001", 1001L)]
    [InlineData("42.987", 42987L)]
    [InlineData("-42.000", -42000)]
    [InlineData("42.000", 42000)]
    [InlineData("-42.200", -42200)]
    public void ParseLong(string input, long expected)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var actual = FastParse.FastParseIntFromFloat(bytes);
        Assert.Equal(expected, actual);
    }
}