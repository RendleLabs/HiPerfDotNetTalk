dotnet build --no-restore -c Release ./src/OneBRC/

$sw = [System.Diagnostics.Stopwatch]::StartNew()

./src/OneBRC/bin/Release/net8.0/OneBRC.exe ./billion.txt

$sw.Stop()
$sw.Elapsed
