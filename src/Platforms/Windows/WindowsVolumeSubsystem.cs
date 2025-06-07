using System;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using XVolume.Common;
using XVolume.Platforms.Windows.Interop;

namespace XVolume.Platforms.Windows
{
    /// <summary>
    /// Windows Core Audio volume subsystem implementation using COM interop.
    /// </summary>
    internal class WindowsVolumeSubsystem : VolumeSubsystemBase
    {
        private readonly IAudioEndpointVolume _endpointVolume;
        private bool _disposed;
        private static readonly object _comLock = new object();
        private static int _comInitCount = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsVolumeSubsystem"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <exception cref="InvalidOperationException">Thrown when Windows Core Audio initialization fails.</exception>
        public WindowsVolumeSubsystem(ILogger logger) : base(logger)
        {
            try
            {
                InitializeCom();
                _endpointVolume = InitializeCoreAudio();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to initialize Windows Core Audio.");
                throw new InvalidOperationException("Unable to initialize Windows Core Audio.", ex);
            }
        }

        /// <inheritdoc/>
        public override string Name => "Windows Core Audio";

        /// <summary>
        /// Initializes COM for the current thread if not already initialized.
        /// </summary>
        private static void InitializeCom()
        {
            lock (_comLock)
            {
                if (_comInitCount == 0)
                {
                    int hr = CoInitializeEx(IntPtr.Zero, COINIT.MULTITHREADED);
                    if (hr < 0 && hr != -2147417850) // RPC_E_CHANGED_MODE
                    {
                        throw new COMException("Failed to initialize COM", hr);
                    }
                }
                _comInitCount++;
            }
        }

        /// <summary>
        /// Uninitializes COM for the current thread.
        /// </summary>
        private static void UninitializeCom()
        {
            lock (_comLock)
            {
                _comInitCount--;
                if (_comInitCount == 0)
                {
                    CoUninitialize();
                }
            }
        }

        [DllImport("ole32.dll")]
        private static extern int CoInitializeEx(IntPtr pvReserved, COINIT dwCoInit);

        [DllImport("ole32.dll")]
        private static extern void CoUninitialize();

        private enum COINIT : uint
        {
            MULTITHREADED = 0x0,
            APARTMENTTHREADED = 0x2,
        }

        /// <summary>
        /// Initializes the Windows Core Audio system and gets the default audio endpoint volume interface.
        /// </summary>
        /// <returns>The audio endpoint volume interface.</returns>
        private static IAudioEndpointVolume InitializeCoreAudio()
        {
            Guid clsid = new Guid("BCDE0395-E52F-467C-8E3D-C4579291692E"); // MMDeviceEnumerator
            Guid iid = new Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"); // IMMDeviceEnumerator

            IntPtr enumeratorPtr;
            Ole32.CoCreateInstance(ref clsid, IntPtr.Zero, 1, ref iid, out enumeratorPtr).ThrowIfFailed();
            var enumerator = (IMMDeviceEnumerator)Marshal.GetObjectForIUnknown(enumeratorPtr);

            IMMDevice device;
            enumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out device).ThrowIfFailed();

            Guid iidEndpointVolume = new Guid("5CDF2C82-841E-4546-9722-0CF74078229A"); // IAudioEndpointVolume
            IntPtr endpointVolumePtr;
            device.Activate(ref iidEndpointVolume, 1, IntPtr.Zero, out endpointVolumePtr).ThrowIfFailed();
            var endpointVolume = (IAudioEndpointVolume)Marshal.GetObjectForIUnknown(endpointVolumePtr);

            Marshal.ReleaseComObject(device);
            Marshal.ReleaseComObject(enumerator);

            return endpointVolume;
        }

        /// <inheritdoc/>
        public override int Volume
        {
            get
            {
                float level;
                _endpointVolume.GetMasterVolumeLevelScalar(out level).ThrowIfFailed();
                return (int)Math.Round(level * 100);
            }
            set
            {
                if (value < 0 || value > 100)
                    throw new ArgumentOutOfRangeException(nameof(value), "Volume must be between 0 and 100.");

                float normalizedVolume = (float)value / 100.0f;
                var eventContext = Guid.Empty;
                _endpointVolume.SetMasterVolumeLevelScalar(normalizedVolume, ref eventContext).ThrowIfFailed();
            }
        }

        /// <inheritdoc/>
        public override bool IsMuted
        {
            get
            {
                int mute;
                _endpointVolume.GetMute(out mute).ThrowIfFailed();
                return mute != 0;
            }
        }

        /// <inheritdoc/>
        public override void Mute()
        {
            var eventContext = Guid.Empty;
            _endpointVolume.SetMute(true, ref eventContext).ThrowIfFailed();
        }

        /// <inheritdoc/>
        public override void Unmute()
        {
            var eventContext = Guid.Empty;
            _endpointVolume.SetMute(false, ref eventContext).ThrowIfFailed();
        }

        /// <inheritdoc/>
        public override void IncrementVolume(int percentage)
        {
            if (percentage <= 0)
                throw new ArgumentOutOfRangeException(nameof(percentage), "Percentage must be positive.");

            float current;
            _endpointVolume.GetMasterVolumeLevelScalar(out current).ThrowIfFailed();
            float newVolume = Math.Min(1.0f, current + ((float)percentage / 100.0f));
            var eventContext = Guid.Empty;
            _endpointVolume.SetMasterVolumeLevelScalar(newVolume, ref eventContext).ThrowIfFailed();
        }

        /// <inheritdoc/>
        public override void DecrementVolume(int percentage)
        {
            if (percentage <= 0)
                throw new ArgumentOutOfRangeException(nameof(percentage), "Percentage must be positive.");

            float current;
            _endpointVolume.GetMasterVolumeLevelScalar(out current).ThrowIfFailed();
            float newVolume = Math.Max(0.0f, current - ((float)percentage / 100.0f));
            var eventContext = Guid.Empty;
            _endpointVolume.SetMasterVolumeLevelScalar(newVolume, ref eventContext).ThrowIfFailed();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (_endpointVolume != null)
                {
                    Marshal.ReleaseComObject(_endpointVolume);
                }

                UninitializeCom();
                _disposed = true;
            }
            base.Dispose(disposing);
        }
    }
}