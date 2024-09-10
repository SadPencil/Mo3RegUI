namespace Mo3RegUI
{
    public partial class NativeMethods
    {
        /// Return Type: int
        ///nIndex: int
        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "GetSystemMetrics")]
        public static extern int GetSystemMetrics(int nIndex);

        /// Return Type: UINT->unsigned int
        [System.Runtime.InteropServices.DllImport("kernel32.dll", EntryPoint = "GetACP")]
        public static extern uint GetACP();

        /// Return Type: UINT->unsigned int
        [System.Runtime.InteropServices.DllImport("winmm.dll", EntryPoint = "waveOutGetNumDevs")]
        public static extern uint waveOutGetNumDevs();
    }
}
