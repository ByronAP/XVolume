using System;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using XVolume.Abstractions;
using XVolume.Common;
using XVolume.Platforms.Windows;
using XVolume.Platforms.MacOS;
using XVolume.Platforms.Linux;
using XVolume.Utilities;

namespace XVolume.Factory
{
    /// <summary>
    /// Factory for creating volume subsystem instances based on the platform and detected system.
    /// </summary>
    public static class VolumeSubsystemFactory
    {
        /// <summary>
        /// Creates an instance of the appropriate volume subsystem for the current platform.
        /// </summary>
        /// <param name="logger">Optional logger for diagnostic information. If null, a console logger is used.</param>
        /// <returns>An instance of <see cref="IVolumeSubsystem"/>.</returns>
        /// <exception cref="PlatformNotSupportedException">Thrown when the platform is not supported.</exception>
        /// <exception cref="InvalidOperationException">Thrown when no supported volume subsystem is detected.</exception>
        public static IVolumeSubsystem Create(ILogger logger = null)
        {
            ILogger effectiveLogger = logger ?? new ConsoleLogger();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                effectiveLogger.LogInformation("Windows detected. Using Windows Core Audio volume subsystem.");
                return new WindowsVolumeSubsystem(effectiveLogger);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                effectiveLogger.LogInformation("macOS detected. Using macOS CoreAudio volume subsystem.");
                return new MacOSVolumeSubsystem(effectiveLogger);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return CreateLinuxVolumeSubsystem(effectiveLogger);
            }

            effectiveLogger.LogError("Unsupported platform: {OSDescription}", RuntimeInformation.OSDescription);
            throw new PlatformNotSupportedException($"Platform '{RuntimeInformation.OSDescription}' is not supported.");
        }

        /// <summary>
        /// Creates the appropriate volume subsystem for Linux by detecting available audio systems.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <returns>An instance of <see cref="IVolumeSubsystem"/> for Linux.</returns>
        /// <exception cref="InvalidOperationException">Thrown when no supported audio system is found.</exception>
        private static IVolumeSubsystem CreateLinuxVolumeSubsystem(ILogger logger)
        {
            // Check for PipeWire first (modern systems)
            if (IsPipeWireAvailable(logger))
            {
                logger.LogInformation("PipeWire detected. Using PipeWire volume subsystem.");
                return new PipeWireVolumeSubsystem(logger);
            }

            // Check for PulseAudio
            if (IsPulseAudioAvailable(logger))
            {
                logger.LogInformation("PulseAudio detected. Using PulseAudio volume subsystem.");
                return new PulseAudioVolumeSubsystem(logger);
            }

            // Check for ALSA
            if (IsAlsaAvailable(logger))
            {
                logger.LogInformation("ALSA detected. Using ALSA volume subsystem.");
                return new AlsaVolumeSubsystem(logger);
            }

            logger.LogError("No supported volume subsystem detected on Linux (ALSA, PulseAudio, or PipeWire).");
            throw new InvalidOperationException("No supported volume subsystem detected on Linux. Please install ALSA, PulseAudio, or PipeWire.");
        }

        /// <summary>
        /// Checks if PipeWire is available on the system.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <returns>true if PipeWire is available; otherwise, false.</returns>
        private static bool IsPipeWireAvailable(ILogger logger)
        {
            try
            {
                // Check for pw-cli (PipeWire CLI tool)
                string output;
                if (CommandExecutor.TryExecute("pw-cli --version", out output, logger))
                {
                    if (output.IndexOf("pipewire", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        // Verify PulseAudio compatibility layer is available
                        if (CommandExecutor.TryExecute("pactl info", out output, logger))
                        {
                            return output.IndexOf("PipeWire", StringComparison.OrdinalIgnoreCase) >= 0;
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex, "PipeWire not available.");
                return false;
            }
        }

        /// <summary>
        /// Checks if PulseAudio is available on the system.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <returns>true if PulseAudio is available; otherwise, false.</returns>
        private static bool IsPulseAudioAvailable(ILogger logger)
        {
            try
            {
                string output;
                if (CommandExecutor.TryExecute("pactl --version", out output, logger))
                {
                    // Make sure it's actual PulseAudio and not PipeWire pretending to be PulseAudio
                    if (output.IndexOf("pulseaudio", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        // Double-check by looking at pactl info
                        if (CommandExecutor.TryExecute("pactl info", out output, logger))
                        {
                            return output.IndexOf("PulseAudio", StringComparison.OrdinalIgnoreCase) >= 0 &&
                                   output.IndexOf("PipeWire", StringComparison.OrdinalIgnoreCase) < 0;
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex, "PulseAudio not available.");
                return false;
            }
        }

        /// <summary>
        /// Checks if ALSA is available on the system.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <returns>true if ALSA is available; otherwise, false.</returns>
        private static bool IsAlsaAvailable(ILogger logger)
        {
            try
            {
                string output;
                if (CommandExecutor.TryExecute("amixer --version", out output, logger))
                {
                    return output.IndexOf("amixer", StringComparison.OrdinalIgnoreCase) >= 0;
                }
                return false;
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex, "ALSA not available.");
                return false;
            }
        }
    }
}