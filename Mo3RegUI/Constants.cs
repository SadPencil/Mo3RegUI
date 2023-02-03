namespace Mo3RegUI
{
    public static class Constants
    {
        public const string GameName = "心灵终结 3.3.6";
        public const string EnglishGameName = "Mental Omega 3.3.6";

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

        public const string GameConfigIniName = "RA2MO.ini";
        public const string LauncherExeName = "MentalOmegaClient.exe";
        public const string GameExeName = "gamemd.exe";
        public const string SecondaryGameExeName = "Syringe.exe";

        public static readonly string[] VulnerableAvExes = new string[] {
            "cncnet5.dll",
            "cncnet5mo.dll",
            "ares.dll",
            "Blowfish.dll",
            "Syringe.exe",
            "Map Editor/FinalAlert2MO.exe",
            "Map Editor/Syringe.exe",
            "Resources/ddraw_dxwnd.dll",
        };

        public const bool CheckDirectXRuntime = false;
    }
}
