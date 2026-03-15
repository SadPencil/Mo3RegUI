using Mo3RegUI.LocalizationResources;

namespace Mo3RegUI
{
    public static class Constants
    {
        public static string GameName => TextResource.Constants_GameName;
        public const string EnglishGameName = "Mental Omega 3.3.6";

        public static string AppName => TextResource.Constants_AppName;
        public const string EnglishAppName = "Register for Mental Omega 3.3.6";

        public const string Version = "v2.5.0";
        public const string AssemblyVersion = "2.5.0.0";

        public const string CopyrightYearFrom = "2019";
        public const string CopyrightYearTo = "2026";

        public static string AuthorName => TextResource.Constants_AuthorName; // You are not supposed to remove the name here. Append your name if you have made significant changes.
        public const string EnglishAuthorName = "SadPencil"; // You are not supposed to remove the name here. Append your name if you have made significant changes.
        public const string RepoUri = "https://github.com/SadPencil/Mo3RegUI"; // Change to your repo if you have forked it. Note: this is REQUIRED by GPLv3.

        public const string NeutralResourcesLanguage = "en-US";

        public const string GameConfigIniName = "RA2MO.ini";
        public const string LauncherExeName = "MentalOmegaClient.exe";
        public const string GameExeName = "gamemd.exe";
        public const string SecondaryGameExeName = "Syringe.exe";

        public static readonly string[] VulnerableAvExes = new string[] {
            "cncnet5.dll",
            "cncnet5mo.dll",
            "ares.dll",
            "Phobos.dll",
            "Syringe.exe",
            "Map Editor/FinalAlert2MO.exe",
            "Map Editor/Syringe.exe",
            "Resources/ddraw_dxwnd.dll",
        };

        public const bool CheckDirectXRuntime = false;
        public const string CnCDDrawDllName = "cnc-ddraw.dll";
        public const string CnCDDrawIniName = "cnc-ddraw.ini";
        public const bool LauncherExeDpiUnaware = false;

        public const bool RequireBlowfishRegistration = false; // No need for Phobos
    }
}
