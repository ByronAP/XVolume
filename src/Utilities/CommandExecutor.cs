using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.Logging;

namespace XVolume.Utilities
{
    /// <summary>
    /// Provides utility methods for executing shell commands across platforms.
    /// </summary>
    internal static class CommandExecutor
    {
        /// <summary>
        /// Executes a shell command and returns the output.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="logger">Optional logger for diagnostic output.</param>
        /// <param name="shell">The shell to use. If null, defaults to cmd.exe on Windows or /bin/sh on Unix.</param>
        /// <param name="timeoutMs">The timeout in milliseconds. Default is 5000ms.</param>
        /// <returns>The command output.</returns>
        /// <exception cref="ArgumentNullException">Thrown when command is null or empty.</exception>
        /// <exception cref="TimeoutException">Thrown when the command execution times out.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the command fails with a non-zero exit code.</exception>
        public static string Execute(string command, ILogger logger = null, string shell = null, int timeoutMs = 5000)
        {
            if (string.IsNullOrEmpty(command))
                throw new ArgumentNullException(nameof(command));

            if (shell == null)
                shell = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "cmd.exe" : "/bin/sh";

            string args = shell.IndexOf("cmd", StringComparison.OrdinalIgnoreCase) >= 0
                ? $"/c {command}"
                : $"-c \"{command}\"";

            var processInfo = new ProcessStartInfo
            {
                FileName = shell,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = new Process { StartInfo = processInfo })
            {
                var outputBuilder = new StringBuilder();
                var errorBuilder = new StringBuilder();

                process.OutputDataReceived += (sender, e) => { if (e.Data != null) outputBuilder.AppendLine(e.Data); };
                process.ErrorDataReceived += (sender, e) => { if (e.Data != null) errorBuilder.AppendLine(e.Data); };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                if (!process.WaitForExit(timeoutMs))
                {
                    try { process.Kill(); } catch { }
                    throw new TimeoutException($"Command '{command}' timed out after {timeoutMs}ms.");
                }

                // Ensure async read operations complete
                process.WaitForExit();

                string output = outputBuilder.ToString();
                string error = errorBuilder.ToString();

                if (process.ExitCode != 0 && !string.IsNullOrWhiteSpace(error))
                {
                    logger?.LogError("Command failed: {Command}, Exit Code: {ExitCode}, Error: {Error}",
                        command, process.ExitCode, error);
                    throw new InvalidOperationException($"Command failed with exit code {process.ExitCode}: {error}");
                }

                logger?.LogDebug("Command executed: {Command}, Output: {Output}", command, output.Trim());
                return output;
            }
        }

        /// <summary>
        /// Executes a shell command and returns whether it succeeded.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="output">When this method returns, contains the command output if successful; otherwise, null.</param>
        /// <param name="logger">Optional logger for diagnostic output.</param>
        /// <param name="shell">The shell to use. If null, defaults to cmd.exe on Windows or /bin/sh on Unix.</param>
        /// <param name="timeoutMs">The timeout in milliseconds. Default is 5000ms.</param>
        /// <returns>true if the command executed successfully; otherwise, false.</returns>
        public static bool TryExecute(string command, out string output, ILogger logger = null, string shell = null, int timeoutMs = 5000)
        {
            output = null;
            try
            {
                output = Execute(command, logger, shell, timeoutMs);
                return true;
            }
            catch (Exception ex)
            {
                logger?.LogDebug(ex, "Command execution failed: {Command}", command);
                return false;
            }
        }
    }
}