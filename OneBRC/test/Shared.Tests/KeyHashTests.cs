using System.Text;
using OneBRC;

namespace Shared.Tests;

public class KeyHashTests
{
    [Fact]
    public void LongKey()
    {
        var bytes = "London;"u8;
        var basic = KeyHash.LongKey(bytes);
        Assert.True(basic > 0L);
    }
    
    [Fact]
    public void FastKey()
    {
        var bytes = "London;"u8;
        var basic = KeyHash.FastKey(bytes);
        Assert.True(basic > 0L);
    }
    
    [Fact]
    public unsafe void FasterKey()
    {
        var bytes = "London;"u8.ToArray();
        fixed (byte* b = bytes)
        {
            var c = b;
            var basic = KeyHash.FasterKey(ref c);
            Assert.True(basic != 0L);
        }
    }
}