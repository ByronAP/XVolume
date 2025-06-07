using System;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using XVolume.Common;
using XVolume.Utilities;

namespace XVolume.Platforms.Linux
{
    /// <summary>
    /// ALSA volume subsystem implementation for Linux.
    /// </summary>
    internal class AlsaVolumeSubsystem : VolumeSubsystemBase
    {
        private string _controlName = "Master";
        private readonly Regex _volumeRegex = new Regex(@"\[(\d+)%\]", RegexOptions.Compiled);
        private readonly Regex _muteRegex = new Regex(@"\[(on|off)\]", RegexOptions.Compiled);

        /// <summary>
        /// Initializes a new instance of the <see cref="AlsaVolumeSubsystem"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        public AlsaVolumeSubsystem(ILogger logger) : base(logger)
        {
            DetectBestControl();
        }

        /// <inheritdoc/>
        public override string Name => "ALSA";

        /// <summary>
        /// Detects the best ALSA control to use for volume management.
        /// </summary>
        private void DetectBestControl()
        {
            try
            {
                // Try to find the best control to use
                string[] possibleControls = { "Master", "PCM", "Speaker", "Headphone" };

                foreach (var control in possibleControls)
                {
                    string output;
                    if (CommandExecutor.TryExecute($"amixer sget '{control}'", out output, Logger))
                    {
                        _controlName = control;
                        Logger.LogInformation("Using ALSA control: {Control}", _controlName);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Failed to detect best ALSA control, using default: {Control}", _controlName);
            }
        }

        /// <inheritdoc/>
        public override int Volume
        {
            get
            {
                try
                {
                    string output = CommandExecutor.Execute($"amixer -M sget '{_controlName}'", Logger);
                    var match = _volumeRegex.Match(output);
                    if (match.Success && int.TryParse(match.Groups[1].Value, out int volume))
                    {
                        return volume;
                    }
                    Logger.LogWarning("Failed to parse ALSA volume from output: {Output}", output);
                    return 0;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Failed to get ALSA volume.");
                    return 0;
                }
            }
            set
            {
                if (value < 0 || value > 100)
                    throw new ArgumentOutOfRangeException(nameof(value), "Volume must be between 0 and 100.");

                try
                {
                    CommandExecutor.Execute($"amixer -q -M sset '{_controlName}' {value}%", Logger);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Failed to set ALSA volume to {Volume}%.", value);
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
                    string output = CommandExecutor.Execute($"amixer -M sget '{_controlName}'", Logger);
                    var match = _muteRegex.Match(output);
                    return match.Success && match.Groups[1].Value == "off";
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Failed to get ALSA mute state.");
                    return false;
                }
            }
        }

        /// <inheritdoc/>
        public override void Mute()
        {
            try
            {
                CommandExecutor.Execute($"amixer -q -M sset '{_controlName}' mute", Logger);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to mute ALSA.");
                throw;
            }
        }

        /// <inheritdoc/>
        public override void Unmute()
        {
            try
            {
                CommandExecutor.Execute($"amixer -q -M sset '{_controlName}' unmute", Logger);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to unmute ALSA.");
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
                CommandExecutor.Execute($"amixer -q -M sset '{_controlName}' {percentage}%+", Logger);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to increment ALSA volume by {Percentage}%.", percentage);
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
                CommandExecutor.Execute($"amixer -q -M sset '{_controlName}' {percentage}%-", Logger);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to decrement ALSA volume by {Percentage}%.", percentage);
                throw;
            }
        }
    }
}