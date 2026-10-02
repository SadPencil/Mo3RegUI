namespace Mo3RegUI
{
    public static class Locks
    {
        public static object RA2MO_INI = new();
        public static object ClientDefinitions_INI = new();

        // Guards the files of the CnC-DDraw deployment: Resources\cnc-ddraw.dll and
        // Resources\cnc-ddraw.ini (the sources) plus ddraw.dll and ddraw.ini in the game
        // directory (the destinations). RendererTask is the only task that copies files, so
        // these four are the only files whose Zone.Identifier stream can reappear after the
        // unblocking scan has already passed them.
        public static object CnC_DDrawDeployment = new();
    }
}
