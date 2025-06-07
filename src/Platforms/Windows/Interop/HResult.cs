using System.Runtime.InteropServices;

namespace XVolume.Platforms.Windows.Interop
{
    /// <summary>
    /// Provides HRESULT error code handling for Windows COM interop.
    /// </summary>
    internal static class HResult
    {
        /// <summary>
        /// The operation completed successfully.
        /// </summary>
        public const int S_OK = 0;

        /// <summary>
        /// Unspecified failure.
        /// </summary>
        public const int E_FAIL = unchecked((int)0x80004005);

        /// <summary>
        /// One or more arguments are invalid.
        /// </summary>
        public const int E_INVALIDARG = unchecked((int)0x80070057);

        /// <summary>
        /// Not implemented.
        /// </summary>
        public const int E_NOTIMPL = unchecked((int)0x80004001);

        /// <summary>
        /// Invalid pointer.
        /// </summary>
        public const int E_POINTER = unchecked((int)0x80004003);

        /// <summary>
        /// Unexpected failure.
        /// </summary>
        public const int E_UNEXPECTED = unchecked((int)0x8000FFFF);

        /// <summary>
        /// Throws a <see cref="COMException"/> if the HRESULT indicates failure.
        /// </summary>
        /// <param name="hr">The HRESULT value to check.</param>
        /// <exception cref="COMException">Thrown when the HRESULT indicates failure (negative value).</exception>
        public static void ThrowIfFailed(this int hr)
        {
            if (hr < 0)
            {
                throw new COMException(GetErrorMessage(hr), hr);
            }
        }

        /// <summary>
        /// Gets a descriptive error message for the specified HRESULT.
        /// </summary>
        /// <param name="hr">The HRESULT value.</param>
        /// <returns>A descriptive error message.</returns>
        private static string GetErrorMessage(int hr)
        {
            switch (hr)
            {
                case E_FAIL:
                    return "Unspecified failure";
                case E_INVALIDARG:
                    return "Invalid argument";
                case E_NOTIMPL:
                    return "Not implemented";
                case E_POINTER:
                    return "Invalid pointer";
                case E_UNEXPECTED:
                    return "Unexpected failure";
                default:
                    return $"Unknown error (HRESULT: 0x{hr:X8})";
            }
        }
    }
}