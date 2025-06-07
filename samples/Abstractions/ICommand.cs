namespace XVolume.Sample.Abstractions
{
    /// <summary>
    /// Defines the interface for sample application commands.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Gets the name of the command.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the description of the command.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets the usage syntax for the command.
        /// </summary>
        string Usage { get; }

        /// <summary>
        /// Gets the aliases for the command.
        /// </summary>
        string[] Aliases { get; }

        /// <summary>
        /// Executes the command with the specified arguments.
        /// </summary>
        /// <param name="args">The command arguments.</param>
        /// <returns>Exit code. 0 for success, non-zero for failure.</returns>
        int Execute(string[] args);

        /// <summary>
        /// Validates the arguments for the command.
        /// </summary>
        /// <param name="args">The command arguments.</param>
        /// <returns>true if arguments are valid; otherwise, false.</returns>
        bool ValidateArgs(string[] args);

        /// <summary>
        /// Displays help information for the command.
        /// </summary>
        void ShowHelp();
    }
}