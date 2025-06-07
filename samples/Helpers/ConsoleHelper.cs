using System.Runtime.InteropServices;

namespace XVolume.Sample.Helpers
{
    /// <summary>
    /// Provides helper methods for console formatting and output.
    /// </summary>
    public static class ConsoleHelper
    {
        private static readonly bool SupportsColor;

        static ConsoleHelper()
        {
            // Check if console supports color
            SupportsColor = !Console.IsOutputRedirected &&
                           (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
                            !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TERM")));
        }

        /// <summary>
        /// Writes text in the specified color.
        /// </summary>
        /// <param name="text">The text to write.</param>
        /// <param name="color">The console color.</param>
        public static void WriteColored(string text, ConsoleColor color)
        {
            if (SupportsColor)
            {
                var originalColor = Console.ForegroundColor;
                Console.ForegroundColor = color;
                Console.Write(text);
                Console.ForegroundColor = originalColor;
            }
            else
            {
                Console.Write(text);
            }
        }

        /// <summary>
        /// Writes a line of text in the specified color.
        /// </summary>
        /// <param name="text">The text to write.</param>
        /// <param name="color">The console color.</param>
        public static void WriteLineColored(string text, ConsoleColor color)
        {
            WriteColored(text, color);
            Console.WriteLine();
        }

        /// <summary>
        /// Writes an error message.
        /// </summary>
        /// <param name="message">The error message.</param>
        public static void WriteError(string message)
        {
            WriteColored("[ERROR] ", ConsoleColor.Red);
            Console.WriteLine(message);
        }

        /// <summary>
        /// Writes a warning message.
        /// </summary>
        /// <param name="message">The warning message.</param>
        public static void WriteWarning(string message)
        {
            WriteColored("[WARN] ", ConsoleColor.Yellow);
            Console.WriteLine(message);
        }

        /// <summary>
        /// Writes a success message.
        /// </summary>
        /// <param name="message">The success message.</param>
        public static void WriteSuccess(string message)
        {
            WriteColored("[OK] ", ConsoleColor.Green);
            Console.WriteLine(message);
        }

        /// <summary>
        /// Writes an info message.
        /// </summary>
        /// <param name="message">The info message.</param>
        public static void WriteInfo(string message)
        {
            WriteColored("[INFO] ", ConsoleColor.Cyan);
            Console.WriteLine(message);
        }

        /// <summary>
        /// Writes a header.
        /// </summary>
        /// <param name="text">The header text.</param>
        public static void WriteHeader(string text)
        {
            Console.WriteLine();
            WriteLineColored($"=== {text} ===", ConsoleColor.White);
            Console.WriteLine();
        }

        /// <summary>
        /// Writes a section header.
        /// </summary>
        /// <param name="text">The section text.</param>
        public static void WriteSection(string text)
        {
            WriteLineColored(text, ConsoleColor.White);
            WriteLineColored(new string('-', text.Length), ConsoleColor.DarkGray);
        }

        /// <summary>
        /// Writes a progress bar.
        /// </summary>
        /// <param name="value">Current value (0-100).</param>
        /// <param name="width">Width of the progress bar.</param>
        public static void WriteProgressBar(int value, int width = 50)
        {
            value = Math.Max(0, Math.Min(100, value));
            int filled = (int)((value / 100.0) * width);

            Console.Write("[");
            WriteColored(new string('█', filled), ConsoleColor.Green);
            WriteColored(new string('░', width - filled), ConsoleColor.DarkGray);
            Console.Write($"] {value,3}%");
        }

        /// <summary>
        /// Writes a key-value pair.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="keyWidth">Width to pad the key to.</param>
        public static void WriteKeyValue(string key, string value, int keyWidth = 20)
        {
            WriteColored($"{key.PadRight(keyWidth)}: ", ConsoleColor.Gray);
            Console.WriteLine(value);
        }

        /// <summary>
        /// Prompts for user input.
        /// </summary>
        /// <param name="prompt">The prompt text.</param>
        /// <returns>The user's input.</returns>
        public static string Prompt(string prompt)
        {
            WriteColored($"{prompt}: ", ConsoleColor.Yellow);
            return Console.ReadLine();
        }

        /// <summary>
        /// Prompts for yes/no confirmation.
        /// </summary>
        /// <param name="prompt">The prompt text.</param>
        /// <param name="defaultValue">The default value if user presses Enter.</param>
        /// <returns>true for yes; false for no.</returns>
        public static bool Confirm(string prompt, bool defaultValue = false)
        {
            var defaultText = defaultValue ? "Y/n" : "y/N";
            WriteColored($"{prompt} [{defaultText}]: ", ConsoleColor.Yellow);

            var input = Console.ReadLine()?.Trim().ToLowerInvariant();

            if (string.IsNullOrEmpty(input))
                return defaultValue;

            return input == "y" || input == "yes";
        }

        /// <summary>
        /// Clears the current line.
        /// </summary>
        public static void ClearLine()
        {
            Console.Write('\r');
            Console.Write(new string(' ', Console.WindowWidth - 1));
            Console.Write('\r');
        }

        /// <summary>
        /// Writes a volume meter visualization.
        /// </summary>
        /// <param name="volume">Volume level (0-100).</param>
        /// <param name="isMuted">Whether audio is muted.</param>
        public static void WriteVolumeMeter(int volume, bool isMuted)
        {
            Console.Write("Volume: ");

            if (isMuted)
            {
                WriteColored("[MUTED] ", ConsoleColor.Red);
            }

            WriteProgressBar(volume, 30);

            // Add visual indicators
            if (volume == 0)
            {
                WriteColored(" 🔇", ConsoleColor.DarkGray);
            }
            else if (volume < 33)
            {
                WriteColored(" 🔈", ConsoleColor.Blue);
            }
            else if (volume < 66)
            {
                WriteColored(" 🔉", ConsoleColor.Cyan);
            }
            else
            {
                WriteColored(" 🔊", ConsoleColor.Green);
            }
        }
    }
}