using System;
using System.Threading;
using System.Threading.Tasks;

namespace XVolume.Abstractions
{
    /// <summary>
    /// Defines the interface for volume control operations across platforms.
    /// </summary>
    public interface IVolumeSubsystem : IDisposable
    {
        /// <summary>
        /// Gets or sets the volume as a percentage (0-100).
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the value is not between 0 and 100.</exception>
        int Volume { get; set; }

        /// <summary>
        /// Gets a value indicating whether the audio is muted.
        /// </summary>
        bool IsMuted { get; }

        /// <summary>
        /// Mutes the audio.
        /// </summary>
        void Mute();

        /// <summary>
        /// Unmutes the audio.
        /// </summary>
        void Unmute();

        /// <summary>
        /// Toggles the mute state.
        /// </summary>
        void ToggleMute();

        /// <summary>
        /// Increments the volume by a specified percentage.
        /// </summary>
        /// <param name="percentage">The percentage to increase the volume by.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when percentage is less than or equal to 0.</exception>
        void IncrementVolume(int percentage);

        /// <summary>
        /// Decrements the volume by a specified percentage.
        /// </summary>
        /// <param name="percentage">The percentage to decrease the volume by.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when percentage is less than or equal to 0.</exception>
        void DecrementVolume(int percentage);

        /// <summary>
        /// Sets the volume with a smooth transition.
        /// </summary>
        /// <param name="targetVolume">Target volume (0-100).</param>
        /// <param name="durationMs">Duration of the transition in milliseconds.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when targetVolume is not between 0 and 100, or when durationMs is less than or equal to 0.</exception>
        Task SetVolumeSmooth(int targetVolume, int durationMs = 500, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Gets the name of the volume subsystem.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the current audio device name if available.
        /// </summary>
        string CurrentDevice { get; }
    }
}