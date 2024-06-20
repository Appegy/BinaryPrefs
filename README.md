# BinaryPrefs

BinaryPrefs is a flexible and efficient binary-based key-value storage system for Unity. It aims to provide a better alternative to Unity's built-in `PlayerPrefs` by offering faster access times and greater configurability.

## Features

- **Flexible Configuration**: Create multiple `BinaryStorage` instances with different configurations.
- **Primitive Type Support**: Built-in serializers for primitive types and common Unity types such as `Vector3`, `Quaternion`, etc.
- **Enum and Collection Support**: Add support for enums and collections such as lists, sets, and dictionaries.
- **Automatic Save**: Option to automatically save changes to disk.
- **Fallback to PlayerPrefs**: Automatically read from `PlayerPrefs` if the key is not found in the binary storage.

## Installation

### Using Unity Package Manager (UPM)

1. Open your Unity project.
2. Go to `Window > Package Manager`.
3. Click the `+` button and select `Add package from git URL...`.
4. Enter the URL of the repository: `https://github.com/Appegy/BinaryPrefs.git`.

## Usage

### Basic Usage

#### Setting and Getting Values

```csharp
using Appegy.Storage;

// Set values
BinaryPrefs.SetInt("player_score", 100);
BinaryPrefs.SetFloat("player_speed", 5.5f);
BinaryPrefs.SetString("player_name", "John Doe");

// Get values
int score = BinaryPrefs.GetInt("player_score", 0);
float speed = BinaryPrefs.GetFloat("player_speed", 1.0f);
string playerName = BinaryPrefs.GetString("player_name", "Unknown");
