# Changelog

All notable changes to this package are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/)
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0/).

## [Unreleased]
### Changed
- Storage files are now written on a shared background thread. Changes are still serialized on the calling thread, but `Set` and auto-save no longer wait for the disk. `Save()` and `Dispose()` still block until the data has reached the disk.

### Added
- `SaveOnBackgroundThread(bool)` on the builder. Pass `false` to write the file before every change returns, as before.

### Removed
- `IsDirty`. With auto-save on it was almost always `false`, and with the background writer it meant "no change is waiting to be handed over", not "everything is on disk". Use `Save()` when you need the data on disk.

### Fixed
- Two storages opened through paths that differ only in form (`a/save.dat` and `./a/save.dat`) no longer publish the same file without serializing against each other.
- A storage larger than 1 MB no longer re-grows its serialization buffer from 1 MB on every save.

## [1.0.5]
- Baseline of the changelog. See the GitHub Releases page for earlier history.
