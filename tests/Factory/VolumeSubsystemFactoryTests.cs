using Microsoft.Extensions.Logging;
using Xunit.Abstractions;
using XVolume.Factory;
using XVolume.Abstractions;
using XVolume.Platforms.Windows;
using XVolume.Platforms.MacOS;
using XVolume.Platforms.Linux;
using XVolume.Tests.Helpers;
using FluentAssertions;

namespace XVolume.Tests.Factory
{
    /// <summary>
    /// Unit tests for the VolumeSubsystemFactory class.
    /// </summary>
    public class VolumeSubsystemFactoryTests : IDisposable
    {
        private readonly ILogger _logger;
        private IVolumeSubsystem _volumeSubsystem;

        public VolumeSubsystemFactoryTests(ITestOutputHelper output)
        {
            _logger = TestHelpers.CreateTestLogger(output, nameof(VolumeSubsystemFactoryTests));
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        public void Create_ShouldReturnCorrectSubsystemForCurrentPlatform()
        {
            // Arrange & Act
            _volumeSubsystem = VolumeSubsystemFactory.Create(_logger);

            // Assert
            _volumeSubsystem.Should().NotBeNull();

            if (TestHelpers.IsRunningOnPlatform(TestPlatform.Windows))
            {
                _volumeSubsystem.Should().BeOfType<WindowsVolumeSubsystem>();
                _volumeSubsystem.Name.Should().Be("Windows Core Audio");
            }
            else if (TestHelpers.IsRunningOnPlatform(TestPlatform.MacOS))
            {
                _volumeSubsystem.Should().BeOfType<MacOSVolumeSubsystem>();
                _volumeSubsystem.Name.Should().Be("macOS CoreAudio");
            }
            else if (TestHelpers.IsRunningOnPlatform(TestPlatform.Linux))
            {
                _volumeSubsystem.Should().BeAssignableTo<IVolumeSubsystem>();
                var acceptableNames = new[] { "PipeWire", "PulseAudio", "ALSA" };
                acceptableNames.Should().Contain(_volumeSubsystem.Name);
            }
            else
            {
                Skip.If(true, "Unknown platform");
            }
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Create_WithNullLogger_ShouldUseConsoleLogger()
        {
            // Act
            Action act = () => _volumeSubsystem = VolumeSubsystemFactory.Create(null);

            // Assert
            act.Should().NotThrow();
            _volumeSubsystem.Should().NotBeNull();
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        [Trait("Category", "Windows")]
        public void Create_OnWindows_ShouldReturnWindowsSubsystem()
        {
            // Arrange
            Skip.IfNot(TestHelpers.IsRunningOnPlatform(TestPlatform.Windows));

            // Act
            _volumeSubsystem = VolumeSubsystemFactory.Create(_logger);

            // Assert
            _volumeSubsystem.Should().BeOfType<WindowsVolumeSubsystem>();
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        [Trait("Category", "MacOS")]
        public void Create_OnMacOS_ShouldReturnMacOSSubsystem()
        {
            // Arrange
            Skip.IfNot(TestHelpers.IsRunningOnPlatform(TestPlatform.MacOS));

            // Act
            _volumeSubsystem = VolumeSubsystemFactory.Create(_logger);

            // Assert
            _volumeSubsystem.Should().BeOfType<MacOSVolumeSubsystem>();
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        [Trait("Category", "Linux")]
        public void Create_OnLinux_ShouldReturnLinuxSubsystem()
        {
            // Arrange
            Skip.IfNot(TestHelpers.IsRunningOnPlatform(TestPlatform.Linux));

            // Act
            _volumeSubsystem = VolumeSubsystemFactory.Create(_logger);

            // Assert
            _volumeSubsystem.Should().BeAssignableTo<IVolumeSubsystem>();
            var volumeType = _volumeSubsystem.GetType();
            var isValidType = volumeType == typeof(PipeWireVolumeSubsystem) ||
                             volumeType == typeof(PulseAudioVolumeSubsystem) ||
                             volumeType == typeof(AlsaVolumeSubsystem);
            isValidType.Should().BeTrue($"Expected one of the Linux subsystems but got {volumeType.Name}");
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        public void Create_ShouldLogPlatformDetection()
        {
            // Arrange
            var mockLogger = new TestLogger();

            // Act
            _volumeSubsystem = VolumeSubsystemFactory.Create(mockLogger);

            // Assert
            mockLogger.LoggedMessages.Should().Contain(m =>
                m.Contains("detected") && m.Contains("volume subsystem"));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Create_MultipleTimes_ShouldReturnNewInstances()
        {
            // Act
            var subsystem1 = VolumeSubsystemFactory.Create(_logger);
            var subsystem2 = VolumeSubsystemFactory.Create(_logger);

            // Assert
            subsystem1.Should().NotBeNull();
            subsystem2.Should().NotBeNull();
            subsystem1.Should().NotBeSameAs(subsystem2);

            // Cleanup
            subsystem1.Dispose();
            subsystem2.Dispose();
        }

        public void Dispose()
        {
            _volumeSubsystem?.Dispose();
        }

        /// <summary>
        /// Test logger that captures log messages.
        /// </summary>
        private class TestLogger : ILogger
        {
            public List<string> LoggedMessages { get; } = new List<string>();

            public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                var message = formatter(state, exception);
                LoggedMessages.Add(message);
            }

            private class NullScope : IDisposable
            {
                public static NullScope Instance { get; } = new NullScope();
                public void Dispose() { }
            }
        }
    }
}