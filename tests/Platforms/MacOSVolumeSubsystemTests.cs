using System;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;
using XVolume.Platforms.MacOS;
using XVolume.Tests.Helpers;
using FluentAssertions;

namespace XVolume.Tests.Platforms
{
    /// <summary>
    /// Unit tests for the MacOSVolumeSubsystem class.
    /// </summary>
    public class MacOSVolumeSubsystemTests : IDisposable
    {
        private readonly ILogger _logger;
        private MacOSVolumeSubsystem _volumeSubsystem;

        public MacOSVolumeSubsystemTests(ITestOutputHelper output)
        {
            _logger = TestHelpers.CreateTestLogger(output, nameof(MacOSVolumeSubsystemTests));
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        [Trait("Category", "MacOS")]
        public void Constructor_ShouldInitializeSuccessfully()
        {
            // Arrange
            Skip.IfNot(TestHelpers.IsRunningOnPlatform(TestPlatform.MacOS));

            // Act
            Action act = () => _volumeSubsystem = new MacOSVolumeSubsystem(_logger);

            // Assert
            act.Should().NotThrow();
            _volumeSubsystem.Should().NotBeNull();
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        [Trait("Category", "MacOS")]
        public void Name_ShouldReturnMacOSCoreAudio()
        {
            // Arrange
            Skip.IfNot(TestHelpers.IsRunningOnPlatform(TestPlatform.MacOS));
            _volumeSubsystem = new MacOSVolumeSubsystem(_logger);

            // Act
            var name = _volumeSubsystem.Name;

            // Assert
            name.Should().Be("macOS CoreAudio");
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        [Trait("Category", "MacOS")]
        public void Volume_Get_ShouldReturnValueBetween0And100()
        {
            // Arrange
            Skip.IfNot(TestHelpers.IsRunningOnPlatform(TestPlatform.MacOS));
            _volumeSubsystem = new MacOSVolumeSubsystem(_logger);

            // Act
            var volume = _volumeSubsystem.Volume;

            // Assert
            volume.Should().BeInRange(0, 100);
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        [Trait("Category", "MacOS")]
        public void Volume_Set_ShouldUpdateVolume()
        {
            // Arrange
            Skip.IfNot(TestHelpers.IsRunningOnPlatform(TestPlatform.MacOS));
            _volumeSubsystem = new MacOSVolumeSubsystem(_logger);
            var originalVolume = _volumeSubsystem.Volume;
            var testVolume = originalVolume == 50 ? 60 : 50;

            try
            {
                // Act
                _volumeSubsystem.Volume = testVolume;

                // Assert
                _volumeSubsystem.Volume.Should().Be(testVolume);
            }
            finally
            {
                // Restore original volume
                _volumeSubsystem.Volume = originalVolume;
            }
        }

        [SkippableTheory]
        [Trait("Category", "Integration")]
        [Trait("Category", "MacOS")]
        [InlineData(-1)]
        [InlineData(101)]
        public void Volume_Set_WithInvalidValue_ShouldThrow(int invalidVolume)
        {
            // Arrange
            Skip.IfNot(TestHelpers.IsRunningOnPlatform(TestPlatform.MacOS));
            _volumeSubsystem = new MacOSVolumeSubsystem(_logger);

            // Act
            Action act = () => _volumeSubsystem.Volume = invalidVolume;

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>()
                .WithParameterName("value");
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        [Trait("Category", "MacOS")]
        public void IsMuted_ShouldReturnBooleanValue()
        {
            // Arrange
            Skip.IfNot(TestHelpers.IsRunningOnPlatform(TestPlatform.MacOS));
            _volumeSubsystem = new MacOSVolumeSubsystem(_logger);

            // Act
            var isMuted = _volumeSubsystem.IsMuted;

            // Assert
            // Just verify it returns a boolean without throwing
            (isMuted || !isMuted).Should().BeTrue();
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        [Trait("Category", "MacOS")]
        public void Mute_Unmute_ShouldToggleMuteState()
        {
            // Arrange
            Skip.IfNot(TestHelpers.IsRunningOnPlatform(TestPlatform.MacOS));
            _volumeSubsystem = new MacOSVolumeSubsystem(_logger);
            var originalMuteState = _volumeSubsystem.IsMuted;

            try
            {
                // Act & Assert - Mute
                _volumeSubsystem.Mute();
                _volumeSubsystem.IsMuted.Should().BeTrue();

                // Act & Assert - Unmute
                _volumeSubsystem.Unmute();
                _volumeSubsystem.IsMuted.Should().BeFalse();
            }
            finally
            {
                // Restore original state
                if (originalMuteState)
                    _volumeSubsystem.Mute();
                else
                    _volumeSubsystem.Unmute();
            }
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        [Trait("Category", "MacOS")]
        public void ToggleMute_ShouldToggleState()
        {
            // Arrange
            Skip.IfNot(TestHelpers.IsRunningOnPlatform(TestPlatform.MacOS));
            _volumeSubsystem = new MacOSVolumeSubsystem(_logger);
            var originalMuteState = _volumeSubsystem.IsMuted;

            try
            {
                // Act & Assert
                _volumeSubsystem.ToggleMute();
                _volumeSubsystem.IsMuted.Should().Be(!originalMuteState);

                _volumeSubsystem.ToggleMute();
                _volumeSubsystem.IsMuted.Should().Be(originalMuteState);
            }
            finally
            {
                // Ensure we're back to original state
                if (_volumeSubsystem.IsMuted != originalMuteState)
                {
                    _volumeSubsystem.ToggleMute();
                }
            }
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        [Trait("Category", "MacOS")]
        public void IncrementVolume_ShouldIncreaseVolume()
        {
            // Arrange
            Skip.IfNot(TestHelpers.IsRunningOnPlatform(TestPlatform.MacOS));
            _volumeSubsystem = new MacOSVolumeSubsystem(_logger);
            var originalVolume = _volumeSubsystem.Volume;

            // Ensure we have room to increment
            if (originalVolume > 90)
            {
                _volumeSubsystem.Volume = 50;
            }

            try
            {
                // Act
                var volumeBefore = _volumeSubsystem.Volume;
                _volumeSubsystem.IncrementVolume(10);

                // Assert
                _volumeSubsystem.Volume.Should().Be(Math.Min(100, volumeBefore + 10));
            }
            finally
            {
                // Restore original volume
                _volumeSubsystem.Volume = originalVolume;
            }
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        [Trait("Category", "MacOS")]
        public void DecrementVolume_ShouldDecreaseVolume()
        {
            // Arrange
            Skip.IfNot(TestHelpers.IsRunningOnPlatform(TestPlatform.MacOS));
            _volumeSubsystem = new MacOSVolumeSubsystem(_logger);
            var originalVolume = _volumeSubsystem.Volume;

            // Ensure we have room to decrement
            if (originalVolume < 10)
            {
                _volumeSubsystem.Volume = 50;
            }

            try
            {
                // Act
                var volumeBefore = _volumeSubsystem.Volume;
                _volumeSubsystem.DecrementVolume(10);

                // Assert
                _volumeSubsystem.Volume.Should().Be(Math.Max(0, volumeBefore - 10));
            }
            finally
            {
                // Restore original volume
                _volumeSubsystem.Volume = originalVolume;
            }
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        [Trait("Category", "MacOS")]
        public void CurrentDevice_ShouldReturnStringOrNull()
        {
            // Arrange
            Skip.IfNot(TestHelpers.IsRunningOnPlatform(TestPlatform.MacOS));
            _volumeSubsystem = new MacOSVolumeSubsystem(_logger);

            // Act
            var device = _volumeSubsystem.CurrentDevice;

            // Assert
            // Device can be null if command fails or no device info available
            if (device != null)
            {
                device.Should().NotBeEmpty();
            }
        }

        [SkippableTheory]
        [Trait("Category", "Integration")]
        [Trait("Category", "MacOS")]
        [InlineData(0)]
        [InlineData(-5)]
        public void IncrementVolume_WithInvalidPercentage_ShouldThrow(int percentage)
        {
            // Arrange
            Skip.IfNot(TestHelpers.IsRunningOnPlatform(TestPlatform.MacOS));
            _volumeSubsystem = new MacOSVolumeSubsystem(_logger);

            // Act
            Action act = () => _volumeSubsystem.IncrementVolume(percentage);

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>()
                .WithParameterName("percentage");
        }

        [SkippableTheory]
        [Trait("Category", "Integration")]
        [Trait("Category", "MacOS")]
        [InlineData(0)]
        [InlineData(-5)]
        public void DecrementVolume_WithInvalidPercentage_ShouldThrow(int percentage)
        {
            // Arrange
            Skip.IfNot(TestHelpers.IsRunningOnPlatform(TestPlatform.MacOS));
            _volumeSubsystem = new MacOSVolumeSubsystem(_logger);

            // Act
            Action act = () => _volumeSubsystem.DecrementVolume(percentage);

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>()
                .WithParameterName("percentage");
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        [Trait("Category", "MacOS")]
        public void Dispose_ShouldCompleteSuccessfully()
        {
            // Arrange
            Skip.IfNot(TestHelpers.IsRunningOnPlatform(TestPlatform.MacOS));
            _volumeSubsystem = new MacOSVolumeSubsystem(_logger);

            // Act
            Action act = () => _volumeSubsystem.Dispose();

            // Assert
            act.Should().NotThrow();
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        [Trait("Category", "MacOS")]
        public void Dispose_MultipleTimes_ShouldNotThrow()
        {
            // Arrange
            Skip.IfNot(TestHelpers.IsRunningOnPlatform(TestPlatform.MacOS));
            _volumeSubsystem = new MacOSVolumeSubsystem(_logger);

            // Act
            Action act = () =>
            {
                _volumeSubsystem.Dispose();
                _volumeSubsystem.Dispose();
            };

            // Assert
            act.Should().NotThrow();
        }

        public void Dispose()
        {
            _volumeSubsystem?.Dispose();
        }
    }
}