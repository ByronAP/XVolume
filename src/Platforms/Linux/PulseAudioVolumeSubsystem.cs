using System;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using XVolume.Common;
using XVolume.Utilities;

namespace XVolume.Platforms.Linux
{
    /// <summary>
    /// PulseAudio volume subsystem implementation for Linux.
    /// </summary>
    internal class PulseAudioVolumeSubsystem : VolumeSubsystemBase
    {
        private readonly Regex _volumeRegex = new Regex(@"Volume:.*?(\d+)%", RegexOptions.Compiled);
        private readonly Regex _muteRegex = new Regex(@"Mute:\s*(yes|no)", RegexOptions.Compiled);
        private readonly Regex _deviceRegex = new Regex(@"Description:\s*(.+)", RegexOptions.Compiled);

        /// <summary>
        /// Initializes a new instance of the <see cref="PulseAudioVolumeSubsystem"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        public PulseAudioVolumeSubsystem(ILogger logger) : base(logger)
        {
        }

        /// <inheritdoc/>
        public override string Name => "PulseAudio";

        /// <inheritdoc/>
        public override string CurrentDevice
        {
            get
            {
                try
                {
                    string output = CommandExecutor.Execute("pactl list sinks", Logger);
                    var match = _deviceRegex.Match(output);
                    return match.Success ? match.Groups[1].Value.Trim() : null;
                }
                catch (Exception ex)
                {
                    Logger.LogDebug(ex, "Failed to get current audio device.");
                    return null;
                }
            }
        }

        /// <inheritdoc/>
        public override int Volume
        {
            get
            {
                try
                {
                    string output = CommandExecutor.Execute("pactl list sinks", Logger);
                    var match = _volumeRegex.Match(output);
                    if (match.Success && int.TryParse(match.Groups[1].Value, out int volume))
                    {
                        return Math.Min(100, volume); // PulseAudio can go over 100%
                    }
                    Logger.LogWarning("Failed to parse PulseAudio volume from output.");
                    return 0;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Failed to get PulseAudio volume.");
                    return 0;
                }
            }
            set
            {
                if (value < 0 || value > 100)
                    throw new ArgumentOutOfRangeException(nameof(value), "Volume must be between 0 and 100.");

                try
                {
                    CommandExecutor.Execute($"pactl set-sink-volume @DEFAULT_SINK@ {value}%", Logger);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Failed to set PulseAudio volume to {Volume}%.", value);
                    throw;
                }
            }
        }

        /// <inheritdoc/>
        public override bool IsMuted
        {
            get
            {
                try
                {
                    string output = CommandExecutor.Execute("pactl list sinks", Logger);
                    var match = _muteRegex.Match(output);
                    return match.Success && match.Groups[1].Value == "yes";
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Failed to get PulseAudio mute state.");
                    return false;
                }
            }
        }

        /// <inheritdoc/>
        public override void Mute()
        {
            try
            {
                CommandExecutor.Execute("pactl set-sink-mute @DEFAULT_SINK@ 1", Logger);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to mute PulseAudio.");
                throw;
            }
        }

        /// <inheritdoc/>
        public override void Unmute()
        {
            try
            {
                CommandExecutor.Execute("pactl set-sink-mute @DEFAULT_SINK@ 0", Logger);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to unmute PulseAudio.");
                throw;
            }
        }

        /// <inheritdoc/>
        public override void IncrementVolume(int percentage)
        {
            if (percentage <= 0)
                throw new ArgumentOutOfRangeException(nameof(percentage), "Percentage must be positive.");

            try
            {
                // Limit max volume to 100%
                int currentVolume = Volume;
                int newVolume = Math.Min(100, currentVolume + percentage);
                Volume = newVolume;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to increment PulseAudio volume by {Percentage}%.", percentage);
                throw;
            }
        }

        /// <inheritdoc/>
        public override void DecrementVolume(int percentage)
        {
            if (percentage <= 0)
                throw new ArgumentOutOfRangeException(nameof(percentage), "Percentage must be positive.");

            try
            {
                CommandExecutor.Execute($"pactl set-sink-volume @DEFAULT_SINK@ -{percentage}%", Logger);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to decrement PulseAudio volume by {Percentage}%.", percentage);
                throw;
            }
        }
    }
}