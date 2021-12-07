using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Mo3RegUI
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

}
