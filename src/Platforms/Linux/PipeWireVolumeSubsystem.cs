using System;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using XVolume.Common;
using XVolume.Utilities;

namespace XVolume.Platforms.Linux
{
    /// <summary>
    /// PipeWire volume subsystem implementation for Linux.
    /// PipeWire provides a PulseAudio compatibility layer, so we use pactl commands.
    /// </summary>
    internal class PipeWireVolumeSubsystem : VolumeSubsystemBase
    {
        private readonly Regex _volumeRegex = new Regex(@"(\d+)%", RegexOptions.Compiled);
        private readonly Regex _muteRegex = new Regex(@"Mute:\s*(yes|no)", RegexOptions.Compiled);
        private readonly Regex _deviceRegex = new Regex(@"Description:\s*(.+)", RegexOptions.Compiled);

        /// <summary>
        /// Initializes a new instance of the <see cref="PipeWireVolumeSubsystem"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        public PipeWireVolumeSubsystem(ILogger logger) : base(logger)
        {
        }

        /// <inheritdoc/>
        public override string Name => "PipeWire";

        /// <inheritdoc/>
        public override string CurrentDevice
        {
            get
            {
                try
                {
                    string output = CommandExecutor.Execute("pactl list sinks short | head -1 | cut -f2", Logger);
                    if (!string.IsNullOrWhiteSpace(output))
                    {
                        // Get more details about the device
                        string details = CommandExecutor.Execute($"pactl list sinks | grep -A 2 'Name: {output.Trim()}'", Logger);
                        var match = _deviceRegex.Match(details);
                        if (match.Success)
                        {
                            return match.Groups[1].Value.Trim();
                        }
                    }
                    return null;
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
                    // PipeWire provides PulseAudio compatibility layer
                    string output = CommandExecutor.Execute("pactl get-sink-volume @DEFAULT_SINK@", Logger);
                    var match = _volumeRegex.Match(output);
                    if (match.Success && int.TryParse(match.Groups[1].Value, out int volume))
                    {
                        return Math.Min(100, volume); // Limit to 100%
                    }
                    Logger.LogWarning("Failed to parse PipeWire volume from output: {Output}", output);
                    return 0;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Failed to get PipeWire volume.");
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
                    Logger.LogError(ex, "Failed to set PipeWire volume to {Volume}%.", value);
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
                    string output = CommandExecutor.Execute("pactl get-sink-mute @DEFAULT_SINK@", Logger);
                    return output.IndexOf("yes", StringComparison.OrdinalIgnoreCase) >= 0;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Failed to get PipeWire mute state.");
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
                Logger.LogError(ex, "Failed to mute PipeWire.");
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
                Logger.LogError(ex, "Failed to unmute PipeWire.");
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
                Logger.LogError(ex, "Failed to increment PipeWire volume by {Percentage}%.", percentage);
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
                Logger.LogError(ex, "Failed to decrement PipeWire volume by {Percentage}%.", percentage);
                throw;
            }
        }
    }
}