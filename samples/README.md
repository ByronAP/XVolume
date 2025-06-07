# XVolume Sample Application

A comprehensive command-line application demonstrating the features of the XVolume cross-platform volume control library.

## Overview

This sample application provides a command-line interface to test and demonstrate all features of the XVolume library, including:
- Getting and setting system volume
- Mute/unmute operations
- Smooth volume transitions
- Real-time volume monitoring
- Platform-specific audio system information

## Prerequisites

- .NET 9.0 or later
- XVolume library
- Platform-specific requirements:
  - **Windows**: No additional requirements
  - **macOS**: No additional requirements
  - **Linux**: ALSA, PulseAudio, or PipeWire

## Building

From the samples directory:
```bash
dotnet build
```

## Running

```bash
dotnet run
```

Or after building:
```bash
./bin/Debug/net9.0/XVolume.Sample
```

## Commands

### info
Display system and audio information.
```bash
XVolume.Sample info
```
Aliases: `i`, `status`

### volume
Get or set system volume.
```bash
# Get current volume
XVolume.Sample volume

# Set volume to 50%
XVolume.Sample volume 50

# Set volume with smooth transition
XVolume.Sample volume 75 --smooth

# Increase volume by 10%
XVolume.Sample volume +10

# Decrease volume by 5%
XVolume.Sample volume -5
```
Aliases: `vol`, `v`

### mute
Control audio mute state.
```bash
# Toggle mute
XVolume.Sample mute

# Mute audio
XVolume.Sample mute on

# Unmute audio
XVolume.Sample mute off
```
Aliases: `m`

### test
Run comprehensive volume control tests.
```bash
# Run all tests
XVolume.Sample test

# Run quick tests only
XVolume.Sample test --quick
```
Aliases: `t`, `check`

### monitor
Monitor volume changes in real-time.
```bash
# Monitor with default 500ms interval
XVolume.Sample monitor

# Monitor with custom interval
XVolume.Sample monitor --interval 100
```
Aliases: `mon`, `watch`

Controls while monitoring:
- `Q` - Quit monitoring
- `C` - Clear screen
- `Ctrl+C` - Stop monitoring

### help
Display help information.
```bash
# Show all commands
XVolume.Sample help

# Show help for specific command
XVolume.Sample help volume
```

## Examples

### Basic Usage
```bash
# Check current volume
XVolume.Sample volume

# Set volume to 50%
XVolume.Sample volume 50

# Mute audio
XVolume.Sample mute on

# Get system info
XVolume.Sample info
```

### Advanced Usage
```bash
# Smooth volume transition
XVolume.Sample volume 80 --smooth

# Monitor volume changes
XVolume.Sample monitor --interval 250

# Run full test suite
XVolume.Sample test
```

### Scripting
```bash
# Increase volume by 10%
XVolume.Sample volume +10

# Check if muted (check exit code)
XVolume.Sample mute status
if [ $? -eq 0 ]; then
    echo "Audio is muted"
fi
```

## Environment Variables

- `XVOLUME_DEBUG=1` - Enable debug output with stack traces

## Exit Codes

- `0` - Success
- `1` - Error occurred

## Platform Notes

### Windows
- Uses Windows Core Audio API
- Requires Windows Vista or later
- COM initialization is handled automatically

### macOS
- Uses CoreAudio via osascript
- Requires macOS 10.15 or later
- May require accessibility permissions for some operations

### Linux
- Automatically detects available audio system
- Supports ALSA, PulseAudio, and PipeWire
- Falls back gracefully if audio system is not available

## Troubleshooting

### No audio system detected
- **Linux**: Install ALSA utilities (`sudo apt install alsa-utils`) or PulseAudio (`sudo apt install pulseaudio`)
- **All platforms**: Run `XVolume.Sample info` to see detected audio system

### Permission errors
- **macOS**: Grant terminal accessibility permissions in System Preferences
- **Linux**: Add user to `audio` group: `sudo usermod -a -G audio $USER`

### Debug mode
Enable debug output:
```bash
export XVOLUME_DEBUG=1
XVolume.Sample volume 50
```

## Contributing

This sample application serves as a reference implementation. Feel free to:
- Add new commands
- Enhance existing functionality
- Improve error handling
- Add platform-specific features

## License

Same as XVolume library - MIT License