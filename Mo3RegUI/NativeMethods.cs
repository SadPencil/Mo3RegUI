using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Mo3RegUI
{
    static class NativeMethods
    {
        [DllImport("gdi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr CreateDC(string lpszDriver, string lpszDeviceName, string lpszOutput, IntPtr devMode);
        
        [DllImport("gdi32.dll", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
        public static extern int GetDeviceCaps(IntPtr hDC, int nIndex);
    }

    public enum DeviceCap
    {
        HORZRES = 8,
        VERTRES = 10,
        DESKTOPVERTRES = 117,
        LOGPIXELSY = 90,
        DESKTOPHORZRES = 118

    }
}
