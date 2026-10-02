using System.Runtime.InteropServices;

namespace Mo3RegUI
{
    /// <summary>
    /// Helpers for the Zone.Identifier alternate data stream that Windows adds to files
    /// downloaded from the Internet (the "mark of the web").
    /// </summary>
    public static class ZoneIdentifier
    {
        /// <summary>
        /// Removes the Zone.Identifier alternate data stream from a file.
        /// </summary>
        /// <returns>0 on success, otherwise the Win32 error code.</returns>
        public static int Remove(string filePath)
        {
            string zoneIdentifier = filePath + ":Zone.Identifier";
            bool success = NativeMethods.DeleteFile(zoneIdentifier);
            return success ? 0 : Marshal.GetLastWin32Error();
        }
    }
}
