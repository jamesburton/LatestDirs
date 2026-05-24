using Spectre.Console;
using System;
using System.Collections.Generic;

namespace LatestDirs;

public static class UI
{
    public static void RenderTable(List<ScanResult> results, int top)
    {
        var table = new Table();
        table.AddColumn("Rank");
        table.AddColumn("Path");
        table.AddColumn("Last Change");
        table.AddColumn("Relative Time");

        var items = results.GetRange(0, Math.Min(top, results.Count));
        
        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            table.AddRow(
                (i + 1).ToString(),
                item.IsGit ? $"[blue]{item.Path}[/]" : item.Path,
                item.LastChange.ToString("yyyy-MM-dd HH:mm:ss"),
                GetRelativeTime(item.LastChange)
            );
        }

        AnsiConsole.Write(table);
    }

    private static string GetRelativeTime(DateTimeOffset time)
    {
        var delta = DateTimeOffset.Now - time;
        if (delta.TotalDays > 365) return $"{(int)(delta.TotalDays / 365)} years ago";
        if (delta.TotalDays > 30) return $"{(int)(delta.TotalDays / 30)} months ago";
        if (delta.TotalDays > 1) return $"{(int)delta.TotalDays} days ago";
        if (delta.TotalHours > 1) return $"{(int)delta.TotalHours} hours ago";
        if (delta.TotalMinutes > 1) return $"{(int)delta.TotalMinutes} minutes ago";
        return "Just now";
    }
}
