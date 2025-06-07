using Microsoft.Extensions.Logging;
using Xunit.Abstractions;
using XVolume.Platforms.Windows;
using XVolume.Tests.Helpers;
using FluentAssertions;

namespace XVolume.Tests.Platforms
{
    /// <summary>
    /// Unit tests for the WindowsVolumeSubsystem class.
    /// </summary>
    public class WindowsVolumeSubsystemTests : IDisposable
    {
        private readonly ILogger _logger;
        private WindowsVolumeSubsystem _volumeSubsystem;

        public WindowsVolumeSubsystemTests(ITestOutputHelper output)
        {
            _logger = TestHelpers.CreateTestLogger(output, nameof(WindowsVolumeSubsystemTests));
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        [Trait("Category", "Windows")]
        public void Constructor_ShouldInitializeSuccessfully()
        {
            // Arrange
            Skip.IfNot(TestHelpers.IsRunningOnPlatform(TestPlatform.Windows));

            // Act
            Action act = () => _volumeSubsystem = new WindowsVolumeSubsystem(_logger);

            // Assert
            act.Should().NotThrow();
            _volumeSubsystem.Should().NotBeNull();
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        [Trait("Category", "Windows")]
        public void Name_ShouldReturnWindowsCoreAudio()
        {
            // Arrange
            Skip.IfNot(TestHelpers.IsRunningOnPlatform(TestPlatform.Windows));
            _volumeSubsystem = new WindowsVolumeSubsystem(_logger);

            // Act
            var name = _volumeSubsystem.Name;

            // Assert
            name.Should().Be("Windows Core Audio");
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        [Trait("Category", "Windows")]
        public void Volume_Get_ShouldReturnValueBetween0And100()
        {
            // Arrange
            Skip.IfNot(TestHelpers.IsRunningOnPlatform(TestPlatform.Windows));
            _volumeSubsystem = new WindowsVolumeSubsystem(_logger);

            // Act
            var volume = _volumeSubsystem.Volume;

            // Assert
            volume.Should().BeInRange(0, 100);
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        [Trait("Category", "Windows")]
        public void Volume_Set_ShouldUpdateVolume()
        {
            // Arrange
            Skip.IfNot(TestHelpers.IsRunningOnPlatform(TestPlatform.Windows));
            _volumeSubsystem = new WindowsVolumeSubsystem(_logger);
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
        [Trait("Category", "Windows")]
        [InlineData(-1)]
        [InlineData(101)]
        public void Volume_Set_WithInvalidValue_ShouldThrow(int invalidVolume)
        {
            // Arrange
            Skip.IfNot(TestHelpers.IsRunningOnPlatform(TestPlatform.Windows));
            _volumeSubsystem = new WindowsVolumeSubsystem(_logger);

            // Act
            Action act = () => _volumeSubsystem.Volume = invalidVolume;

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>()
                .WithParameterName("value");
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        [Trait("Category", "Windows")]
        public void Mute_Unmute_ShouldToggleMuteState()
        {
            // Arrange
            Skip.IfNot(TestHelpers.IsRunningOnPlatform(TestPlatform.Windows));
            _volumeSubsystem = new WindowsVolumeSubsystem(_logger);
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
        [Trait("Category", "Windows")]
        public void IncrementVolume_ShouldIncreaseVolume()
        {
            // Arrange
            Skip.IfNot(TestHelpers.IsRunningOnPlatform(TestPlatform.Windows));
            _volumeSubsystem = new WindowsVolumeSubsystem(_logger);
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
        [Trait("Category", "Windows")]
        public void DecrementVolume_ShouldDecreaseVolume()
        {
            // Arrange
            Skip.IfNot(TestHelpers.IsRunningOnPlatform(TestPlatform.Windows));
            _volumeSubsystem = new WindowsVolumeSubsystem(_logger);
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
        [Trait("Category", "Windows")]
        public void Dispose_ShouldReleaseResources()
        {
            // Arrange
            Skip.IfNot(TestHelpers.IsRunningOnPlatform(TestPlatform.Windows));
            _volumeSubsystem = new WindowsVolumeSubsystem(_logger);

            // Act
            Action act = () => _volumeSubsystem.Dispose();

            // Assert
            act.Should().NotThrow();
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        [Trait("Category", "Windows")]
        public void Dispose_MultipleTimes_ShouldNotThrow()
        {
            // Arrange
            Skip.IfNot(TestHelpers.IsRunningOnPlatform(TestPlatform.Windows));
            _volumeSubsystem = new WindowsVolumeSubsystem(_logger);

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