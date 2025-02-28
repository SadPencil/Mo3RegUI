namespace Mo3RegUI
{
    public static class Constants
    {
        public const string GameName = "泰矿危机2";
        public const string EnglishGameName = "Tiberium Crisis 2";

        public const string AppName = GameName + " 注册机";
        public const string EnglishAppName = "Register for " + EnglishGameName;

        public const string Version = "v1.10";
        public const string AssemblyVersion = "1.10";

        public const string CopyrightYearFrom = "2019";
        public const string CopyrightYearTo = "2025";

        public const string AuthorName = "伤心的笔"; // You are not supposed to remove the name here. Append your name if you have made significant changes.
        public const string EnglishAuthorName = "SadPencil"; // You are not supposed to remove the name here. Append your name if you have made significant changes.
        public const string RepoUri = "https://github.com/SadPencil/Mo3RegUI"; // Change to your repo if you have forked it. Note: this is REQUIRED by GPLv3.

        public const string NeutralResourcesLanguage = "zh-CN";

        public const string GameConfigIniName = "RA2MD.ini";
        public const string LauncherExeName = "TiberiumCrisis.exe";
        public const string GameExeName = "gamemd.exe";
        public const string SecondaryGameExeName = "GotoTC.exe";

        public static readonly string[] VulnerableAvExes = new string[] {
            LauncherExeName,
            //GameExeName,
            SecondaryGameExeName,
            "clientupdt.dat",
            "Ares.dll",
            "Ares.dll.inj",
            "AxionGear.ext",
            "cncnet5.ext",
            "gamemd.ext",
            "Crisis.ext",
            "Crisis.ext.inc",
            "GScript.ext",
            "Phobos.ext",
            "d3dx9_29.ext",
            "ra2md.csf",
            "stringtable00.csf",
            "stringtable01.csf",
            "Resources/d3d9.ext",
            "Resources/mainexecutable.exe",
            "Resources/mainexecutable2.exe",
            "Resources/mainexecutable3.exe",
            "Resources/cnc_ddraw.dll",
            "Resources/clientdx.exe",
            "Resources/clientxna.exe",
            "Resources/clientogl.exe",
            "GameShaders/BaseShared/tchelper/tchelper.exe",
            "GameShaders/BaseShared/tchelper/TCHelper.dll",
            "gamemd.exe",
            "BINKW32.dll",
            "Blowfish.dll",
        };

        public const bool DirectXRuntimeTaskEnabled = true;
        public const bool ChinaNetworkTaskEnabled = false;
        public const bool RendererTaskEnabled = false;
        public const bool ResolutionTaskEnabled = false;
        public const bool FirstRunTaskEnabled = false;
        public const bool UserNameTaskEnabled = false;

        public const string CnCDDrawSectionName = "CNC_DDRAW";
        public const string CnCDDrawDllName = "cnc_ddraw.dll";
        public const string CnCDDrawIniName = "cnc_ddraw.ini";

        public const bool LauncherExeDpiUnaware = false;

        public const bool DetectDotNet35 = true;
        public const bool DetectXna40 = false;
    }
}
