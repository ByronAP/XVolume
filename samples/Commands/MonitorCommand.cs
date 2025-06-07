using XVolume.Factory;
using XVolume.Sample.Abstractions;
using XVolume.Sample.Helpers;

namespace XVolume.Sample.Commands
{
    /// <summary>
    /// Command for monitoring volume changes in real-time.
    /// </summary>
    public class MonitorCommand : ICommand
    {
        /// <inheritdoc/>
        public string Name => "monitor";

        /// <inheritdoc/>
        public string Description => "Monitor volume changes in real-time";

        /// <inheritdoc/>
        public string Usage => "monitor [--interval <ms>]";

        /// <inheritdoc/>
        public string[] Aliases => new[] { "mon", "watch" };

        private volatile bool _shouldStop = false;

        /// <inheritdoc/>
        public bool ValidateArgs(string[] args)
        {
            if (args.Length == 0)
                return true;

            if (args.Length == 2 && args[0] == "--interval")
            {
                return int.TryParse(args[1], out var interval) && interval > 0;
            }

            return false;
        }

        /// <inheritdoc/>
        public int Execute(string[] args)
        {
            var interval = 500; // Default 500ms

            if (args.Length == 2 && args[0] == "--interval")
            {
                interval = int.Parse(args[1]);
            }

            ConsoleHelper.WriteHeader("Volume Monitor");
            ConsoleHelper.WriteInfo($"Monitoring volume changes every {interval}ms");
            ConsoleHelper.WriteInfo("Press 'Q' to quit, 'C' to clear screen");
            Console.WriteLine();

            // Set up Ctrl+C handler
            Console.CancelKeyPress += OnCancelKeyPress;

            using (var volumeSystem = VolumeSubsystemFactory.Create())
            {
                ConsoleHelper.WriteInfo($"Using: {volumeSystem.Name}");
                var device = volumeSystem.CurrentDevice;
                if (!string.IsNullOrEmpty(device))
                {
                    ConsoleHelper.WriteInfo($"Device: {device}");
                }
                Console.WriteLine();

                var lastVolume = -1;
                var lastMuted = false;
                var firstRun = true;

                while (!_shouldStop)
                {
                    try
                    {
                        var currentVolume = volumeSystem.Volume;
                        var currentMuted = volumeSystem.IsMuted;

                        if (firstRun || currentVolume != lastVolume || currentMuted != lastMuted)
                        {
                            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");

                            Console.Write($"[{timestamp}] ");
                            ConsoleHelper.WriteVolumeMeter(currentVolume, currentMuted);

                            if (!firstRun)
                            {
                                // Show what changed
                                if (currentVolume != lastVolume)
                                {
                                    var change = currentVolume - lastVolume;
                                    var changeStr = change > 0 ? $"+{change}" : change.ToString();
                                    ConsoleHelper.WriteColored($" (Volume: {changeStr}%)", ConsoleColor.Yellow);
                                }

                                if (currentMuted != lastMuted)
                                {
                                    var action = currentMuted ? "MUTED" : "UNMUTED";
                                    ConsoleHelper.WriteColored($" ({action})", ConsoleColor.Cyan);
                                }
                            }

                            Console.WriteLine();

                            lastVolume = currentVolume;
                            lastMuted = currentMuted;
                            firstRun = false;
                        }

                        // Check for key press
                        if (Console.KeyAvailable)
                        {
                            var key = Console.ReadKey(true);
                            if (key.Key == ConsoleKey.Q)
                            {
                                break;
                            }
                            else if (key.Key == ConsoleKey.C)
                            {
                                Console.Clear();
                                ConsoleHelper.WriteHeader("Volume Monitor");
                                firstRun = true;
                            }
                        }

                        Thread.Sleep(interval);
                    }
                    catch (Exception ex)
                    {
                        ConsoleHelper.WriteError($"Monitor error: {ex.Message}");
                        return 1;
                    }
                }
            }

            Console.WriteLine();
            ConsoleHelper.WriteInfo("Monitoring stopped");
            return 0;
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
            Console.WriteLine("Options:");
            Console.WriteLine("  --interval <ms>    Update interval in milliseconds (default: 500)");
            Console.WriteLine();
            Console.WriteLine("Controls:");
            Console.WriteLine("  Q          Quit monitoring");
            Console.WriteLine("  C          Clear screen");
            Console.WriteLine("  Ctrl+C     Stop monitoring");
            Console.WriteLine();
            Console.WriteLine("The monitor will display:");
            Console.WriteLine("  - Current volume level");
            Console.WriteLine("  - Mute state");
            Console.WriteLine("  - Changes as they occur");
            Console.WriteLine("  - Timestamp for each change");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  monitor                  Monitor with default 500ms interval");
            Console.WriteLine("  monitor --interval 100   Monitor with 100ms interval");
            Console.WriteLine();
            Console.WriteLine($"Aliases: {string.Join(", ", Aliases)}");
        }

        private void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            _shouldStop = true;
            e.Cancel = true;
        }
    }
}