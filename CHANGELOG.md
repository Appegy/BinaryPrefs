# Changelog

All notable changes to this package are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/)
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0/).

## [Unreleased]

## [1.2.0] - 2026-10-06
Storage files are now written on a background thread: a change reaches the disk a moment after `Set` returns, and a process killed in that moment loses only the last change. Pass `SaveOnBackgroundThread(false)` to the builder to write the file before every change returns, as before.

### Added
- `SaveOnBackgroundThread(bool)` on the builder.
- `SetCorruptedFileBehaviour(CorruptedFileBehaviour)` on the builder: `ThrowException`, `ResetToEmpty` or `ResetToEmptyWithError` (default).

### Changed
- Storage files are written on a shared background thread by default. `Save()` and `Dispose()` still block until the data is on disk.
- The file is flushed to the disk (fsync) before it replaces the old one.
- The file is published with `File.Replace`, and the previous save stays next to it in a `.bak` file.
- A record that runs past the end of the file now makes the file corrupted. Before, the record was skipped.
- Every read error in the file now counts as a corrupted file. An error on opening the file still reaches the caller.
- By default `Build()` no longer throws when neither the file nor `.bak` can be read: the storage starts empty and logs the exception.
- Saving and loading are faster.

### Removed
- `IsDirty`. Use `Save()` when you need the data on disk.
- The `BinaryStorage` finalizer. A storage that is never disposed keeps its editor path lock until the domain reloads.

### Fixed
- A file that cannot be read no longer fails every load: the storage loads `.bak` instead.
- A save stopped inside `File.Replace` on Windows no longer loses the data: the next load reads the new file, or `.bak` if the new file cannot be read.
- A failed write no longer loses the change: the next change, `Save()` or `Dispose()` writes the data again.

## [1.0.5]
- Baseline of the changelog. See the GitHub Releases page for earlier history.
