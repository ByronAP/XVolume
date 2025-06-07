using System;
using Microsoft.Extensions.Logging;

namespace XVolume.Common
{
    /// <summary>
    /// Simple console logger implementation for fallback when no ILogger is provided.
    /// </summary>
    internal class ConsoleLogger : ILogger
    {
        /// <inheritdoc/>
        public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

        /// <inheritdoc/>
        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

        /// <inheritdoc/>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var message = formatter(state, exception);

            Console.WriteLine($"[{timestamp}] [{logLevel}] {message}");

            if (exception != null)
            {
                Console.WriteLine($"Exception: {exception}");
            }
        }

        /// <summary>
        /// Represents a null scope that does nothing when disposed.
        /// </summary>
        private class NullScope : IDisposable
        {
            /// <summary>
            /// Gets the singleton instance of <see cref="NullScope"/>.
            /// </summary>
            public static NullScope Instance { get; } = new NullScope();

            /// <inheritdoc/>
            public void Dispose() { }
        }
    }
}
