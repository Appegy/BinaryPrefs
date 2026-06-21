<!-- omit from toc -->
# 💾 Fast binary prefs for Unity3d

[![openupm](https://img.shields.io/npm/v/com.appegy.binary-prefs?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.appegy.binary-prefs/)

Almost every project needs to persist small pieces of data: player progress, settings, the last opened screen, feature flags. The default tool for this in Unity is `PlayerPrefs`, and for anything beyond a couple of values it gets painful fast.

Unity's standard `PlayerPrefs` has several limitations:

- It supports only three types: `int`, `float` and `string`. No `bool`, no `Vector3`, no `DateTime`, no enums, and no collections.
- It is not type-safe. Nothing stops you from writing a value with `SetInt` and reading it back with `GetString` - you just get a wrong value at runtime, with no error.
- It stores data in platform-specific locations (Windows registry, macOS plist, ...) as text, which is slow, bloated, and awkward to inspect or ship with the project.
- It has no way to remove keys by pattern, no change notifications, and no control over *when* data is written - saving is all-or-nothing.
- There is no way to group related keys, so everything lives in one flat global namespace.

All of these issues are addressed by BinaryPrefs: a configurable, strongly typed, binary key-value storage with support for Unity types, enums, collections, custom serializers, change events, and scoped sub-storages.

And the feature I like most: **every change is persisted the moment it happens**. As soon as you set a value - or even mutate a stored list, set or dictionary - it is written to disk, with atomic, corruption-safe writes. You never have to remember to call `Save()`, and an unexpected crash or quit can't lose the last change. When you need it, many changes can still be batched into a single write.

<!-- omit from toc -->
## Table of content

- [Package installation](#package-installation)
- [Quick start](#quick-start)
- [Configuring storage](#configuring-storage)
- [Reading and writing](#reading-and-writing)
- [Collections](#collections)
- [Batch changes](#batch-changes)
- [Saving](#saving)
- [Change events](#change-events)
- [Nested storage](#nested-storage)
- [Behavior reference](#behavior-reference)
- [License](#license)

## Package installation

<!-- omit from toc -->
### Using OpenUPM

Using [OpenUPM-CLI](https://openupm.com/docs/getting-started.html) run the command

```
openupm add com.appegy.binary-prefs
```

Alternatively, you can install the package manually by following the instructions on the package [page](https://openupm.com/packages/com.appegy.binary-prefs/).

<!-- omit from toc -->
### Using Git link

Add the package to your `manifest.json`.

```json
"dependencies": {
  "com.appegy.binary-prefs": "https://github.com/appegy/binaryprefs.git?path=/src",
  ...
},
```

## Quick start

The simplest way to get a storage with all primitive types and auto-save enabled:

```csharp
using System.IO;
using Appegy.Storage;
using UnityEngine;

var path = Path.Combine(Application.persistentDataPath, "player.bin");

// Pre-configured: primitive types + auto-save on change.
using var storage = BinaryStorage.Get(path);

storage.Set("player_score", 100);
storage.Set("player_speed", 5.5f);
storage.Set("player_name", "John Doe");

int score = storage.Get("player_score", 0);
float speed = storage.Get("player_speed", 1.0f);
string name = storage.Get("player_name", "Unknown");
```

> `BinaryStorage` implements `IDisposable`. Dispose it (e.g. with `using`) to flush and release the file. In the Editor the file path is locked while a storage instance is open, preventing accidental concurrent access to the same file.

## Configuring storage

For full control use the fluent builder via `BinaryStorage.Construct`:

```csharp
using var storage = BinaryStorage.Construct(path)
    .AddPrimitiveTypes()                                   // built-in C# and Unity types
    .SupportEnum<GameState>()                              // an enum
    .SupportListsOf<int>()                                 // list of a supported type
    .SupportSetsOf<string>()                               // set of a supported type
    .SupportDictionariesOf<string, int>()                 // dictionary of supported types
    .SetMissingKeyBehaviour(MissingKeyBehavior.ReturnDefaultValueOnly)
    .SetTypeMismatchBehaviour(TypeMismatchBehaviour.OverrideValueAndType)
    .EnableAutoSaveOnChange()
    .Build(KeyLoadFailedBehaviour.IgnoreWithWarning);
```

`AddPrimitiveTypes` registers all C# primitives plus common Unity types: `bool`, `char`, all integer types, `float`, `double`, `decimal`, `string`, `DateTime`, `TimeSpan`, `Quaternion`, `Vector2/3/4`, `Vector2Int` and `Vector3Int`.

Notes:
- `SupportListsOf<T>` / `SupportSetsOf<T>` / `SupportDictionariesOf<TKey, TValue>` require the element types to be registered first (e.g. via `AddPrimitiveTypes` or `AddTypeSerializer`); otherwise a `CantSupportCollectionOfException` is thrown.
- `AddTypeSerializer` throws if a serializer for that type, or with the same type name, is already registered.

To persist your own type, register a serializer for it:

```csharp
.AddTypeSerializer(myCustomSerializer)   // a TypeSerializer<MyType>
```

## Reading and writing

```csharp
storage.Set("level", 7);                      // returns bool: true if the value was written
bool exists = storage.Has("level");           // key present?
System.Type type = storage.TypeOf("level");   // stored type, or null if absent
int level = storage.Get("level", 1);          // typed read with default

storage.Remove("level");                       // remove one key
storage.Remove(key => key.StartsWith("tmp_")); // remove by predicate, returns count
storage.RemoveAll();                           // clear everything
```

Untyped access is available via `GetRaw` / `SetRaw` when the type is only known at runtime:

```csharp
object raw = storage.GetRaw("level");
storage.SetRaw("level", 10);
```

## Collections

Collections returned by the storage are live: mutating them updates the storage (and triggers auto-save if enabled).

```csharp
IList<int> scores = storage.GetListOf<int>("scores");
scores.Add(100);
scores.Add(250);

ISet<string> tags = storage.GetSetOf<string>("tags");
tags.Add("vip");

IDictionary<string, int> inventory = storage.GetDictionaryOf<string, int>("inventory");
inventory["coins"] = 50;
```

Read-only views are also available: `GetReadOnlyListOf<T>`, `GetReadOnlySetOf<T>`, `GetReadOnlyDictionaryOf<TKey, TValue>`.

## Batch changes

When changing many keys at once, wrap them in a scope so the storage saves only once at the end instead of on every change:

```csharp
using (storage.MultipleChangeScope())
{
    storage.Set("a", 1);
    storage.Set("b", 2);
    storage.Set("c", 3);
} // saved here once (when auto-save is enabled)
```

## Saving

By default (and with `BinaryStorage.Get`) auto-save is enabled, so **every change is written to disk immediately** - the moment you `Set` a value or mutate a stored collection. There is no "dirty" window where data lives only in memory: forget-to-save bugs and data lost to a crash simply don't happen.

Writes are atomic: data is written to a temporary file first and only then swapped in, so an interrupted save can never corrupt your existing file.

Need to apply many changes as a single write? Wrap them in a [change scope](#batch-changes) - auto-save then fires once, when the scope ends.

If you prefer full manual control, skip `EnableAutoSaveOnChange` and persist yourself:

```csharp
storage.Save();
```

## Change events

```csharp
storage.OnKeyAdded   += key => Debug.Log($"Added: {key}");
storage.OnKeyChanged += key => Debug.Log($"Changed: {key}");
storage.OnKeyRemoved += key => Debug.Log($"Removed: {key}");
```

## Nested storage

`CreateChild` returns an `IBinaryStorage` view scoped under a prefix, useful for grouping keys (e.g. per player or per feature) while sharing one file:

```csharp
IBinaryStorage player1 = storage.CreateChild("player1");
player1.Set("score", 100); // stored under the "player1" prefix in the same file
```

## Behavior reference

<!-- omit from toc -->
### MissingKeyBehavior
Controls what `Get<T>` does when the key is absent.
- `InitializeWithDefaultValue` - store the provided default and return it.
- `ReturnDefaultValueOnly` - return the default without storing it.

<!-- omit from toc -->
### TypeMismatchBehaviour
Controls what happens when a key already exists with a different type.
- `ThrowException` - throw on mismatch.
- `OverrideValueAndType` - replace the stored value and its type.
- `Ignore` - keep the existing value, ignore the new one.

<!-- omit from toc -->
### KeyLoadFailedBehaviour
Passed to `Build`; controls what happens when a key fails to deserialize on load.
- `ThrowException` - abort loading with an exception.
- `Ignore` - skip the bad key silently.
- `IgnoreWithWarning` - skip the bad key and log a warning (default).

Per-call overrides are available too: `Get<T>(key, default, overrideMissingKeyBehavior)` and `Set<T>(key, value, overrideTypeMismatchBehaviour)`.

## License

MIT. See [LICENSE](LICENSE).
