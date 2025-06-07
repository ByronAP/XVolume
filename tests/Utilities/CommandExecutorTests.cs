using Microsoft.Extensions.Logging;
using Xunit.Abstractions;
using XVolume.Utilities;
using XVolume.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace XVolume.Tests.Utilities
{
    /// <summary>
    /// Unit tests for the CommandExecutor class.
    /// </summary>
    public class CommandExecutorTests
    {
        private readonly ILogger _logger;

        public CommandExecutorTests(ITestOutputHelper output)
        {
            _logger = TestHelpers.CreateTestLogger(output, nameof(CommandExecutorTests));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Execute_WithNullCommand_ShouldThrow()
        {
            // Act
            Action act = () => CommandExecutor.Execute(null, _logger);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("command");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Execute_WithEmptyCommand_ShouldThrow()
        {
            // Act
            Action act = () => CommandExecutor.Execute(string.Empty, _logger);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("command");
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        public void Execute_WithValidCommand_ShouldReturnOutput()
        {
            // Arrange
            Skip.IfNot(TestHelpers.IsRunningOnPlatform(TestPlatform.Windows) ||
                      TestHelpers.IsRunningOnPlatform(TestPlatform.Linux) ||
                      TestHelpers.IsRunningOnPlatform(TestPlatform.MacOS));

            string command = TestHelpers.IsRunningOnPlatform(TestPlatform.Windows)
                ? "echo Hello"
                : "echo 'Hello'";

            // Act
            var result = CommandExecutor.Execute(command, _logger);

            // Assert
            result.Should().Contain("Hello");
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        public void Execute_WithInvalidCommand_ShouldThrow()
        {
            // Arrange
            Skip.IfNot(TestHelpers.IsRunningOnPlatform(TestPlatform.Windows) ||
                      TestHelpers.IsRunningOnPlatform(TestPlatform.Linux) ||
                      TestHelpers.IsRunningOnPlatform(TestPlatform.MacOS));

            string command = "invalidcommandthatdoesnotexist";

            // Act
            Action act = () => CommandExecutor.Execute(command, _logger);

            // Assert
            act.Should().Throw<InvalidOperationException>();
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        public void Execute_WithTimeout_ShouldThrowTimeoutException()
        {
            // Arrange
            Skip.IfNot(TestHelpers.IsRunningOnPlatform(TestPlatform.Windows) ||
                      TestHelpers.IsRunningOnPlatform(TestPlatform.Linux) ||
                      TestHelpers.IsRunningOnPlatform(TestPlatform.MacOS));

            string command = TestHelpers.IsRunningOnPlatform(TestPlatform.Windows)
                ? "ping -n 5 127.0.0.1"
                : "sleep 5";

            // Act
            Action act = () => CommandExecutor.Execute(command, _logger, timeoutMs: 100);

            // Assert
            act.Should().Throw<TimeoutException>()
                .WithMessage("*timed out after 100ms*");
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        public void TryExecute_WithValidCommand_ShouldReturnTrue()
        {
            // Arrange
            Skip.IfNot(TestHelpers.IsRunningOnPlatform(TestPlatform.Windows) ||
                      TestHelpers.IsRunningOnPlatform(TestPlatform.Linux) ||
                      TestHelpers.IsRunningOnPlatform(TestPlatform.MacOS));

            string command = TestHelpers.IsRunningOnPlatform(TestPlatform.Windows)
                ? "echo Test"
                : "echo 'Test'";

            // Act
            var result = CommandExecutor.TryExecute(command, out string output, _logger);

            // Assert
            result.Should().BeTrue();
            output.Should().Contain("Test");
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        public void TryExecute_WithInvalidCommand_ShouldReturnFalse()
        {
            // Arrange
            Skip.IfNot(TestHelpers.IsRunningOnPlatform(TestPlatform.Windows) ||
                      TestHelpers.IsRunningOnPlatform(TestPlatform.Linux) ||
                      TestHelpers.IsRunningOnPlatform(TestPlatform.MacOS));

            string command = "invalidcommandthatdoesnotexist";

            // Act
            var result = CommandExecutor.TryExecute(command, out string output, _logger);

            // Assert
            result.Should().BeFalse();
            output.Should().BeNull();
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        public void Execute_WithCustomShell_ShouldUseSpecifiedShell()
        {
            // Arrange
            Skip.IfNot(TestHelpers.IsRunningOnPlatform(TestPlatform.Linux) ||
                      TestHelpers.IsRunningOnPlatform(TestPlatform.MacOS));

            string command = "echo $0";
            string shell = "/bin/bash";

            // Act
            var result = CommandExecutor.Execute(command, _logger, shell);

            // Assert
            result.Should().Contain("bash");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Execute_WithoutLogger_ShouldNotThrow()
        {
            // Arrange
            string command = TestHelpers.IsRunningOnPlatform(TestPlatform.Windows)
                ? "echo Test"
                : "echo 'Test'";

            // Act
            Action act = () => CommandExecutor.Execute(command, null);

            // Assert
            act.Should().NotThrow();
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        [Trait("Category", "Windows")]
        public void Execute_OnWindows_ShouldUseCmdByDefault()
        {
            // Arrange
            Skip.IfNot(TestHelpers.IsRunningOnPlatform(TestPlatform.Windows));

            string command = "echo %COMSPEC%";

            // Act
            var result = CommandExecutor.Execute(command, _logger);

            // Assert
            result.Should().Contain("cmd.exe", "because Windows should use cmd.exe by default");
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        [Trait("Category", "Linux")]
        public void Execute_OnLinux_ShouldUseShByDefault()
        {
            // Arrange
            Skip.IfNot(TestHelpers.IsRunningOnPlatform(TestPlatform.Linux));

            string command = "echo $SHELL";

            // Act
            var result = CommandExecutor.Execute(command, _logger);

            // Assert
            result.Should().NotBeEmpty();
        }
    }
}