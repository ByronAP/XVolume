using Microsoft.Extensions.Logging;
using Xunit.Abstractions;
using XVolume.Platforms.Linux;
using XVolume.Tests.Helpers;
using FluentAssertions;

namespace XVolume.Tests.Platforms
{
    /// <summary>
    /// Unit tests for Linux volume subsystems (ALSA, PulseAudio, PipeWire).
    /// </summary>
    public class LinuxVolumeSubsystemTests : IDisposable
    {
        private readonly ILogger _logger;
        private IDisposable _volumeSubsystem;

        public LinuxVolumeSubsystemTests(ITestOutputHelper output)
        {
            _logger = TestHelpers.CreateTestLogger(output, nameof(LinuxVolumeSubsystemTests));
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        [Trait("Category", "Linux")]
        public void AlsaVolumeSubsystem_Constructor_ShouldInitializeSuccessfully()
        {
            // Arrange
            Skip.IfNot(TestHelpers.IsRunningOnPlatform(TestPlatform.Linux));

            // Act
            Action act = () => _volumeSubsystem = new AlsaVolumeSubsystem(_logger);

            // Assert
            // May throw if ALSA is not available, which is expected
            try
            {
                act();
                _volumeSubsystem.Should().NotBeNull();
            }
            catch (InvalidOperationException)
            {
                // ALSA not available on this system
                Skip.If(true, "ALSA not available on this Linux system");
            }
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        [Trait("Category", "Linux")]
        public void PulseAudioVolumeSubsystem_Constructor_ShouldInitializeSuccessfully()
        {
            // Arrange
            Skip.IfNot(TestHelpers.IsRunningOnPlatform(TestPlatform.Linux));

            // Act
            Action act = () => _volumeSubsystem = new PulseAudioVolumeSubsystem(_logger);

            // Assert
            act.Should().NotThrow();
            _volumeSubsystem.Should().NotBeNull();
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        [Trait("Category", "Linux")]
        public void PipeWireVolumeSubsystem_Constructor_ShouldInitializeSuccessfully()
        {
            // Arrange
            Skip.IfNot(TestHelpers.IsRunningOnPlatform(TestPlatform.Linux));

            // Act
            Action act = () => _volumeSubsystem = new PipeWireVolumeSubsystem(_logger);

            // Assert
            act.Should().NotThrow();
            _volumeSubsystem.Should().NotBeNull();
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        [Trait("Category", "Linux")]
        public void AlsaVolumeSubsystem_Name_ShouldReturnALSA()
        {
            // Arrange
            Skip.IfNot(TestHelpers.IsRunningOnPlatform(TestPlatform.Linux));

            try
            {
                var alsa = new AlsaVolumeSubsystem(_logger);
                _volumeSubsystem = alsa;

                // Act
                var name = alsa.Name;

                // Assert
                name.Should().Be("ALSA");
            }
            catch (InvalidOperationException)
            {
                Skip.If(true, "ALSA not available on this Linux system");
            }
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        [Trait("Category", "Linux")]
        public void PulseAudioVolumeSubsystem_Name_ShouldReturnPulseAudio()
        {
            // Arrange
            Skip.IfNot(TestHelpers.IsRunningOnPlatform(TestPlatform.Linux));
            var pulseAudio = new PulseAudioVolumeSubsystem(_logger);
            _volumeSubsystem = pulseAudio;

            // Act
            var name = pulseAudio.Name;

            // Assert
            name.Should().Be("PulseAudio");
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        [Trait("Category", "Linux")]
        public void PipeWireVolumeSubsystem_Name_ShouldReturnPipeWire()
        {
            // Arrange
            Skip.IfNot(TestHelpers.IsRunningOnPlatform(TestPlatform.Linux));
            var pipeWire = new PipeWireVolumeSubsystem(_logger);
            _volumeSubsystem = pipeWire;

            // Act
            var name = pipeWire.Name;

            // Assert
            name.Should().Be("PipeWire");
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        [Trait("Category", "Linux")]
        public void LinuxSubsystems_Volume_Get_ShouldReturnValueBetween0And100()
        {
            // Arrange
            Skip.IfNot(TestHelpers.IsRunningOnPlatform(TestPlatform.Linux));

            // Try to find a working audio subsystem
            var subsystem = TryCreateWorkingSubsystem();
            if (subsystem == null)
            {
                Skip.If(true, "No audio subsystem available on this Linux system");
                return;
            }

            // Act
            var volume = subsystem.Volume;

            // Assert
            volume.Should().BeInRange(0, 100);
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        [Trait("Category", "Linux")]
        public void LinuxSubsystems_Volume_Set_ShouldUpdateVolume()
        {
            // Arrange
            Skip.IfNot(TestHelpers.IsRunningOnPlatform(TestPlatform.Linux));

            var subsystem = TryCreateWorkingSubsystem();
            if (subsystem == null)
            {
                Skip.If(true, "No audio subsystem available on this Linux system");
                return;
            }

            var originalVolume = subsystem.Volume;
            var testVolume = originalVolume == 50 ? 60 : 50;

            try
            {
                // Act
                subsystem.Volume = testVolume;

                // Assert
                subsystem.Volume.Should().Be(testVolume);
            }
            finally
            {
                // Restore original volume
                subsystem.Volume = originalVolume;
            }
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        [Trait("Category", "Linux")]
        public void LinuxSubsystems_MuteUnmute_ShouldToggleState()
        {
            // Arrange
            Skip.IfNot(TestHelpers.IsRunningOnPlatform(TestPlatform.Linux));

            var subsystem = TryCreateWorkingSubsystem();
            if (subsystem == null)
            {
                Skip.If(true, "No audio subsystem available on this Linux system");
                return;
            }

            var originalMuteState = subsystem.IsMuted;

            try
            {
                // Act & Assert - Mute
                subsystem.Mute();
                subsystem.IsMuted.Should().BeTrue();

                // Act & Assert - Unmute
                subsystem.Unmute();
                subsystem.IsMuted.Should().BeFalse();
            }
            finally
            {
                // Restore original state
                if (originalMuteState)
                    subsystem.Mute();
                else
                    subsystem.Unmute();
            }
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        [Trait("Category", "Linux")]
        public void PulseAudioVolumeSubsystem_CurrentDevice_ShouldReturnDeviceOrNull()
        {
            // Arrange
            Skip.IfNot(TestHelpers.IsRunningOnPlatform(TestPlatform.Linux));
            var pulseAudio = new PulseAudioVolumeSubsystem(_logger);
            _volumeSubsystem = pulseAudio;

            // Act
            var device = pulseAudio.CurrentDevice;

            // Assert
            // Device can be null if no device is available
            if (device != null)
            {
                device.Should().NotBeEmpty();
            }
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        [Trait("Category", "Linux")]
        public void LinuxSubsystems_IncrementVolume_ShouldIncreaseVolume()
        {
            // Arrange
            Skip.IfNot(TestHelpers.IsRunningOnPlatform(TestPlatform.Linux));

            var subsystem = TryCreateWorkingSubsystem();
            if (subsystem == null)
            {
                Skip.If(true, "No audio subsystem available on this Linux system");
                return;
            }

            var originalVolume = subsystem.Volume;

            // Ensure we have room to increment
            if (originalVolume > 90)
            {
                subsystem.Volume = 50;
            }

            try
            {
                // Act
                var volumeBefore = subsystem.Volume;
                subsystem.IncrementVolume(10);

                // Assert
                var expectedVolume = Math.Min(100, volumeBefore + 10);
                subsystem.Volume.Should().BeCloseTo(expectedVolume, 1); // Allow 1% tolerance
            }
            finally
            {
                // Restore original volume
                subsystem.Volume = originalVolume;
            }
        }

        private dynamic TryCreateWorkingSubsystem()
        {
            // Try PulseAudio first (most common)
            try
            {
                var pulseAudio = new PulseAudioVolumeSubsystem(_logger);
                _volumeSubsystem = pulseAudio;
                // Test if it works
                var _ = pulseAudio.Volume;
                return pulseAudio;
            }
            catch { }

            // Try PipeWire
            try
            {
                var pipeWire = new PipeWireVolumeSubsystem(_logger);
                _volumeSubsystem = pipeWire;
                // Test if it works
                var _ = pipeWire.Volume;
                return pipeWire;
            }
            catch { }

            // Try ALSA
            try
            {
                var alsa = new AlsaVolumeSubsystem(_logger);
                _volumeSubsystem = alsa;
                // Test if it works
                var _ = alsa.Volume;
                return alsa;
            }
            catch { }

            return null;
        }

        public void Dispose()
        {
            _volumeSubsystem?.Dispose();
        }
    }
}