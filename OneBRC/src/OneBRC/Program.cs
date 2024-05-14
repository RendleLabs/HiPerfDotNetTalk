using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using OneBRC;

// const int chunkSize = 1024 * 1024;

var stopwatch = Stopwatch.StartNew();

var filePath = Path.GetFullPath(args[0]);
var size = new FileInfo(filePath).Length;
using var mmf = MemoryMappedFile.CreateFromFile(filePath, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
var wss = new WorkStealingRawStrategy(mmf, size, Environment.ProcessorCount * 2);
wss.Run();

// var mmfs = new MemoryMappedFileStrategy(Environment.ProcessorCount);
// mmfs.Play(filePath);

// var processor = new Processor(chunkSize, Environment.ProcessorCount);
// processor.Run(args);

stopwatch.Stop();

Console.WriteLine($"{stopwatch.Elapsed}");
