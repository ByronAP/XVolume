# XVolume

A cross-platform .NET library for controlling system volume on Windows, macOS, and Linux. Built on .NET Standard 2.0 for maximum compatibility.

## Features
- Get and set system volume (0-100%)
- Mute, unmute, and toggle mute state
- Increment and decrement volume by percentage
- Smooth volume transitions with customizable duration
- Query current audio device name
- Support for Windows Core Audio, macOS CoreAudio, ALSA, PulseAudio, and PipeWire
- Lightweight with minimal dependencies
- Thread-safe operations

## Installation
```bash
dotnet add package XVolume
```

## Usage
```csharp
using XVolume;
using Microsoft.Extensions.Logging;

// Create with optional logger
using var volume = VolumeSubsystemFactory.Create();

// Basic operations
Console.WriteLine($"Audio System: {volume.Name}");
Console.WriteLine($"Current Device: {volume.CurrentDevice ?? "Unknown"}");
Console.WriteLine($"Current Volume: {volume.Volume}%");
Console.WriteLine($"Muted: {volume.IsMuted}");

// Set volume
volume.Volume = 50;

// Mute operations
volume.Mute();
volume.Unmute();
volume.ToggleMute();

// Adjust volume
volume.IncrementVolume(10); // +10%
volume.DecrementVolume(5);  // -5%

// Smooth transitions
await volume.SetVolumeSmooth(75, durationMs: 1000);
```

## Platform Support
- **.NET Standard 2.0** compatible
- Works with .NET Framework 4.6.1+, .NET Core 2.0+, and modern .NET versions

### Supported Audio Systems
- **Windows**: Core Audio API (Vista and later)
- **macOS**: CoreAudio via osascript (macOS 10.15+)
- **Linux**: 
  - ALSA (Advanced Linux Sound Architecture)
  - PulseAudio
  - PipeWire (with PulseAudio compatibility layer)

## Prerequisites
### Windows/macOS
No additional dependencies required.

### Linux
Depending on your audio system:
- **ALSA**: `sudo apt install alsa-utils`
- **PulseAudio**: `sudo apt install pulseaudio pulseaudio-utils`
- **PipeWire**: `sudo apt install pipewire pipewire-pulse`

The library automatically detects and uses the available audio system.

## Error Handling
```csharp
using Microsoft.Extensions.Logging;

// Use a logger for detailed diagnostics
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<Program>();

try 
{
    using var volume = VolumeSubsystemFactory.Create(logger);
    volume.Volume = 50;
}
catch (PlatformNotSupportedException ex)
{
    Console.WriteLine($"Platform not supported: {ex.Message}");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Audio system not available: {ex.Message}");
}
```

## License
MIT License - see [LICENSE](LICENSE) file for details.

## Contributing
Contributions are welcome! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

## Links
- [NuGet Package](https://www.nuget.org/packages/XVolume)
- [GitHub Repository](https://github.com/ByronAP/XVolume)
- [Documentation](https://github.com/ByronAP/XVolume/wiki)
- [Issue Tracker](https://github.com/ByronAP/XVolume/issues)