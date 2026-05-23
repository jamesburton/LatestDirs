## Current Status (v1.0.4+)

### Benchmarks (100k Files)
- **Normal Scan**: ~49ms (approx. 2M files/sec)
- **Git Scan**: ~17ms
- **Startup (Native AOT)**: ~800ms (end-to-end for --help)

### Optimizations Applied
- **Lock-Free Channels**: `System.Threading.Channels` for result aggregation.
- **SIMD-Ready Loops**: Buffer-based `Span<long>` processing for JIT auto-vectorization.
- **Native AOT**: Compiled to standalone native binary for Windows x64.

---

## Phase 4: "Ultra" Mode (Windows Only)

To exceed ripgrep's metadata performance on Windows, we are implementing "Ultra" mode using the **NTFS USN Journal**.

### USN Journal Strategy
1. **Direct Access**: Use `DeviceIoControl` to read the journal directly from the disk volume.
2. **Instant Change Detection**: Instead of walking the directory tree, we query the journal for all changes since a specific timestamp.
3. **MFT Caching**: 
   - USN records only provide Parent IDs.
   - We will pre-load the **Master File Table (MFT)** using `FSCTL_ENUM_USN_DATA` into a high-performance memory map (e.g., `FrozenDictionary<ulong, ulong>`).
   - This allows resolving the full path for any changed file in O(depth) time using in-memory lookups.

### Target Performance
- **Volume Scan**: < 100ms for entire `C:\` drive (regardless of file count).
- **Recency Filter**: Constant time O(changes) rather than O(total files).

## Phase 5: I/O Ring (Linux)
- Researching `io_uring` integration via `System.IO.Pipelines` or direct P/Invoke for high-concurrency metadata retrieval on Linux.
