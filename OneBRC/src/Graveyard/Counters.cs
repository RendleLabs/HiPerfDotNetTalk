using System.Text;

namespace OneBRC;

public class Counters
{
    private readonly Dictionary<ReadOnlyMemory<byte>, string> _keys = new();
    private readonly SortedDictionary<string, Counter> _counters = new();

    public void Count(ReadOnlyMemory<byte> key, float value)
    {
        if (!_keys.TryGetValue(key, out var str))
        {
            _keys[key] = str = Encoding.UTF8.GetString(key.Span);
            (_counters[str] = new()).Count(value);
        }
        else
        {
            _counters[str].Count(value);
        }
    }

    public IEnumerable<KeyValuePair<string, Counter>> All() => _counters;
}