﻿namespace Mo3RegUI
{

    public partial class NativeMethods
    {
        /// Return Type: int
        ///nIndex: int
        [System.Runtime.InteropServices.DllImportAttribute("user32.dll", EntryPoint = "GetSystemMetrics")]
        public static extern int GetSystemMetrics(int nIndex);
    }

    public partial class NativeConstants
    {
        /// SM_CXSCREEN -> 0
        public const int SM_CXSCREEN = 0;
    }

    public partial class NativeConstants
    {
        /// SM_CYSCREEN -> 1
        public const int SM_CYSCREEN = 1;
    }

    public partial class NativeMethods
    {

        /// Return Type: UINT->unsigned int
        [System.Runtime.InteropServices.DllImportAttribute("kernel32.dll", EntryPoint = "GetACP")]
        public static extern uint GetACP();

    }

    public partial class NativeMethods
    {

        /// Return Type: UINT->unsigned int
        [System.Runtime.InteropServices.DllImportAttribute("winmm.dll", EntryPoint = "waveOutGetNumDevs")]
        public static extern uint waveOutGetNumDevs();

    }

}
