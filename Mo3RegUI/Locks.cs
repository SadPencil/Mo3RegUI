namespace Mo3RegUI
{
    public static class Locks
    {
        public static object RA2MO_INI = new();
        public static object ClientDefinitions_INI = new();

        // Guards the files of the CnC-DDraw deployment: Resources\cnc-cdraw.dll and
        // Resources\cnc-cdraw.ini (the sources) plus ddraw.dll and ddraw.ini in the game
        // directory (the destinations). A single lock covers the whole set, so taking it once
        // is enough and no acquisition order is needed.
        public static object CnC_DDrawDeployment = new();
    }
}
