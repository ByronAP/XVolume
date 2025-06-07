using System.Reflection;
using XVolume.Sample.Commands;
using XVolume.Sample.Helpers;

namespace XVolume.Sample
{
    /// <summary>
    /// Entry point for the XVolume sample application.
    /// </summary>
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                // Show banner for interactive mode
                if (args.Length == 0)
                {
                    ShowBanner();
                }

                // Create command parser and register commands
                var parser = new CommandParser();
                parser.RegisterCommand(new InfoCommand());
                parser.RegisterCommand(new VolumeCommand());
                parser.RegisterCommand(new MuteCommand());
                parser.RegisterCommand(new TestCommand());
                parser.RegisterCommand(new MonitorCommand());

                // Parse and execute
                return parser.Parse(args);
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError($"Unhandled exception: {ex.Message}");
                if (Environment.GetEnvironmentVariable("XVOLUME_DEBUG") == "1")
                {
                    Console.WriteLine(ex.StackTrace);
                }
                return 1;
            }
        }

        private static void ShowBanner()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";

            ConsoleHelper.WriteColored(@"
 __  ____     __    _                      
 \ \/ /\ \   / /__ | |_   _ _ __ ___   ___ 
  \  /  \ \ / / _ \| | | | | '_ ` _ \ / _ \
  /  \   \ V / (_) | | |_| | | | | | |  __/
 /_/\_\   \_/ \___/|_|\__,_|_| |_| |_|\___|
", ConsoleColor.Cyan);

            Console.WriteLine();
            ConsoleHelper.WriteColored($" Cross-Platform Volume Control Sample v{version}", ConsoleColor.White);
            Console.WriteLine();
            Console.WriteLine(" Type 'help' to see available commands");
            Console.WriteLine();
        }
    }
}