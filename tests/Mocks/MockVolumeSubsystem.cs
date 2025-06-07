using Microsoft.Extensions.Logging;
using XVolume.Common;

namespace XVolume.Tests.Mocks
{
    /// <summary>
    /// Mock implementation of IVolumeSubsystem for testing.
    /// </summary>
    internal class MockVolumeSubsystem : VolumeSubsystemBase
    {
        private int _volume = 50;
        private bool _isMuted = false;
        private bool _throwOnVolumeGet = false;
        private bool _throwOnVolumeSet = false;
        private bool _throwOnMute = false;
        private bool _throwOnUnmute = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="MockVolumeSubsystem"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        public MockVolumeSubsystem(ILogger logger) : base(logger)
        {
        }

        /// <inheritdoc/>
        public override string Name => "Mock Volume Subsystem";

        /// <inheritdoc/>
        public override string CurrentDevice => "Mock Audio Device";

        /// <inheritdoc/>
        public override int Volume
        {
            get
            {
                if (_throwOnVolumeGet)
                    throw new InvalidOperationException("Mock exception on volume get");
                return _volume;
            }
            set
            {
                if (_throwOnVolumeSet)
                    throw new InvalidOperationException("Mock exception on volume set");

                if (value < 0 || value > 100)
                    throw new ArgumentOutOfRangeException(nameof(value), "Volume must be between 0 and 100.");

                _volume = value;
                VolumeChanged?.Invoke(this, value);
            }
        }

        /// <inheritdoc/>
        public override bool IsMuted => _isMuted;

        /// <inheritdoc/>
        public override void Mute()
        {
            if (_throwOnMute)
                throw new InvalidOperationException("Mock exception on mute");

            _isMuted = true;
            MuteStateChanged?.Invoke(this, true);
        }

        /// <inheritdoc/>
        public override void Unmute()
        {
            if (_throwOnUnmute)
                throw new InvalidOperationException("Mock exception on unmute");

            _isMuted = false;
            MuteStateChanged?.Invoke(this, false);
        }

        /// <summary>
        /// Event raised when volume changes.
        /// </summary>
        public event EventHandler<int> VolumeChanged;

        /// <summary>
        /// Event raised when mute state changes.
        /// </summary>
        public event EventHandler<bool> MuteStateChanged;

        /// <summary>
        /// Gets the number of times SetVolumeSmooth has been called.
        /// </summary>
        public int SetVolumeSmoothCallCount { get; private set; }

        /// <summary>
        /// Gets the last target volume passed to SetVolumeSmooth.
        /// </summary>
        public int LastSmoothTargetVolume { get; private set; }

        /// <summary>
        /// Gets the last duration passed to SetVolumeSmooth.
        /// </summary>
        public int LastSmoothDurationMs { get; private set; }

        /// <inheritdoc/>
        public override async Task SetVolumeSmooth(int targetVolume, int durationMs = 500, CancellationToken cancellationToken = default(CancellationToken))
        {
            SetVolumeSmoothCallCount++;
            LastSmoothTargetVolume = targetVolume;
            LastSmoothDurationMs = durationMs;

            // Simulate the smooth transition
            await base.SetVolumeSmooth(targetVolume, durationMs, cancellationToken);
        }

        /// <summary>
        /// Configures the mock to throw exceptions on specific operations.
        /// </summary>
        /// <param name="throwOnVolumeGet">Whether to throw on Volume get.</param>
        /// <param name="throwOnVolumeSet">Whether to throw on Volume set.</param>
        /// <param name="throwOnMute">Whether to throw on Mute().</param>
        /// <param name="throwOnUnmute">Whether to throw on Unmute().</param>
        public void ConfigureExceptions(
            bool throwOnVolumeGet = false,
            bool throwOnVolumeSet = false,
            bool throwOnMute = false,
            bool throwOnUnmute = false)
        {
            _throwOnVolumeGet = throwOnVolumeGet;
            _throwOnVolumeSet = throwOnVolumeSet;
            _throwOnMute = throwOnMute;
            _throwOnUnmute = throwOnUnmute;
        }

        /// <summary>
        /// Resets the mock to default state.
        /// </summary>
        public void Reset()
        {
            _volume = 50;
            _isMuted = false;
            _throwOnVolumeGet = false;
            _throwOnVolumeSet = false;
            _throwOnMute = false;
            _throwOnUnmute = false;
            SetVolumeSmoothCallCount = 0;
            LastSmoothTargetVolume = 0;
            LastSmoothDurationMs = 0;
        }
    }
}