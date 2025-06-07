using Microsoft.Extensions.Logging;
using Xunit.Abstractions;
using XVolume.Tests.Helpers;
using XVolume.Tests.Mocks;
using FluentAssertions;

namespace XVolume.Tests.Common
{
    /// <summary>
    /// Unit tests for the VolumeSubsystemBase class.
    /// </summary>
    public class VolumeSubsystemBaseTests
    {
        private readonly ILogger _logger;
        private readonly MockVolumeSubsystem _mockSubsystem;

        public VolumeSubsystemBaseTests(ITestOutputHelper output)
        {
            _logger = TestHelpers.CreateTestLogger(output, nameof(VolumeSubsystemBaseTests));
            _mockSubsystem = new MockVolumeSubsystem(_logger);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ToggleMute_WhenMuted_ShouldUnmute()
        {
            // Arrange
            _mockSubsystem.Mute();
            _mockSubsystem.IsMuted.Should().BeTrue();

            // Act
            _mockSubsystem.ToggleMute();

            // Assert
            _mockSubsystem.IsMuted.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ToggleMute_WhenUnmuted_ShouldMute()
        {
            // Arrange
            _mockSubsystem.Unmute();
            _mockSubsystem.IsMuted.Should().BeFalse();

            // Act
            _mockSubsystem.ToggleMute();

            // Assert
            _mockSubsystem.IsMuted.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void IncrementVolume_ShouldIncreaseVolume()
        {
            // Arrange
            _mockSubsystem.Volume = 50;

            // Act
            _mockSubsystem.IncrementVolume(10);

            // Assert
            _mockSubsystem.Volume.Should().Be(60);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void IncrementVolume_AtMaxVolume_ShouldNotExceed100()
        {
            // Arrange
            _mockSubsystem.Volume = 95;

            // Act
            _mockSubsystem.IncrementVolume(10);

            // Assert
            _mockSubsystem.Volume.Should().Be(100);
        }

        [Theory]
        [Trait("Category", "Unit")]
        [InlineData(0)]
        [InlineData(-5)]
        public void IncrementVolume_WithInvalidPercentage_ShouldThrow(int percentage)
        {
            // Act
            Action act = () => _mockSubsystem.IncrementVolume(percentage);

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>()
                .WithParameterName("percentage");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DecrementVolume_ShouldDecreaseVolume()
        {
            // Arrange
            _mockSubsystem.Volume = 50;

            // Act
            _mockSubsystem.DecrementVolume(10);

            // Assert
            _mockSubsystem.Volume.Should().Be(40);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DecrementVolume_AtMinVolume_ShouldNotGoBelowZero()
        {
            // Arrange
            _mockSubsystem.Volume = 5;

            // Act
            _mockSubsystem.DecrementVolume(10);

            // Assert
            _mockSubsystem.Volume.Should().Be(0);
        }

        [Theory]
        [Trait("Category", "Unit")]
        [InlineData(0)]
        [InlineData(-5)]
        public void DecrementVolume_WithInvalidPercentage_ShouldThrow(int percentage)
        {
            // Act
            Action act = () => _mockSubsystem.DecrementVolume(percentage);

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>()
                .WithParameterName("percentage");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task SetVolumeSmooth_ShouldTransitionToTargetVolume()
        {
            // Arrange
            _mockSubsystem.Volume = 20;
            int targetVolume = 80;

            // Act
            await _mockSubsystem.SetVolumeSmooth(targetVolume, 100);

            // Assert
            _mockSubsystem.Volume.Should().Be(targetVolume);
            _mockSubsystem.SetVolumeSmoothCallCount.Should().Be(1);
            _mockSubsystem.LastSmoothTargetVolume.Should().Be(targetVolume);
            _mockSubsystem.LastSmoothDurationMs.Should().Be(100);
        }

        [Theory]
        [Trait("Category", "Unit")]
        [InlineData(-1)]
        [InlineData(101)]
        public async Task SetVolumeSmooth_WithInvalidTargetVolume_ShouldThrow(int targetVolume)
        {
            // Act
            Func<Task> act = async () => await _mockSubsystem.SetVolumeSmooth(targetVolume);

            // Assert
            await act.Should().ThrowAsync<ArgumentOutOfRangeException>()
                .WithParameterName("targetVolume");
        }

        [Theory]
        [Trait("Category", "Unit")]
        [InlineData(0)]
        [InlineData(-100)]
        public async Task SetVolumeSmooth_WithInvalidDuration_ShouldThrow(int duration)
        {
            // Act
            Func<Task> act = async () => await _mockSubsystem.SetVolumeSmooth(50, duration);

            // Assert
            await act.Should().ThrowAsync<ArgumentOutOfRangeException>()
                .WithParameterName("durationMs");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task SetVolumeSmooth_WhenCancelled_ShouldStopTransition()
        {
            // Arrange
            _mockSubsystem.Volume = 0;
            using var cts = new CancellationTokenSource();

            // Act
            var task = _mockSubsystem.SetVolumeSmooth(100, 1000, cts.Token);
            await Task.Delay(50); // Let it start
            cts.Cancel();
            await task; // Should complete without exception

            // Assert
            _mockSubsystem.Volume.Should().BeGreaterThan(0).And.BeLessThan(100);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task SetVolumeSmooth_WithSameVolume_ShouldCompleteImmediately()
        {
            // Arrange
            _mockSubsystem.Volume = 50;

            // Act
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            await _mockSubsystem.SetVolumeSmooth(50, 1000);
            stopwatch.Stop();

            // Assert
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(100);
            _mockSubsystem.Volume.Should().Be(50);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Dispose_ShouldCompleteWithoutException()
        {
            // Arrange
            var subsystem = new MockVolumeSubsystem(_logger);

            // Act
            Action act = () => subsystem.Dispose();

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Dispose_MultipleTimes_ShouldNotThrow()
        {
            // Arrange
            var subsystem = new MockVolumeSubsystem(_logger);

            // Act
            Action act = () =>
            {
                subsystem.Dispose();
                subsystem.Dispose();
            };

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Name_ShouldReturnExpectedValue()
        {
            // Assert
            _mockSubsystem.Name.Should().Be("Mock Volume Subsystem");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void CurrentDevice_ShouldReturnExpectedValue()
        {
            // Assert
            _mockSubsystem.CurrentDevice.Should().Be("Mock Audio Device");
        }
    }
}