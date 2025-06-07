# Contributing to XVolume

Thank you for your interest in contributing to XVolume, a cross-platform .NET library for controlling system volume on Windows, macOS, and Linux (ALSA/PulseAudio/PipeWire). We welcome contributions from the community to improve functionality, fix bugs, and enhance documentation. This guide outlines how to contribute effectively.

## Code of Conduct
By participating, you agree to abide by our [Code of Conduct](CODE_OF_CONDUCT.md). We are committed to fostering an inclusive and respectful community.

## How to Contribute

### Reporting Issues
- Use the [GitHub Issues](https://github.com/ByronAP/XVolume/issues) page to report bugs, suggest features, or ask questions.
- Before creating a new issue, check if it already exists.
- Provide a clear title and description, including:
  - **Environment**: OS (e.g., Windows 11, macOS Ventura, Ubuntu 24.04.2), .NET version (e.g., .NET 8.0).
  - **Audio System**: For Linux, specify ALSA, PulseAudio, or PipeWire.
  - **Steps to Reproduce**: Detailed steps for bugs.
  - **Expected vs. Actual Behavior**: What you expected and what happened.
  - **Logs or Screenshots**: If applicable, include error logs or screenshots.

### Submitting Pull Requests
1. **Fork the Repository**:
   - Click the "Fork" button on the [XVolume repository](https://github.com/ByronAP/XVolume).
   - Clone your fork: `git clone https://github.com/YOUR_USERNAME/XVolume.git`

2. **Create a Branch**:
   - Create a descriptive branch name: `git checkout -b feature/your-feature` or `git checkout -b fix/your-bug-fix`.
   - Example: `feature/add-volume-profiles` or `fix/windows-com-leak`.

3. **Make Changes**:
   - Follow the [Coding Guidelines](#coding-guidelines) below.
   - Keep changes focused; one pull request per feature or fix.
   - Update tests and documentation if applicable.

4. **Test Your Changes**:
   - Build the library: `dotnet build`
   - Run tests: `dotnet test`
   - Test on relevant platforms (Windows, macOS, Linux).
   - Ensure no regressions in existing functionality.

5. **Commit Your Changes**:
   - Use clear, concise commit messages: `git commit -m "Add PipeWire volume control support"`.
   - Sign off your commits: `git commit -s -m "Your message"` (required for [DCO](#developer-certificate-of-origin)).

6. **Push and Create a Pull Request**:
   - Push to your fork: `git push origin feature/your-feature`.
   - Open a pull request against the `main` branch of the XVolume repository.
   - Provide a clear title and description, referencing any related issues (e.g., `Fixes #123`).
   - Check the PR template (if available) for additional requirements.

7. **Review Process**:
   - Maintainers will review your PR, provide feedback, and request changes if needed.
   - Respond to feedback promptly and make necessary updates.
   - Once approved, your PR will be merged.

### Coding Guidelines
- **Target Framework**: .NET Standard 2.0 for maximum compatibility.
- **Language Version**: C# 7.3 (compatible with .NET Standard 2.0).
- **Style**:
  - Follow [C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions).
  - Use 4-space indentation, PascalCase for public members, and camelCase for private fields.
  - Include XML documentation for all public types and members.
  - Keep line length reasonable (around 120 characters).
- **Dependencies**: Only `Microsoft.Extensions.Logging.Abstractions` (minimal version for .NET Standard 2.0).
- **Platform Code**: 
  - Place platform-specific code in appropriate namespace (e.g., `XVolume.Platforms.Windows`).
  - Use `RuntimeInformation` for platform detection.
  - Handle platform-specific exceptions gracefully.
- **Error Handling**: 
  - Use robust exception handling with meaningful error messages.
  - Log errors, warnings, and debug information via `ILogger`.
  - Throw appropriate exceptions (`ArgumentException`, `InvalidOperationException`, etc.).
- **Performance**: 
  - Optimize for resource-constrained systems (e.g., Raspberry Pi).
  - Avoid unnecessary allocations and string operations.
  - Use compiled regex patterns where appropriate.
- **Thread Safety**: 
  - Ensure all public methods are thread-safe.
  - Use appropriate synchronization primitives (`SemaphoreSlim`, etc.).

### Project Structure
```
XVolume/
├── src/
│   ├── Abstractions/       # Interfaces
│   ├── Factory/            # Factory pattern implementation
│   ├── Common/             # Base classes and utilities
│   ├── Platforms/          # Platform-specific implementations
│   │   ├── Windows/
│   │   ├── Linux/
│   │   └── MacOS/
│   ├── Utilities/          # Shared utilities (CommandExecutor, etc.)
│   └── XVolume.csproj      # Project file
├── tests/                  # Unit tests     
└── samples/                # Sample applications       
```

### Developer Certificate of Origin (DCO)
By contributing, you agree to the [Developer Certificate of Origin](https://developercertificate.org/). Sign off your commits with `git commit -s` to certify that you have the right to submit your contribution.

### Areas for Contribution
- **Bug Fixes**: Address issues in volume control, COM interop, or shell commands.
- **New Features**: 
  - Volume profiles (save/restore volume settings)
  - Per-application volume control
  - Audio device enumeration and switching
  - Volume change notifications/events
- **Platform Enhancements**: 
  - Improve PipeWire native support (without PulseAudio compatibility)
  - Add support for other audio systems (JACK, OSS)
  - Optimize shell command execution
- **Documentation**: 
  - Enhance README.md with more examples
  - Add XML documentation for internal classes
  - Create wiki pages for advanced usage
- **Testing**: 
  - Add comprehensive unit tests
  - Create integration tests for each platform
  - Add performance benchmarks
- **CI/CD**: 
  - Set up GitHub Actions for multi-platform builds
  - Add automated testing on different OS versions
  - Configure automatic NuGet package publishing

### Setting Up the Development Environment
1. **Install .NET SDK**: 
   - .NET 6.0+ SDK (includes .NET Standard 2.0 support)
   - Download from [dotnet.microsoft.com](https://dotnet.microsoft.com/download)

2. **Platform-Specific Setup**:
   - **Windows**: No additional dependencies (uses Core Audio API).
   - **macOS**: Ensure `osascript` is available (included in macOS).
   - **Linux**:
     - ALSA: `sudo apt install alsa-utils`
     - PulseAudio: `sudo apt install pulseaudio pulseaudio-utils`
     - PipeWire: `sudo apt install pipewire pipewire-pulse`

3. **Clone and Build**:
   ```bash
   git clone https://github.com/ByronAP/XVolume.git
   cd XVolume
   dotnet build
   ```

4. **Run Tests**:
   ```bash
   dotnet test
   ```

5. **Create Sample**:
   ```bash
   cd samples/XVolume.Sample
   dotnet run
   ```

### Testing Guidelines
- Test on multiple platforms if possible.
- For Linux, test with different audio systems (ALSA, PulseAudio, PipeWire).
- Verify volume changes are reflected in system settings.
- Test edge cases (0%, 100%, muted state).
- Check for memory leaks and proper resource disposal.

### Questions?
If you have questions or need help, open an issue or reach out via the [Discussions](https://github.com/ByronAP/XVolume/discussions) tab.

Thank you for contributing to XVolume!