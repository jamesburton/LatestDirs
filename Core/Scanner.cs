using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Enumeration;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace LatestDirs;

public record ScanResult(string Path, DateTimeOffset LastChange, bool IsGit = false);

public static class Scanner
{
    private static readonly string[] ExcludedFolders = { "node_modules", "bin", "obj", ".git" };

    public static async Task<List<ScanResult>> ScanAsync(string rootPath, int maxDepth, bool byGit)
    {
        var results = new List<ScanResult>();
        var topLevelDirs = Directory.GetDirectories(rootPath);
        
        var channel = Channel.CreateUnbounded<ScanResult>();
        
        var producerTask = Parallel.ForEachAsync(topLevelDirs, async (dir, ct) =>
        {
            if (ExcludedFolders.Any(e => dir.EndsWith(Path.DirectorySeparatorChar + e) || dir.EndsWith(Path.AltDirectorySeparatorChar + e)))
                return;

            if (byGit)
            {
                var gitResult = await ScanGitRepo(dir);
                if (gitResult != null)
                {
                    await channel.Writer.WriteAsync(gitResult);
                }
            }
            else
            {
                var latestChange = GetLatestChange(dir, maxDepth);
                await channel.Writer.WriteAsync(new ScanResult(dir, latestChange));
            }
        });

        _ = producerTask.ContinueWith(_ => channel.Writer.Complete());

        await foreach (var result in channel.Reader.ReadAllAsync())
        {
            results.Add(result);
        }

        return results.OrderByDescending(r => r.LastChange).ToList();
    }

    private static async Task<ScanResult?> ScanGitRepo(string path)
    {
        if (!Directory.Exists(Path.Combine(path, ".git")))
            return null;

        var lastCommit = await GitInterop.GetLastCommitDate(path);
        return new ScanResult(path, lastCommit, true);
    }

    private static DateTimeOffset GetLatestChange(string path, int maxDepth)
    {
        var options = new EnumerationOptions
        {
            RecurseSubdirectories = maxDepth > 0,
            MaxRecursionDepth = maxDepth,
            IgnoreInaccessible = true,
            AttributesToSkip = FileAttributes.System | FileAttributes.Hidden | FileAttributes.ReparsePoint,
            ReturnSpecialDirectories = false,
            BufferSize = 65536 // Increased for performance
        };

        var latestTicks = DateTime.MinValue.Ticks;

        try
        {
            var enumerable = new FileSystemEnumerable<long>(
                path,
                (ref FileSystemEntry entry) => entry.LastWriteTimeUtc.Ticks,
                options)
            {
                ShouldIncludePredicate = (ref FileSystemEntry entry) => true
            };

            // Process in chunks to help JIT auto-vectorize
            const int bufferSize = 256;
            var buffer = new long[bufferSize];
            int count = 0;

            foreach (var ticks in enumerable)
            {
                buffer[count++] = ticks;
                if (count == bufferSize)
                {
                    latestTicks = Math.Max(latestTicks, GetMax(buffer.AsSpan()));
                    count = 0;
                }
            }

            if (count > 0)
            {
                latestTicks = Math.Max(latestTicks, GetMax(buffer.AsSpan(0, count)));
            }
        }
        catch (Exception)
        {
            // Ignore access issues
        }

        if (latestTicks == DateTime.MinValue.Ticks)
        {
            try { latestTicks = Directory.GetLastWriteTimeUtc(path).Ticks; }
            catch { }
        }

        return new DateTimeOffset(latestTicks, TimeSpan.Zero);
    }

    private static long GetMax(ReadOnlySpan<long> values)
    {
        if (values.IsEmpty) return DateTime.MinValue.Ticks;
        long max = values[0];
        // .NET 10 JIT will likely vectorize this simple loop
        for (int i = 1; i < values.Length; i++)
        {
            if (values[i] > max) max = values[i];
        }
        return max;
    }
}
