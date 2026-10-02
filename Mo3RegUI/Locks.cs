namespace Mo3RegUI
{
    public static class Locks
    {
        public static object RA2MO_INI = new();
        public static object ClientDefinitions_INI = new();
        public static object CnC_DDraw_INI = new();
        public static object CnC_DDraw_DLL = new(); // Resources\cnc-cdraw.dll
        public static object DDraw_DLL = new(); // ddraw.dll deployed to the game directory
        public static object DDraw_INI = new(); // ddraw.ini deployed to the game directory
    }
}
