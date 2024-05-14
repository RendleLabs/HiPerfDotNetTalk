using System.Diagnostics;
using Shared;

var stopwatch = Stopwatch.StartNew();

var dict = new Dictionary<string, Counter>();

var filePath = Path.GetFullPath(args[0]);

int count = 0;

using (var reader = File.OpenText(filePath))
{
    while (!reader.EndOfStream)
    {
        if (++count % 1000000 == 0)
        {
            Console.WriteLine(count);
        }
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
}

foreach (var (key, counter) in dict.OrderBy(p => p.Key))
{
    Console.WriteLine($"{key} : {counter.Min:F1}/{counter.Mean:F1}/{counter.Max:F1}");
}

stopwatch.Stop();
Console.WriteLine(stopwatch.Elapsed);