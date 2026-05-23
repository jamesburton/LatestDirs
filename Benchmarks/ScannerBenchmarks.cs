using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using LatestDirs;
using System.IO;

namespace LatestDirs.Benchmarks;

public class ScannerBenchmarks
{
    private string _testDir;

    [GlobalSetup]
    public void Setup()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "LatestDirsBenchmark");
        if (Directory.Exists(_testDir)) Directory.Delete(_testDir, true);
        Directory.CreateDirectory(_testDir);

        // Create 1000 directories with 100 files each (100k files total)
        for (int i = 0; i < 1000; i++)
        {
            var subDir = Path.Combine(_testDir, $"Dir{i}");
            Directory.CreateDirectory(subDir);
            for (int j = 0; j < 100; j++)
            {
                File.WriteAllText(Path.Combine(subDir, $"File{j}.txt"), "test");
            }
        }
    }

    [Benchmark]
    public async Task ScanAsync_Normal()
    {
        await Scanner.ScanAsync(_testDir, 10, false);
    }

    [Benchmark]
    public async Task ScanAsync_Git()
    {
        // Note: This won't find git repos in the temp dir unless we init them, 
        // but it tests the overhead of checking for .git folders.
        await Scanner.ScanAsync(_testDir, 10, true);
    }
}
