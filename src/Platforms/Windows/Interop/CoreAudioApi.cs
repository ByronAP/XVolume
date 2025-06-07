using System;
using System.Runtime.InteropServices;

namespace XVolume.Platforms.Windows.Interop
{
    /// <summary>
    /// Specifies the data flow direction for an audio endpoint.
    /// </summary>
    internal enum EDataFlow
    {
        /// <summary>
        /// Audio rendering stream. Audio data flows from the application to the audio endpoint device, which renders the stream.
        /// </summary>
        eRender = 0,

        /// <summary>
        /// Audio capture stream. Audio data flows from the audio endpoint device that captures the stream to the application.
        /// </summary>
        eCapture = 1,

        /// <summary>
        /// Audio rendering or capture stream.
        /// </summary>
        eAll = 2
    }

    /// <summary>
    /// Defines the role that the system has assigned to an audio endpoint device.
    /// </summary>
    internal enum ERole
    {
        /// <summary>
        /// Games, system notification sounds, and voice commands.
        /// </summary>
        eConsole = 0,

        /// <summary>
        /// Music, movies, narration, and live music recording.
        /// </summary>
        eMultimedia = 1,

        /// <summary>
        /// Voice communications (talking to another person).
        /// </summary>
        eCommunications = 2
    }

    /// <summary>
    /// Enumerates audio endpoint devices.
    /// </summary>
    [ComImport]
    [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMDeviceEnumerator
    {
        /// <summary>
        /// Not implemented.
        /// </summary>
        void NotImplemented1(); // EnumAudioEndpoints

        /// <summary>
        /// Gets the default audio endpoint for the specified data flow direction and role.
        /// </summary>
        /// <param name="dataFlow">The data flow direction.</param>
        /// <param name="role">The device role.</param>
        /// <param name="ppDevice">Receives the default audio endpoint device.</param>
        /// <returns>An HRESULT indicating success or failure.</returns>
        [PreserveSig]
        int GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role, out IMMDevice ppDevice);

        /// <summary>
        /// Not implemented.
        /// </summary>
        void NotImplemented2(); // GetDevice

        /// <summary>
        /// Not implemented.
        /// </summary>
        void NotImplemented3(); // RegisterEndpointNotificationCallback

        /// <summary>
        /// Not implemented.
        /// </summary>
        void NotImplemented4(); // UnregisterEndpointNotificationCallback
    }

    /// <summary>
    /// Represents an audio endpoint device.
    /// </summary>
    /// <summary>
    /// Represents an audio endpoint device.
    /// </summary>
    [ComImport]
    [Guid("D666063F-1587-4E43-81F1-B948E807363F")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMDevice
    {
        /// <summary>
        /// Activates a COM object for the audio endpoint device.
        /// </summary>
        /// <param name="iid">The interface identifier.</param>
        /// <param name="dwClsCtx">The execution context in which the object is to be run.</param>
        /// <param name="pActivationParams">Activation parameters. Pass IntPtr.Zero for default activation.</param>
        /// <param name="ppInterface">Receives the interface pointer.</param>
        /// <returns>An HRESULT indicating success or failure.</returns>
        [PreserveSig]
        int Activate(ref Guid iid, uint dwClsCtx, IntPtr pActivationParams, out IntPtr ppInterface);

        /// <summary>
        /// Not implemented.
        /// </summary>
        void NotImplemented1(); // OpenPropertyStore

        /// <summary>
        /// Not implemented.
        /// </summary>
        void NotImplemented2(); // GetId

        /// <summary>
        /// Not implemented.
        /// </summary>
        void NotImplemented3(); // GetState
    }

    /// <summary>
    /// Represents the volume controls on the audio stream to or from an audio endpoint device.
    /// </summary>
    [ComImport]
    [Guid("5CDF2C82-841E-4546-9722-0CF74078229A")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioEndpointVolume
    {
        // RegisterControlChangeNotify
        [PreserveSig]
        int RegisterControlChangeNotify(IntPtr pNotify);

        // UnregisterControlChangeNotify
        [PreserveSig]
        int UnregisterControlChangeNotify(IntPtr pNotify);

        // GetChannelCount
        [PreserveSig]
        int GetChannelCount(out uint pnChannelCount);

        // SetMasterVolumeLevel
        [PreserveSig]
        int SetMasterVolumeLevel(float fLevelDB, ref Guid pguidEventContext);

        // SetMasterVolumeLevelScalar
        [PreserveSig]
        int SetMasterVolumeLevelScalar(float fLevel, ref Guid pguidEventContext);

        // GetMasterVolumeLevel
        [PreserveSig]
        int GetMasterVolumeLevel(out float pfLevelDB);

        // GetMasterVolumeLevelScalar
        [PreserveSig]
        int GetMasterVolumeLevelScalar(out float pfLevel);

        // SetChannelVolumeLevel
        [PreserveSig]
        int SetChannelVolumeLevel(uint nChannel, float fLevelDB, ref Guid pguidEventContext);

        // SetChannelVolumeLevelScalar
        [PreserveSig]
        int SetChannelVolumeLevelScalar(uint nChannel, float fLevel, ref Guid pguidEventContext);

        // GetChannelVolumeLevel
        [PreserveSig]
        int GetChannelVolumeLevel(uint nChannel, out float pfLevelDB);

        // GetChannelVolumeLevelScalar
        [PreserveSig]
        int GetChannelVolumeLevelScalar(uint nChannel, out float pfLevel);

        // SetMute
        [PreserveSig]
        int SetMute([MarshalAs(UnmanagedType.Bool)] bool bMute, ref Guid pguidEventContext);

        // GetMute
        [PreserveSig]
        int GetMute(out int pbMute);

        // GetVolumeStepInfo
        [PreserveSig]
        int GetVolumeStepInfo(out uint pnStep, out uint pnStepCount);

        // VolumeStepUp
        [PreserveSig]
        int VolumeStepUp(ref Guid pguidEventContext);

        // VolumeStepDown
        [PreserveSig]
        int VolumeStepDown(ref Guid pguidEventContext);

        // QueryHardwareSupport
        [PreserveSig]
        int QueryHardwareSupport(out uint pdwHardwareSupportMask);

        // GetVolumeRange
        [PreserveSig]
        int GetVolumeRange(out float pflVolumeMindB, out float pflVolumeMaxdB, out float pflVolumeIncrementdB);
    }
}