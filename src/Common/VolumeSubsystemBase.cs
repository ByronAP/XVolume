using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using XVolume.Abstractions;

namespace XVolume.Common
{
    /// <summary>
    /// Base class for volume subsystem implementations.
    /// </summary>
    internal abstract class VolumeSubsystemBase : IVolumeSubsystem
    {
        /// <summary>
        /// The logger instance for diagnostic output.
        /// </summary>
        protected readonly ILogger Logger;

        private readonly SemaphoreSlim _volumeLock = new SemaphoreSlim(1, 1);
        private CancellationTokenSource _smoothTransitionCts;

        /// <summary>
        /// Initializes a new instance of the <see cref="VolumeSubsystemBase"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <exception cref="ArgumentNullException">Thrown when logger is null.</exception>
        protected VolumeSubsystemBase(ILogger logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public abstract int Volume { get; set; }

        /// <inheritdoc/>
        public abstract bool IsMuted { get; }

        /// <inheritdoc/>
        public abstract string Name { get; }

        /// <inheritdoc/>
        public virtual string CurrentDevice => null;

        /// <inheritdoc/>
        public abstract void Mute();

        /// <inheritdoc/>
        public abstract void Unmute();

        /// <inheritdoc/>
        public virtual void ToggleMute()
        {
            if (IsMuted)
                Unmute();
            else
                Mute();
        }

        /// <inheritdoc/>
        public virtual void IncrementVolume(int percentage)
        {
            if (percentage <= 0)
                throw new ArgumentOutOfRangeException(nameof(percentage), "Percentage must be positive.");

            Volume = Math.Min(100, Volume + percentage);
        }

        /// <inheritdoc/>
        public virtual void DecrementVolume(int percentage)
        {
            if (percentage <= 0)
                throw new ArgumentOutOfRangeException(nameof(percentage), "Percentage must be positive.");

            Volume = Math.Max(0, Volume - percentage);
        }

        /// <inheritdoc/>
        public virtual async Task SetVolumeSmooth(int targetVolume, int durationMs = 500, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (targetVolume < 0 || targetVolume > 100)
                throw new ArgumentOutOfRangeException(nameof(targetVolume), "Volume must be between 0 and 100.");

            if (durationMs <= 0)
                throw new ArgumentOutOfRangeException(nameof(durationMs), "Duration must be positive.");

            // Cancel any existing smooth transition
            if (_smoothTransitionCts != null)
            {
                _smoothTransitionCts.Cancel();
                _smoothTransitionCts.Dispose();
            }

            _smoothTransitionCts = new CancellationTokenSource();

            // Link the cancellation tokens
            using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _smoothTransitionCts.Token))
            {
                await _volumeLock.WaitAsync(linkedCts.Token).ConfigureAwait(false);
                try
                {
                    int startVolume = Volume;
                    int volumeDiff = targetVolume - startVolume;

                    if (volumeDiff == 0) return;

                    const int steps = 20;
                    int stepDuration = durationMs / steps;

                    for (int i = 1; i <= steps; i++)
                    {
                        if (linkedCts.Token.IsCancellationRequested)
                            break;

                        double progress = (double)i / steps;
                        // Use easing function for smooth transition
                        double easedProgress = 1 - Math.Pow(1 - progress, 3);
                        int newVolume = startVolume + (int)(volumeDiff * easedProgress);

                        Volume = newVolume;
                        await Task.Delay(stepDuration, linkedCts.Token).ConfigureAwait(false);
                    }

                    // Ensure we reach the target volume
                    if (!linkedCts.Token.IsCancellationRequested)
                        Volume = targetVolume;
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancelled
                }
                finally
                {
                    _volumeLock.Release();
                }
            }
        }

        /// <summary>
        /// Executes a shell command and returns the output.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="shell">The shell to use. If null, defaults to cmd.exe on Windows or /bin/sh on Unix.</param>
        /// <param name="timeoutMs">The timeout in milliseconds.</param>
        /// <returns>The command output.</returns>
        /// <exception cref="TimeoutException">Thrown when the command times out.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the command fails.</exception>
        protected string ExecuteCommand(string command, string shell = null, int timeoutMs = 5000)
        {
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
                    Logger.LogError("Command failed: {Command}, Exit Code: {ExitCode}, Error: {Error}",
                        command, process.ExitCode, error);
                    throw new InvalidOperationException($"Command failed with exit code {process.ExitCode}: {error}");
                }

                Logger.LogDebug("Command executed: {Command}, Output: {Output}", command, output.Trim());
                return output;
            }
        }

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="VolumeSubsystemBase"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_smoothTransitionCts != null)
                {
                    _smoothTransitionCts.Cancel();
                    _smoothTransitionCts.Dispose();
                    _smoothTransitionCts = null;
                }
                _volumeLock?.Dispose();
            }
        }
    }
}