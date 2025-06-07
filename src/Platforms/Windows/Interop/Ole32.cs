using System;
using System.Runtime.InteropServices;

namespace XVolume.Platforms.Windows.Interop
{
    /// <summary>
    /// Provides P/Invoke declarations for Ole32.dll functions.
    /// </summary>
    internal static class Ole32
    {
        /// <summary>
        /// Creates an instance of a COM object.
        /// </summary>
        /// <param name="rclsid">The CLSID of the object to create.</param>
        /// <param name="pUnkOuter">Pointer to the controlling IUnknown interface. Pass IntPtr.Zero if not using aggregation.</param>
        /// <param name="dwClsContext">Context in which the code that manages the newly created object will run.</param>
        /// <param name="riid">The IID of the interface to retrieve.</param>
        /// <param name="ppv">Receives the interface pointer requested in riid.</param>
        /// <returns>An HRESULT indicating success or failure.</returns>
        [DllImport("ole32.dll")]
        public static extern int CoCreateInstance(
            ref Guid rclsid,
            IntPtr pUnkOuter,
            uint dwClsContext,
            ref Guid riid,
            out IntPtr ppv);
    }
}