dotnet publish -c Release ./src/NativeOneBRC/

$sw = [System.Diagnostics.Stopwatch]::StartNew()

./src/NativeOneBRC/bin/Release/net8.0/win-x64/publish/NativeOneBRC.exe ./billion.txt

$sw.Stop()
$sw.Elapsed
