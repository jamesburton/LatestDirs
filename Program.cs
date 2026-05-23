using System.CommandLine;
using LatestDirs;

var rootArgument = new Argument<DirectoryInfo>(
    name: "directory",
    description: "The directory to scan",
    getDefaultValue: () => new DirectoryInfo(Directory.GetCurrentDirectory()));

var byGitOption = new Option<bool>(
    name: "--by-git",
    description: "Check latest commit dates and only return git repo folders");

var maxDepthOption = new Option<int>(
    name: "--max-depth",
    description: "Maximum depth to scan for changes",
    getDefaultValue: () => 10);

var topOption = new Option<int>(
    name: "--top",
    description: "Number of latest directories to list",
    getDefaultValue: () => 20);

var rootCommand = new RootCommand("LatestDirs - Find recently changed directories rapidly");
rootCommand.AddArgument(rootArgument);
rootCommand.AddOption(byGitOption);
rootCommand.AddOption(maxDepthOption);
rootCommand.AddOption(topOption);

rootCommand.SetHandler(async (dir, byGit, maxDepth, top) =>
{
    var results = await Scanner.ScanAsync(dir.FullName, maxDepth, byGit);
    UI.RenderTable(results, top);
}, rootArgument, byGitOption, maxDepthOption, topOption);

return await rootCommand.InvokeAsync(args);
