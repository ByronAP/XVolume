using XVolume.Factory;
using XVolume.Sample.Abstractions;
using XVolume.Sample.Helpers;

namespace XVolume.Sample.Commands
{
    /// <summary>
    /// Command for testing all volume control functionality.
    /// </summary>
    public class TestCommand : ICommand
    {
        /// <inheritdoc/>
        public string Name => "test";

        /// <inheritdoc/>
        public string Description => "Run comprehensive volume control tests";

        /// <inheritdoc/>
        public string Usage => "test [--quick]";

        /// <inheritdoc/>
        public string[] Aliases => new[] { "t", "check" };

        /// <inheritdoc/>
        public bool ValidateArgs(string[] args)
        {
            if (args.Length == 0)
                return true;

            return args.Length == 1 && args[0] == "--quick";
        }

        /// <inheritdoc/>
        public int Execute(string[] args)
        {
            var quickMode = args.Length > 0 && args[0] == "--quick";

            ConsoleHelper.WriteHeader("XVolume Test Suite");

            if (!ConsoleHelper.Confirm("This will change your system volume. Continue?", true))
            {
                ConsoleHelper.WriteWarning("Test cancelled by user");
                return 0;
            }

            using (var volumeSystem = VolumeSubsystemFactory.Create())
            {
                ConsoleHelper.WriteInfo($"Testing with: {volumeSystem.Name}");
                Console.WriteLine();

                var originalVolume = volumeSystem.Volume;
                var originalMute = volumeSystem.IsMuted;
                var failureCount = 0;

                try
                {
                    // Test 1: Get current volume
                    failureCount += RunTest("Get current volume", () =>
                    {
                        var volume = volumeSystem.Volume;
                        ConsoleHelper.WriteInfo($"Current volume: {volume}%");
                        return volume >= 0 && volume <= 100;
                    });

                    // Test 2: Get mute state
                    failureCount += RunTest("Get mute state", () =>
                    {
                        var isMuted = volumeSystem.IsMuted;
                        ConsoleHelper.WriteInfo($"Mute state: {(isMuted ? "Muted" : "Unmuted")}");
                        return true;
                    });

                    // Test 3: Set volume
                    failureCount += RunTest("Set volume to 50%", () =>
                    {
                        volumeSystem.Volume = 50;
                        Thread.Sleep(500);
                        return volumeSystem.Volume == 50;
                    });

                    // Test 4: Increment volume
                    failureCount += RunTest("Increment volume by 10%", () =>
                    {
                        var before = volumeSystem.Volume;
                        volumeSystem.IncrementVolume(10);
                        Thread.Sleep(500);
                        var after = volumeSystem.Volume;
                        return after == Math.Min(100, before + 10);
                    });

                    // Test 5: Decrement volume
                    failureCount += RunTest("Decrement volume by 20%", () =>
                    {
                        var before = volumeSystem.Volume;
                        volumeSystem.DecrementVolume(20);
                        Thread.Sleep(500);
                        var after = volumeSystem.Volume;
                        return after == Math.Max(0, before - 20);
                    });

                    // Test 6: Mute
                    failureCount += RunTest("Mute audio", () =>
                    {
                        volumeSystem.Mute();
                        Thread.Sleep(500);
                        return volumeSystem.IsMuted;
                    });

                    // Test 7: Unmute
                    failureCount += RunTest("Unmute audio", () =>
                    {
                        volumeSystem.Unmute();
                        Thread.Sleep(500);
                        return !volumeSystem.IsMuted;
                    });

                    // Test 8: Toggle mute
                    failureCount += RunTest("Toggle mute", () =>
                    {
                        var before = volumeSystem.IsMuted;
                        volumeSystem.ToggleMute();
                        Thread.Sleep(500);
                        return volumeSystem.IsMuted != before;
                    });

                    // Test 9: Boundary test - minimum volume
                    failureCount += RunTest("Set volume to 0%", () =>
                    {
                        volumeSystem.Volume = 0;
                        Thread.Sleep(500);
                        return volumeSystem.Volume == 0;
                    });

                    // Test 10: Boundary test - maximum volume
                    failureCount += RunTest("Set volume to 100%", () =>
                    {
                        volumeSystem.Volume = 100;
                        Thread.Sleep(500);
                        return volumeSystem.Volume == 100;
                    });

                    if (!quickMode)
                    {
                        // Test 11: Smooth volume transition
                        failureCount += RunTest("Smooth volume transition", () =>
                        {
                            ConsoleHelper.WriteInfo("Transitioning from 20% to 80%...");
                            volumeSystem.Volume = 20;
                            Thread.Sleep(500);

                            var task = volumeSystem.SetVolumeSmooth(80, 3000);
                            task.Wait();

                            return Math.Abs(volumeSystem.Volume - 80) <= 1;
                        });

                        // Test 12: Device name
                        failureCount += RunTest("Get device name", () =>
                        {
                            var device = volumeSystem.CurrentDevice;
                            if (string.IsNullOrEmpty(device))
                            {
                                ConsoleHelper.WriteInfo("Device name not available");
                            }
                            else
                            {
                                ConsoleHelper.WriteInfo($"Device: {device}");
                            }
                            return true;
                        });
                    }

                    // Summary
                    Console.WriteLine();
                    ConsoleHelper.WriteSection("Test Summary");
                    var totalTests = quickMode ? 10 : 12;
                    var passedTests = totalTests - failureCount;

                    ConsoleHelper.WriteKeyValue("Total Tests", totalTests.ToString());
                    ConsoleHelper.WriteKeyValue("Passed", passedTests.ToString());
                    ConsoleHelper.WriteKeyValue("Failed", failureCount.ToString());

                    if (failureCount == 0)
                    {
                        ConsoleHelper.WriteSuccess("All tests passed!");
                    }
                    else
                    {
                        ConsoleHelper.WriteError($"{failureCount} test(s) failed");
                    }

                    return failureCount > 0 ? 1 : 0;
                }
                finally
                {
                    // Restore original state
                    ConsoleHelper.WriteInfo("Restoring original audio state...");
                    volumeSystem.Volume = originalVolume;
                    if (originalMute)
                        volumeSystem.Mute();
                    else
                        volumeSystem.Unmute();

                    ConsoleHelper.WriteSuccess($"Restored: Volume={originalVolume}%, Muted={originalMute}");
                }
            }
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
            Console.WriteLine("  --quick    Run quick tests only (skip slow tests)");
            Console.WriteLine();
            Console.WriteLine("Tests performed:");
            Console.WriteLine("  - Get current volume");
            Console.WriteLine("  - Get mute state");
            Console.WriteLine("  - Set volume to specific values");
            Console.WriteLine("  - Increment/decrement volume");
            Console.WriteLine("  - Mute/unmute operations");
            Console.WriteLine("  - Toggle mute");
            Console.WriteLine("  - Boundary tests (0% and 100%)");
            Console.WriteLine("  - Smooth volume transitions (unless --quick)");
            Console.WriteLine("  - Device name retrieval (unless --quick)");
            Console.WriteLine();
            Console.WriteLine("The test will restore your original volume settings when complete.");
            Console.WriteLine();
            Console.WriteLine($"Aliases: {string.Join(", ", Aliases)}");
        }

        private int RunTest(string testName, Func<bool> test)
        {
            Console.Write($"Testing: {testName}... ");

            try
            {
                var result = test();
                if (result)
                {
                    ConsoleHelper.WriteSuccess("PASSED");
                    return 0;
                }
                else
                {
                    ConsoleHelper.WriteError("FAILED");
                    return 1;
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError($"FAILED - {ex.Message}");
                return 1;
            }
        }
    }
}
