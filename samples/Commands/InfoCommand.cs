using System.Runtime.InteropServices;
using XVolume.Factory;
using XVolume.Sample.Abstractions;
using XVolume.Sample.Helpers;

namespace XVolume.Sample.Commands
{
    /// <summary>
    /// Command for displaying system and audio information.
    /// </summary>
    public class InfoCommand : ICommand
    {
        /// <inheritdoc/>
        public string Name => "info";

        /// <inheritdoc/>
        public string Description => "Display system and audio information";

        /// <inheritdoc/>
        public string Usage => "info";

        /// <inheritdoc/>
        public string[] Aliases => new[] { "i", "status" };

        /// <inheritdoc/>
        public bool ValidateArgs(string[] args)
        {
            return args.Length == 0;
        }

        /// <inheritdoc/>
        public int Execute(string[] args)
        {
            try
            {
                ShowSystemInfo();
                Console.WriteLine();
                ShowAudioInfo();
                return 0;
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError($"Failed to get system information: {ex.Message}");
                return 1;
            }
        }

        /// <inheritdoc/>
        public void ShowHelp()
        {
            ConsoleHelper.WriteHeader($"Help: {Name}");
            Console.WriteLine($"Description: {Description}");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine($"  {Usage}");
            Console.WriteLine();
            Console.WriteLine("This command displays:");
            Console.WriteLine("  - Operating system information");
            Console.WriteLine("  - .NET runtime information");
            Console.WriteLine("  - Audio subsystem information");
            Console.WriteLine("  - Current volume and mute state");
            Console.WriteLine("  - Audio device information (if available)");
            Console.WriteLine();
            Console.WriteLine($"Aliases: {string.Join(", ", Aliases)}");
        }

        private void ShowSystemInfo()
        {
            ConsoleHelper.WriteSection("System Information");
            ConsoleHelper.WriteKeyValue("OS", RuntimeInformation.OSDescription);
            ConsoleHelper.WriteKeyValue("Architecture", RuntimeInformation.OSArchitecture.ToString());
            ConsoleHelper.WriteKeyValue("Framework", RuntimeInformation.FrameworkDescription);
            ConsoleHelper.WriteKeyValue("Process Architecture", RuntimeInformation.ProcessArchitecture.ToString());
            ConsoleHelper.WriteKeyValue("Machine Name", Environment.MachineName);
            ConsoleHelper.WriteKeyValue("User Name", Environment.UserName);
        }

        private void ShowAudioInfo()
        {
            ConsoleHelper.WriteSection("Audio Information");

            using (var volumeSystem = VolumeSubsystemFactory.Create())
            {
                ConsoleHelper.WriteKeyValue("Audio System", volumeSystem.Name);

                var currentDevice = volumeSystem.CurrentDevice;
                if (!string.IsNullOrEmpty(currentDevice))
                {
                    ConsoleHelper.WriteKeyValue("Current Device", currentDevice);
                }
                else
                {
                    ConsoleHelper.WriteKeyValue("Current Device", "Not available");
                }

                Console.WriteLine();
                ConsoleHelper.WriteSection("Volume Status");

                var volume = volumeSystem.Volume;
                var isMuted = volumeSystem.IsMuted;

                ConsoleHelper.WriteKeyValue("Volume Level", $"{volume}%");
                ConsoleHelper.WriteKeyValue("Mute State", isMuted ? "Muted" : "Unmuted");

                Console.WriteLine();
                ConsoleHelper.WriteVolumeMeter(volume, isMuted);
                Console.WriteLine();

                // Platform-specific information
                ShowPlatformSpecificInfo();
            }
        }

        private void ShowPlatformSpecificInfo()
        {
            ConsoleHelper.WriteSection("Platform Details");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                ConsoleHelper.WriteInfo("Windows Core Audio API is being used");
                ConsoleHelper.WriteInfo("COM-based audio control interface");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                ConsoleHelper.WriteInfo("Linux audio subsystem detected");
                ConsoleHelper.WriteInfo("Possible systems: ALSA, PulseAudio, PipeWire");

                // Check which audio systems are available
                CheckLinuxAudioSystem("amixer --version", "ALSA");
                CheckLinuxAudioSystem("pactl --version", "PulseAudio");
                CheckLinuxAudioSystem("pw-cli --version", "PipeWire");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                ConsoleHelper.WriteInfo("macOS CoreAudio is being used");
                ConsoleHelper.WriteInfo("Using osascript for volume control");
            }
        }

        private void CheckLinuxAudioSystem(string command, string systemName)
        {
            try
            {
                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{command}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = System.Diagnostics.Process.Start(startInfo))
                {
                    process.WaitForExit(1000);
                    if (process.ExitCode == 0)
                    {
                        ConsoleHelper.WriteSuccess($"{systemName} is available");
                    }
                }
            }
            catch
            {
                // Ignore errors
            }
        }
    }
}