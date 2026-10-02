using Mo3RegUI.LocalizationResources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace Mo3RegUI.Tasks
{
    public class RemoveZoneIdentifierTaskParameter : ITaskParameter
    {
        public string GameDir;
    }
    public class RemoveZoneIdentifierTask : ITask
    {
        // RemoveZoneIdentifierTask_Description: Unblock Downloaded Files
        public string Description => TextResource.RemoveZoneIdentifierTask_Description;
        public event EventHandler<TaskMessageEventArgs> ReportMessage;

        public void DoWork(ITaskParameter p)
        {
            if (p is RemoveZoneIdentifierTaskParameter pp)
            {
                this._DoWork(pp);
            }
            else { throw new ArgumentException(); }
        }
        private void _DoWork(RemoveZoneIdentifierTaskParameter p)
        {
            // Windows marks files downloaded from the Internet by adding a Zone.Identifier
            // alternate data stream to them. Such files may make the client misbehave, so
            // remove the stream from every file in the game directory.
            // https://stackoverflow.com/a/6375373
            var failedFiles = new List<string>();
            var failedDirectories = new List<string>();
            int unblockedCount = 0;

            // Enumerate the directory tree manually. Directory.GetFiles(..., AllDirectories)
            // finishes the whole recursive enumeration before returning and throws as soon as
            // a single subdirectory cannot be enumerated, which would abort the entire scan.
            var pendingDirectories = new Queue<string>();
            pendingDirectories.Enqueue(p.GameDir);
            while (pendingDirectories.Count > 0)
            {
                string directory = pendingDirectories.Dequeue();

                string[] files;
                try
                {
                    files = Directory.GetFiles(directory);
                }
                catch (Exception ex)
                {
                    failedDirectories.Add($"{directory}: {ex.Message}");
                    continue;
                }

                foreach (string file in files)
                {
                    // Other tasks may create or overwrite this file, so serialize with them.
                    object fileLock = GetSharedFileLock(file);
                    bool unblocked;
                    if (fileLock is null)
                    {
                        unblocked = ProcessFile(file, failedFiles);
                    }
                    else
                    {
                        lock (fileLock)
                        {
                            unblocked = ProcessFile(file, failedFiles);
                        }
                    }

                    if (unblocked) { unblockedCount++; }
                }

                string[] subDirectories;
                try
                {
                    subDirectories = Directory.GetDirectories(directory);
                }
                catch (Exception ex)
                {
                    failedDirectories.Add($"{directory}: {ex.Message}");
                    continue;
                }

                foreach (string subDirectory in subDirectories)
                {
                    pendingDirectories.Enqueue(subDirectory);
                }
            }

            if (failedFiles.Count > 0)
            {
                ReportMessage(this, new TaskMessageEventArgs()
                {
                    Level = MessageLevel.Warning,
                    // RemoveZoneIdentifierTask_FailedFiles: Failed to unblock the following files:
                    Text = string.Format(TextResource.RemoveZoneIdentifierTask_FailedFiles, string.Join("\n", failedFiles)),
                });
            }

            if (failedDirectories.Count > 0)
            {
                ReportMessage(this, new TaskMessageEventArgs()
                {
                    Level = MessageLevel.Warning,
                    // RemoveZoneIdentifierTask_FailedDirectories: Failed to scan the following directories:
                    Text = string.Format(TextResource.RemoveZoneIdentifierTask_FailedDirectories, string.Join("\n", failedDirectories)),
                });
            }

            if (unblockedCount > 0)
            {
                ReportMessage(this, new TaskMessageEventArgs()
                {
                    Level = MessageLevel.Info,
                    // RemoveZoneIdentifierTask_Unblocked: Unblocked {0} file(s) marked as downloaded from the Internet.
                    Text = string.Format(TextResource.RemoveZoneIdentifierTask_Unblocked, unblockedCount),
                });
            }
            else if (failedFiles.Count == 0 && failedDirectories.Count == 0)
            {
                ReportMessage(this, new TaskMessageEventArgs()
                {
                    Level = MessageLevel.Info,
                    // RemoveZoneIdentifierTask_NoBlockedFiles: No files are marked as downloaded from the Internet. No action required.
                    Text = TextResource.RemoveZoneIdentifierTask_NoBlockedFiles,
                });
            }
        }

        /// <summary>
        /// Removes the Zone.Identifier alternate data stream from a single file. Failures are
        /// recorded instead of aborting the whole scan.
        /// </summary>
        /// <returns>true when the file was unblocked.</returns>
        private static bool ProcessFile(string file, List<string> failedFiles)
        {
            try
            {
                int ret = ZoneIdentifier.Remove(file);
                if (ret == 0) { return true; }

                // The file does not carry a Zone.Identifier stream. This is the normal case.
                if (ret == NativeConstants.ERROR_FILE_NOT_FOUND) { return false; }

                // The file system does not support alternate data streams (e.g. exFAT).
                if (ret == NativeConstants.ERROR_INVALID_NAME) { return false; }

                // Try again, but temporarily remove the read-only attribute.
                if (ret == NativeConstants.ERROR_ACCESS_DENIED)
                {
                    var info = new FileInfo(file);
                    if (info.Exists && info.IsReadOnly)
                    {
                        info.IsReadOnly = false;
                        try
                        {
                            ret = ZoneIdentifier.Remove(file);
                        }
                        finally
                        {
                            info.IsReadOnly = true;
                        }
                        if (ret == 0) { return true; }
                    }
                }

                failedFiles.Add($"{file}: {new Win32Exception(ret).Message} ({ret})");
            }
            catch (Exception ex)
            {
                failedFiles.Add($"{file}: {ex.Message}");
            }
            return false;
        }

        /// <summary>
        /// Returns the lock that guards files which may be created or overwritten by other tasks.
        /// </summary>
        private static object GetSharedFileLock(string file)
        {
            string fileName = Path.GetFileName(file);
            if (string.Equals(fileName, "ddraw.dll", StringComparison.OrdinalIgnoreCase)) { return Locks.DDraw_DLL; }
            if (string.Equals(fileName, "ddraw.ini", StringComparison.OrdinalIgnoreCase)) { return Locks.DDraw_INI; }
            if (string.Equals(fileName, Constants.CnCDDrawIniName, StringComparison.OrdinalIgnoreCase)) { return Locks.CnC_DDraw_INI; }
            if (string.Equals(fileName, Constants.GameConfigIniName, StringComparison.OrdinalIgnoreCase)) { return Locks.RA2MO_INI; }
            return null;
        }
    }
}
