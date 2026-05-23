using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace LatestDirs;

public static class GitInterop
{
    public static async Task<DateTimeOffset> GetLastCommitDate(string repoPath)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = "log -1 --format=%ct",
            WorkingDirectory = repoPath,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo);
        if (process == null) return DateTimeOffset.MinValue;

        string output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (long.TryParse(output.Trim(), out long unixTimestamp))
        {
            return DateTimeOffset.FromUnixTimeSeconds(unixTimestamp);
        }

        return DateTimeOffset.MinValue;
    }
}
