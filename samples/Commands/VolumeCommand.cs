using XVolume.Abstractions;
using XVolume.Factory;
using XVolume.Sample.Abstractions;
using XVolume.Sample.Helpers;

namespace XVolume.Sample.Commands
{
    /// <summary>
    /// Command for getting and setting system volume.
    /// </summary>
    public class VolumeCommand : ICommand
    {
        /// <inheritdoc/>
        public string Name => "volume";

        /// <inheritdoc/>
        public string Description => "Get or set system volume";

        /// <inheritdoc/>
        public string Usage => "volume [value] [+|-]";

        /// <inheritdoc/>
        public string[] Aliases => new[] { "vol", "v" };

        /// <inheritdoc/>
        public bool ValidateArgs(string[] args)
        {
            if (args.Length == 0)
                return true; // Get current volume

            if (args.Length > 2)
                return false;

            var arg = args[0];

            // Check for increment/decrement
            if (arg.StartsWith("+") || arg.StartsWith("-"))
            {
                var valueStr = arg.Substring(1);
                return int.TryParse(valueStr, out _);
            }

            // Check for absolute value
            if (!int.TryParse(arg, out var value))
                return false;

            return value >= 0 && value <= 100;
        }

        /// <inheritdoc/>
        public int Execute(string[] args)
        {
            using (var volumeSystem = VolumeSubsystemFactory.Create())
            {
                ConsoleHelper.WriteInfo($"Using: {volumeSystem.Name}");

                if (args.Length == 0)
                {
                    // Get current volume
                    ShowCurrentVolume(volumeSystem);
                    return 0;
                }

                var arg = args[0];

                // Handle increment/decrement
                if (arg.StartsWith("+"))
                {
                    var increment = int.Parse(arg.Substring(1));
                    return IncrementVolume(volumeSystem, increment);
                }
                else if (arg.StartsWith("-"))
                {
                    var decrement = int.Parse(arg.Substring(1));
                    return DecrementVolume(volumeSystem, decrement);
                }
                else
                {
                    // Set absolute volume
                    var targetVolume = int.Parse(arg);
                    return SetVolume(volumeSystem, targetVolume, args.Length > 1 && args[1] == "--smooth");
                }
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
            Console.WriteLine("Arguments:");
            Console.WriteLine("  [value]     Volume level (0-100)");
            Console.WriteLine("  +[value]    Increment volume by value");
            Console.WriteLine("  -[value]    Decrement volume by value");
            Console.WriteLine("  --smooth    Use smooth transition (when setting absolute value)");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  volume              Get current volume");
            Console.WriteLine("  volume 50           Set volume to 50%");
            Console.WriteLine("  volume 75 --smooth  Smoothly transition to 75%");
            Console.WriteLine("  volume +10          Increase volume by 10%");
            Console.WriteLine("  volume -5           Decrease volume by 5%");
            Console.WriteLine();
            Console.WriteLine($"Aliases: {string.Join(", ", Aliases)}");
        }

        private void ShowCurrentVolume(IVolumeSubsystem volumeSystem)
        {
            var volume = volumeSystem.Volume;
            var isMuted = volumeSystem.IsMuted;

            ConsoleHelper.WriteSection("Current Volume");
            ConsoleHelper.WriteVolumeMeter(volume, isMuted);
            Console.WriteLine();

            if (isMuted)
            {
                ConsoleHelper.WriteWarning("Audio is currently muted");
            }
        }

        private int SetVolume(IVolumeSubsystem volumeSystem, int targetVolume, bool smooth)
        {
            try
            {
                var currentVolume = volumeSystem.Volume;
                ConsoleHelper.WriteInfo($"Current volume: {currentVolume}%");

                if (smooth && currentVolume != targetVolume)
                {
                    ConsoleHelper.WriteInfo($"Smoothly transitioning to {targetVolume}%...");

                    var task = volumeSystem.SetVolumeSmooth(targetVolume, 2000);

                    // Show progress
                    while (!task.IsCompleted)
                    {
                        ConsoleHelper.ClearLine();
                        ConsoleHelper.WriteVolumeMeter(volumeSystem.Volume, volumeSystem.IsMuted);
                        System.Threading.Thread.Sleep(100);
                    }

                    task.Wait();
                    Console.WriteLine();
                }
                else
                {
                    volumeSystem.Volume = targetVolume;
                }

                ConsoleHelper.WriteSuccess($"Volume set to {targetVolume}%");
                ConsoleHelper.WriteVolumeMeter(targetVolume, volumeSystem.IsMuted);
                Console.WriteLine();

                return 0;
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError($"Failed to set volume: {ex.Message}");
                return 1;
            }
        }

        private int IncrementVolume(IVolumeSubsystem volumeSystem, int increment)
        {
            try
            {
                var currentVolume = volumeSystem.Volume;
                var newVolume = Math.Min(100, currentVolume + increment);

                volumeSystem.IncrementVolume(increment);

                ConsoleHelper.WriteSuccess($"Volume increased from {currentVolume}% to {newVolume}%");
                ConsoleHelper.WriteVolumeMeter(newVolume, volumeSystem.IsMuted);
                Console.WriteLine();

                return 0;
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError($"Failed to increment volume: {ex.Message}");
                return 1;
            }
        }

        private int DecrementVolume(IVolumeSubsystem volumeSystem, int decrement)
        {
            try
            {
                var currentVolume = volumeSystem.Volume;
                var newVolume = Math.Max(0, currentVolume - decrement);

                volumeSystem.DecrementVolume(decrement);

                ConsoleHelper.WriteSuccess($"Volume decreased from {currentVolume}% to {newVolume}%");
                ConsoleHelper.WriteVolumeMeter(newVolume, volumeSystem.IsMuted);
                Console.WriteLine();

                return 0;
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError($"Failed to decrement volume: {ex.Message}");
                return 1;
            }
        }
    }
}