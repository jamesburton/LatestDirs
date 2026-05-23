# LatestDirs: Performance Optimization & Benchmarking Plan

Goal: Achieve and exceed the scanning performance of tools like `ripgrep` and `Agent Ransack` for recursive directory recency checks.

## Phase 1: Research (Current)

### 1.1 IO Strategy
- **IO Ring (.NET 10)**: Investigate using the new `System.IO.Pipelines` or direct `io_uring` wrappers on Linux for high-concurrency, zero-copy I/O.
- **Direct Syscalls**: For Windows, explore bypassing `FileSystemEnumerable` for direct `GetQueuedCompletionStatus` or `ReadDirectoryChangesW` if managed overhead remains a bottleneck.

### 1.2 Processing & Memory
- **SIMD Metadata Parsing**: Use Vectorized instructions (`Vector<T>`) to compare timestamps across blocks of files.
- **Lock-Free Aggregation**: Replace `lock(results)` with `ConcurrentStack` or `System.Threading.Channels` to eliminate lock contention during high-speed parallel scans.
- **Zero-String Scanning**: Attempt to scan and compare timestamps using `ReadOnlySpan<byte>` or `ReadOnlySpan<char>` directly from the buffer, avoiding `string` allocations for every filename.

## Phase 2: Benchmarking Suite

### 2.1 Baseline Comparison
Tools to benchmark against:
- `LatestDirs` (Current)
- `ripgrep` (`rg --files --sort mtime`)
- `find` / `ls -lt` (Linux)
- `Get-ChildItem` (PowerShell)

### 2.2 Workload Scenarios
- **Shallow & Wide**: Root directory with 10,000 subdirectories.
- **Deep & Narrow**: Single path with 100 levels of depth.
- **Real World**: `C:\` or `/home/user` with mixed content (source code, media, system files).
- **Network Share**: High latency, high throughput scenario.

## Phase 3: Optimizations

### 3.1 Parallelism Tuning
- **Work-Stealing Scheduler**: Custom TaskScheduler to optimize for I/O-bound workloads.
- **Adaptive Concurrency**: Dynamically adjust the number of parallel workers based on disk type (SSD vs. HDD) and I/O wait times.

### 3.2 Pre-Filtering
- **Metadata Cache**: Optional local cache for extremely large drives, using filesystem journals (USN Journal on NTFS) to only scan changed blocks.

---

## Verification Plan
- Use `BenchmarkDotNet` for micro-benchmarks of `GetLatestChange`.
- Use `hyperfine` for end-to-end CLI performance comparison against `ripgrep`.
