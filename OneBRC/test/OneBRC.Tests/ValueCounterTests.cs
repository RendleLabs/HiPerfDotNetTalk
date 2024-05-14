using System.Runtime.InteropServices;
using Shared;

namespace OneBRC.Tests;

public class ValueCounterTests
{
    [Fact]
    public void WorksWithCollectionsMarshal()
    {
        var dict = new Dictionary<ReadOnlyMemory<byte>, ValueCounter>();
        ReadOnlyMemory<byte> key = "London"u8.ToArray();

        ref var counter = ref CollectionsMarshal.GetValueRefOrAddDefault(dict, key, out _);
        counter.Record(1);
        
        ref var counter2 = ref CollectionsMarshal.GetValueRefOrAddDefault(dict, key, out _);
        counter2.Record(100);
        
        Assert.True(dict.TryGetValue(key, out counter));
        Assert.Equal(1d, counter.Min, 3);
        Assert.Equal(100d, counter.Max, 3);
    }
}