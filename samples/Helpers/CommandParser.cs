using XVolume.Sample.Abstractions;

namespace XVolume.Sample.Helpers
{
    /// <summary>
    /// Parses and routes command-line arguments to appropriate commands.
    /// </summary>
    public class CommandParser
    {
        private readonly Dictionary<string, ICommand> _commands;
        private readonly Dictionary<string, string> _aliases;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandParser"/> class.
        /// </summary>
        public CommandParser()
        {
            _commands = new Dictionary<string, ICommand>(StringComparer.OrdinalIgnoreCase);
            _aliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Registers a command with the parser.
        /// </summary>
        /// <param name="command">The command to register.</param>
        public void RegisterCommand(ICommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            _commands[command.Name] = command;

            // Register aliases
            foreach (var alias in command.Aliases)
            {
                _aliases[alias] = command.Name;
            }
        }

        /// <summary>
        /// Parses and executes the command from the given arguments.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <returns>Exit code. 0 for success, non-zero for failure.</returns>
        public int Parse(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                ShowAllHelp();
                return 0;
            }

            var commandName = args[0];
            var commandArgs = args.Skip(1).ToArray();

            // Check for global help
            if (IsHelpCommand(commandName))
            {
                if (commandArgs.Length > 0 && _commands.ContainsKey(commandArgs[0]))
                {
                    _commands[commandArgs[0]].ShowHelp();
                }
                else
                {
                    ShowAllHelp();
                }
                return 0;
            }

            // Find command
            var command = FindCommand(commandName);
            if (command == null)
            {
                ConsoleHelper.WriteError($"Unknown command: '{commandName}'");
                Console.WriteLine($"Try 'help' to see available commands.");
                return 1;
            }

            // Check for command-specific help
            if (commandArgs.Length > 0 && IsHelpCommand(commandArgs[0]))
            {
                command.ShowHelp();
                return 0;
            }

            // Validate arguments
            if (!command.ValidateArgs(commandArgs))
            {
                ConsoleHelper.WriteError($"Invalid arguments for command '{command.Name}'");
                command.ShowHelp();
                return 1;
            }

            // Execute command
            try
            {
                return command.Execute(commandArgs);
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError($"Error executing command '{command.Name}': {ex.Message}");
                if (Environment.GetEnvironmentVariable("XVOLUME_DEBUG") == "1")
                {
                    Console.WriteLine(ex.StackTrace);
                }
                return 1;
            }
        }

        /// <summary>
        /// Finds a command by name or alias.
        /// </summary>
        /// <param name="name">The command name or alias.</param>
        /// <returns>The command if found; otherwise, null.</returns>
        private ICommand FindCommand(string name)
        {
            // Direct command lookup
            if (_commands.TryGetValue(name, out var command))
                return command;

            // Alias lookup
            if (_aliases.TryGetValue(name, out var commandName))
                return _commands[commandName];

            return null;
        }

        /// <summary>
        /// Determines if the given argument is a help command.
        /// </summary>
        /// <param name="arg">The argument to check.</param>
        /// <returns>true if it's a help command; otherwise, false.</returns>
        private bool IsHelpCommand(string arg)
        {
            return arg.Equals("help", StringComparison.OrdinalIgnoreCase) ||
                   arg.Equals("-h", StringComparison.OrdinalIgnoreCase) ||
                   arg.Equals("--help", StringComparison.OrdinalIgnoreCase) ||
                   arg.Equals("-?", StringComparison.OrdinalIgnoreCase) ||
                   arg.Equals("/?", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Shows help for all registered commands.
        /// </summary>
        private void ShowAllHelp()
        {
            ConsoleHelper.WriteHeader("XVolume Sample Application");
            Console.WriteLine();
            Console.WriteLine("Usage: XVolume.Sample <command> [arguments]");
            Console.WriteLine();
            ConsoleHelper.WriteSection("Available Commands:");

            var maxNameLength = _commands.Values.Max(c => c.Name.Length);
            foreach (var command in _commands.Values.OrderBy(c => c.Name))
            {
                Console.Write("  ");
                ConsoleHelper.WriteColored(command.Name.PadRight(maxNameLength + 2), ConsoleColor.Yellow);
                Console.WriteLine(command.Description);
            }

            Console.WriteLine();
            Console.WriteLine("Use 'help <command>' to see detailed help for a specific command.");
            Console.WriteLine();
            ConsoleHelper.WriteSection("Examples:");
            Console.WriteLine("  XVolume.Sample info");
            Console.WriteLine("  XVolume.Sample volume 50");
            Console.WriteLine("  XVolume.Sample mute toggle");
            Console.WriteLine("  XVolume.Sample test");
            Console.WriteLine();
            Console.WriteLine("Set XVOLUME_DEBUG=1 environment variable to see detailed error information.");
        }
    }
}