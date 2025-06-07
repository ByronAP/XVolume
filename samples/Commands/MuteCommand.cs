using XVolume.Factory;
using XVolume.Sample.Abstractions;
using XVolume.Sample.Helpers;

namespace XVolume.Sample.Commands
{
    /// <summary>
    /// Command for muting, unmuting, and toggling audio mute state.
    /// </summary>
    public class MuteCommand : ICommand
    {
        /// <inheritdoc/>
        public string Name => "mute";

        /// <inheritdoc/>
        public string Description => "Control audio mute state";

        /// <inheritdoc/>
        public string Usage => "mute [on|off|toggle]";

        /// <inheritdoc/>
        public string[] Aliases => new[] { "m" };

        /// <inheritdoc/>
        public bool ValidateArgs(string[] args)
        {
            if (args.Length == 0)
                return true; // Default to toggle

            if (args.Length > 1)
                return false;

            var action = args[0].ToLowerInvariant();
            return action == "on" || action == "off" || action == "toggle";
        }

        /// <inheritdoc/>
        public int Execute(string[] args)
        {
            using (var volumeSystem = VolumeSubsystemFactory.Create())
            {
                ConsoleHelper.WriteInfo($"Using: {volumeSystem.Name}");

                var action = args.Length > 0 ? args[0].ToLowerInvariant() : "toggle";

                try
                {
                    var wasMuted = volumeSystem.IsMuted;
                    var currentVolume = volumeSystem.Volume;

                    switch (action)
                    {
                        case "on":
                            volumeSystem.Mute();
                            ConsoleHelper.WriteSuccess("Audio muted");
                            break;

                        case "off":
                            volumeSystem.Unmute();
                            ConsoleHelper.WriteSuccess("Audio unmuted");
                            break;

                        case "toggle":
                            volumeSystem.ToggleMute();
                            var newState = volumeSystem.IsMuted ? "muted" : "unmuted";
                            ConsoleHelper.WriteSuccess($"Audio {newState}");
                            break;
                    }

                    ConsoleHelper.WriteVolumeMeter(currentVolume, volumeSystem.IsMuted);
                    Console.WriteLine();

                    return 0;
                }
                catch (Exception ex)
                {
                    ConsoleHelper.WriteError($"Failed to change mute state: {ex.Message}");
                    return 1;
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
            Console.WriteLine("  on      Mute audio");
            Console.WriteLine("  off     Unmute audio");
            Console.WriteLine("  toggle  Toggle mute state (default)");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  mute           Toggle mute state");
            Console.WriteLine("  mute on        Mute audio");
            Console.WriteLine("  mute off       Unmute audio");
            Console.WriteLine("  mute toggle    Toggle mute state");
            Console.WriteLine();
            Console.WriteLine($"Aliases: {string.Join(", ", Aliases)}");
        }
    }
}