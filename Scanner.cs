using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Enumeration;
using System.Linq;
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

        await Parallel.ForEachAsync(topLevelDirs, async (dir, ct) =>
        {
            if (ExcludedFolders.Any(e => dir.EndsWith(Path.DirectorySeparatorChar + e) || dir.EndsWith(Path.AltDirectorySeparatorChar + e)))
                return;

            if (byGit)
            {
                var gitResult = await ScanGitRepo(dir);
                if (gitResult != null)
                {
                    lock (results) results.Add(gitResult);
                }
            }
            else
            {
                var latestChange = GetLatestChange(dir, maxDepth);
                lock (results) results.Add(new ScanResult(dir, latestChange));
            }
        });

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
            ReturnSpecialDirectories = false
        };

        var latest = DateTime.MinValue;

        try
        {
            var enumerable = new FileSystemEnumerable<DateTime>(
                path,
                (ref FileSystemEntry entry) => entry.LastWriteTimeUtc.UtcDateTime,
                options)
            {
                ShouldIncludePredicate = (ref FileSystemEntry entry) => true
            };

            foreach (var time in enumerable)
            {
                if (time > latest) latest = time;
            }
        }
        catch (Exception)
        {
            // Log or ignore specific access issues
        }

        if (latest == DateTime.MinValue)
        {
            try { latest = Directory.GetLastWriteTimeUtc(path); }
            catch { latest = DateTime.MinValue; }
        }

        return new DateTimeOffset(latest, TimeSpan.Zero);
    }
}
