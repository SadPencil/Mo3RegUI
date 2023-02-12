namespace Mo3RegUI
{
    public static class Constants
    {
        public const string GameName = "Tiberium Crisis 2";
        public const string EnglishGameName = "Tiberium Crisis 2";

        public const string AppName = GameName + " 注册机";
        public const string EnglishAppName = "Register for " + EnglishGameName;

        public const string Version = "v2.2.0";
        public const string AssemblyVersion = "2.2.0.0";

        public const string CopyrightYearFrom = "2019";
        public const string CopyrightYearTo = "2023";

        public const string AuthorName = "伤心的笔"; // You are not supposed to remove the name here. Append your name if you have made significant changes.
        public const string EnglishAuthorName = "Sad Pencil"; // You are not supposed to remove the name here. Append your name if you have made significant changes.
        public const string RepoUri = "https://github.com/SadPencil/Mo3RegUI"; // Change to your repo if you have forked it. Note: this is REQUIRED by GPLv3.

        public const string NeutralResourcesLanguage = "zh-CN";

        public const string GameConfigIniName = "RA2MD.ini";
        public const string LauncherExeName = "TiberiumCrisis.exe";
        public const string GameExeName = "gamemd.exe";
        public const string SecondaryGameExeName = "Syringe.exe";

        public static readonly string[] VulnerableAvExes = new string[] {
            "TiberiumCrisis.exe",
            "gamemd.exe",
            "GotoTC.exe",
            "Ares.dll",
            "AxionGear.ext",
            "cncnet5.ext",
            "Crisis.ext",
            "GScript.ext",
            "d3d9.ext",
            "d3d9_29.ext",
            "ra2md.csf",
            "Resources/clientdx.exe",
            "Resources/clientxna.exe",
            "Resources/clientogl.exe",
            "Resources/cnc_ddraw.dll",
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

        public const bool DetectDotNet35 = false;
        public const bool DetectXna40 = false;
    }
}
