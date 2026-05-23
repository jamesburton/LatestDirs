using BenchmarkDotNet.Running;
using LatestDirs.Benchmarks;

var summary = BenchmarkRunner.Run<ScannerBenchmarks>();
