using Mo3RegUI.MVVMContract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Mo3RegUI.ViewModel.Infrastructure
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
        public static object CnCDDrawDeployment = new();

        // File names that are guarded by CnC_DDrawDeployment, wherever they appear below the
        // game directory.
        private static readonly List<string> CnCDDrawDeploymentFileNames = new()
        {
            "ddraw.dll",
            "ddraw.ini",
            Constants.CnCDDrawDllName,
            Constants.CnCDDrawIniName,
        };

        /// <summary>
        /// Returns the lock that guards a file which another task reads or overwrites, or null
        /// when no other task touches it.
        /// </summary>
        public static object GetSharedFileLock(string file)
        {
            string fileName = Path.GetFileName(file);

            if (CnCDDrawDeploymentFileNames.Contains(fileName, StringComparer.OrdinalIgnoreCase))
            {
                return CnCDDrawDeployment;
            }

            if (string.Equals(fileName, Constants.GameConfigIniName, StringComparison.OrdinalIgnoreCase))
            {
                return RA2MO_INI;
            }

            return null;
        }
    }
}
