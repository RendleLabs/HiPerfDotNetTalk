using System.Diagnostics;
using WorkStealing;

var stopwatch = Stopwatch.StartNew();

var filePath = Path.GetFullPath(args[0]);
using var wss = new WorkStealingRawStrategy(filePath);
var values = wss.Run();

foreach (var counter in values.OrderBy(c => c.Name))
{
    Console.WriteLine($"{counter.Name}: {counter.Min:F1}/{counter.Max:F1}/{counter.Mean:F1}");
}
stopwatch.Stop();

Console.WriteLine($"{stopwatch.Elapsed}");
