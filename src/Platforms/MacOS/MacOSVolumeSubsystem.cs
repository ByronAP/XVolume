using System;
using Microsoft.Extensions.Logging;
using XVolume.Common;
using XVolume.Utilities;

namespace XVolume.Platforms.MacOS
{
    /// <summary>
    /// macOS CoreAudio volume subsystem implementation using osascript.
    /// </summary>
    internal class MacOSVolumeSubsystem : VolumeSubsystemBase
    {
        private readonly string _shell;

        /// <summary>
        /// Initializes a new instance of the <see cref="MacOSVolumeSubsystem"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        public MacOSVolumeSubsystem(ILogger logger) : base(logger)
        {
            _shell = DetectShell();
            Logger.LogDebug("Using shell: {Shell}", _shell);
        }

        /// <inheritdoc/>
        public override string Name => "macOS CoreAudio";

        /// <summary>
        /// Detects the best shell to use for executing commands.
        /// </summary>
        /// <returns>The path to the shell executable.</returns>
        private string DetectShell()
        {
            // Try to use zsh first (default on newer macOS), then bash
            string output;
            if (CommandExecutor.TryExecute("echo $SHELL", out output, Logger, "/bin/zsh", 1000))
            {
                return "/bin/zsh";
            }

            if (CommandExecutor.TryExecute("echo $SHELL", out output, Logger, "/bin/bash", 1000))
            {
                return "/bin/bash";
            }

            // Fallback to sh
            return "/bin/sh";
        }

        /// <inheritdoc/>
        public override int Volume
        {
            get
            {
                try
                {
                    string output = CommandExecutor.Execute("osascript -e 'output volume of (get volume settings)'", Logger, _shell);
                    if (int.TryParse(output.Trim(), out int volume))
                    {
                        return volume;
                    }
                    Logger.LogWarning("Failed to parse macOS volume from output: {Output}", output);
                    return 0;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Failed to get macOS volume.");
                    return 0;
                }
            }
            set
            {
                if (value < 0 || value > 100)
                    throw new ArgumentOutOfRangeException(nameof(value), "Volume must be between 0 and 100.");

                try
                {
                    CommandExecutor.Execute($"osascript -e 'set volume output volume {value}'", Logger, _shell);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Failed to set macOS volume to {Volume}.", value);
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
                    string output = CommandExecutor.Execute("osascript -e 'output muted of (get volume settings)'", Logger, _shell);
                    return output.Trim().Equals("true", StringComparison.OrdinalIgnoreCase);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Failed to get macOS mute state.");
                    return false;
                }
            }
        }

        /// <inheritdoc/>
        public override void Mute()
        {
            try
            {
                CommandExecutor.Execute("osascript -e 'set volume output muted true'", Logger, _shell);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to mute macOS audio.");
                throw;
            }
        }

        /// <inheritdoc/>
        public override void Unmute()
        {
            try
            {
                CommandExecutor.Execute("osascript -e 'set volume output muted false'", Logger, _shell);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to unmute macOS audio.");
                throw;
            }
        }

        /// <inheritdoc/>
        public override string CurrentDevice
        {
            get
            {
                try
                {
                    // This requires additional setup and permissions
                    string output = CommandExecutor.Execute(
                        "system_profiler SPAudioDataType | grep 'Default Output Device' | cut -d: -f2",
                        Logger,
                        _shell);
                    return string.IsNullOrWhiteSpace(output) ? null : output.Trim();
                }
                catch (Exception ex)
                {
                    Logger.LogDebug(ex, "Failed to get current audio device.");
                    return null;
                }
            }
        }
    }
}