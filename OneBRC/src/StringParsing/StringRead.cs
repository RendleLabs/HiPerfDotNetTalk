using Shared;

namespace StringParsing;

public static class StringRead
{
    public static Dictionary<string, Counter> Process(Stream stream)
    {
        var dict = new Dictionary<string, Counter>();
        using var reader = new StreamReader(stream, leaveOpen: true);

        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            if (line is { Length: > 0 })
            {
                var parts = line.Split(';');
                if (parts.Length != 2) continue;

                if (!float.TryParse(parts[1], out var value)) continue;

                var key = parts[0];
                if (!dict.TryGetValue(key, out var counter))
                {
                    dict[key] = counter = new Counter(key);
                }

                counter.Record(value);
            }
        }

        return dict;
    }
}