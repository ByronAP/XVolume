using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Abstractions;

namespace XVolume.Tests.Helpers
{
    /// <summary>
    /// Provides helper methods and utilities for unit tests.
    /// </summary>
    internal static class TestHelpers
    {
        /// <summary>
        /// Creates a mock ILogger instance for testing.
        /// </summary>
        /// <typeparam name="T">The type to create the logger for.</typeparam>
        /// <returns>A mock ILogger instance.</returns>
        public static ILogger<T> CreateMockLogger<T>()
        {
            return new Mock<ILogger<T>>().Object;
        }

        /// <summary>
        /// Creates an ILogger instance that writes to xUnit test output.
        /// </summary>
        /// <typeparam name="T">The type to create the logger for.</typeparam>
        /// <param name="testOutput">The xUnit test output helper.</param>
        /// <returns>An ILogger instance that writes to test output.</returns>
        public static ILogger<T> CreateTestLogger<T>(ITestOutputHelper testOutput)
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddProvider(new XUnitLoggerProvider(testOutput));
                builder.SetMinimumLevel(LogLevel.Debug);
            });
            return loggerFactory.CreateLogger<T>();
        }

        /// <summary>
        /// Creates an ILogger instance that writes to xUnit test output.
        /// </summary>
        /// <param name="testOutput">The xUnit test output helper.</param>
        /// <param name="categoryName">The category name for the logger.</param>
        /// <returns>An ILogger instance that writes to test output.</returns>
        public static ILogger CreateTestLogger(ITestOutputHelper testOutput, string categoryName)
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddProvider(new XUnitLoggerProvider(testOutput));
                builder.SetMinimumLevel(LogLevel.Debug);
            });
            return loggerFactory.CreateLogger(categoryName);
        }

        /// <summary>
        /// Determines if the current platform matches the specified platform.
        /// </summary>
        /// <param name="platform">The platform to check.</param>
        /// <returns>true if running on the specified platform; otherwise, false.</returns>
        public static bool IsRunningOnPlatform(TestPlatform platform)
        {
            switch (platform)
            {
                case TestPlatform.Windows:
                    return System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
                        System.Runtime.InteropServices.OSPlatform.Windows);
                case TestPlatform.Linux:
                    return System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
                        System.Runtime.InteropServices.OSPlatform.Linux);
                case TestPlatform.MacOS:
                    return System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
                        System.Runtime.InteropServices.OSPlatform.OSX);
                default:
                    return false;
            }
        }

        /// <summary>
        /// Skips the test if not running on the specified platform.
        /// </summary>
        /// <param name="requiredPlatform">The required platform for the test.</param>
        /// <exception cref="SkipTestException">Thrown to skip the test if not on the required platform.</exception>
        public static void RequirePlatform(TestPlatform requiredPlatform)
        {
            if (!IsRunningOnPlatform(requiredPlatform))
            {
                throw new SkipTestException($"Test requires {requiredPlatform} platform.");
            }
        }
    }

    /// <summary>
    /// Represents the test platforms.
    /// </summary>
    internal enum TestPlatform
    {
        Windows,
        Linux,
        MacOS
    }

    /// <summary>
    /// Exception thrown to skip a test.
    /// </summary>
    internal class SkipTestException : Exception
    {
        public SkipTestException(string message) : base(message) { }
    }

    /// <summary>
    /// Logger provider that writes to xUnit test output.
    /// </summary>
    internal class XUnitLoggerProvider : ILoggerProvider
    {
        private readonly ITestOutputHelper _testOutput;

        public XUnitLoggerProvider(ITestOutputHelper testOutput)
        {
            _testOutput = testOutput;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new XUnitLogger(_testOutput, categoryName);
        }

        public void Dispose() { }
    }

    /// <summary>
    /// Logger that writes to xUnit test output.
    /// </summary>
    internal class XUnitLogger : ILogger
    {
        private readonly ITestOutputHelper _testOutput;
        private readonly string _categoryName;

        public XUnitLogger(ITestOutputHelper testOutput, string categoryName)
        {
            _testOutput = testOutput;
            _categoryName = categoryName;
        }

        public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            var message = formatter(state, exception);
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");

            _testOutput.WriteLine($"[{timestamp}] [{logLevel}] [{_categoryName}] {message}");

            if (exception != null)
            {
                _testOutput.WriteLine($"Exception: {exception}");
            }
        }

        private class NullScope : IDisposable
        {
            public static NullScope Instance { get; } = new NullScope();
            public void Dispose() { }
        }
    }
}