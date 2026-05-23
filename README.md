# LatestDirs

A high-performance .NET 10 CLI tool to rapidly identify recently changed directories. Designed for developers who need to quickly resume work in the most relevant project folders.

## Features

- **Blazing Fast**: Uses .NET 10's `FileSystemEnumerable` for low-allocation, high-throughput directory scanning.
- **Zero-Install**: Run instantly via `dnx` without global installation.
- **Git Aware**: Can identify git repositories and sort by latest commit date.
- **Smart Exclusions**: Automatically skips common noise folders like `node_modules`, `bin`, `obj`, and `.git`.
- **Beautiful Output**: Clean, ranked table view with relative time indicators.

## Prerequisites

- **.NET 10 SDK** (or later) is the only requirement. No other dependencies or installations are needed.

## Quick Start

The recommended way to run LatestDirs is via the .NET 10 `dnx` tool. This ensures you are always using the latest version with zero permanent install.

### Run with one shot (Zero-Install)
To scan the current directory and list the top 20 recently edited directories without being prompted for confirmation:

```bash
dnx -y LatestDirs
```

### Sorting by Git Commit
To find git repositories and sort them by the most recent commit:

```bash
dnx -y LatestDirs --by-git
```

## Usage & Options

```text
Usage:
  LatestDirs [<directory>] [options]

Arguments:
  <directory>    The directory to scan [default: Current Directory]

Options:
  --by-git       Check latest commit dates and only return git repo folders [default: False]
  --max-depth    Maximum depth to scan for changes [default: 10]
  --top          Number of latest directories to list [default: 20]
  --version      Show version information
  -?, -h, --help Show help and usage information
```

## Why LatestDirs?

LatestDirs is optimized for the "developer context switch." Unlike general file search tools, it focuses specifically on **directory-level recency**. 

- **vs. `ls -lt`**: Scans recursively to find the actual latest change *inside* subfolders, not just the folder's own metadata change.
- **vs. `find`**: Significantly faster on modern Windows/Linux/macOS filesystems by leveraging native .NET 10 performance primitives and parallel I/O.

## Installation (Optional)

If you prefer to have it available globally:

```bash
dotnet tool install -g LatestDirs
```

## Performance Note

LatestDirs is designed to rival the speed of native tools. It saturates disk I/O by parallelizing the scan of top-level directories and uses non-allocating metadata retrieval to minimize GC pressure during massive scans.
