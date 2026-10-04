using Mo3RegUI.MVVMContract;
using Mo3RegUI.ViewModel.Infrastructure;
using Mo3RegUI.LocalizationResources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace Mo3RegUI.ViewModel.Tasks
{
    public class RemoveZoneIdentifierTaskParameter : ITaskParameter
    {
        public string GameDir;
    }
    public class RemoveZoneIdentifierTask : ITask
    {
        // RemoveZoneIdentifierTask_Description: Unblock Downloaded Files
        public string DescriptionResourceKey => nameof(TextResource.RemoveZoneIdentifierTask_Description);
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

            // RendererTask copies a few files into the game directory, and File.Copy carries the
            // source's Zone.Identifier stream over to the destination. One lock covers all of
            // those files, so take it once and handle them before the general scan: RendererTask
            // cannot start a copy while this runs, and a copy that had already started before
            // the lock was taken left a marked destination, which this pass cleans as well.
            // They are excluded from the general scan below so that each file is handled and
            // reported exactly once.
            var deploymentFiles = new List<string>(GetDeploymentFiles(p.GameDir));
            var deploymentFileSet = new HashSet<string>(deploymentFiles, StringComparer.OrdinalIgnoreCase);
            lock (Locks.CnCDDrawDeployment)
            {
                foreach (string file in deploymentFiles)
                {
                    if (ProcessFile(file, failedFiles)) { unblockedCount++; }
                }
            }

            unblockedCount += ScanDirectories(p.GameDir, deploymentFileSet, failedFiles, failedDirectories);

            if (failedFiles.Count > 0)
            {
                ReportMessage(this, new TaskMessageEventArgs()
                {
                    Level = MessageLevel.Warning,
                    // RemoveZoneIdentifierTask_FailedFiles: Failed to unblock the following files: ...
                    Text = LocalizedText.FromResource(nameof(TextResource.RemoveZoneIdentifierTask_FailedFiles), string.Join("\n", failedFiles)),
                });
            }

            if (failedDirectories.Count > 0)
            {
                ReportMessage(this, new TaskMessageEventArgs()
                {
                    Level = MessageLevel.Warning,
                    // RemoveZoneIdentifierTask_FailedDirectories: Failed to scan the following directories: ...
                    Text = LocalizedText.FromResource(nameof(TextResource.RemoveZoneIdentifierTask_FailedDirectories), string.Join("\n", failedDirectories)),
                });
            }

            if (unblockedCount > 0)
            {
                ReportMessage(this, new TaskMessageEventArgs()
                {
                    Level = MessageLevel.Info,
                    // RemoveZoneIdentifierTask_Unblocked: Unblocked {0} file(s) marked as downloaded from the Internet.
                    Text = LocalizedText.FromResource(nameof(TextResource.RemoveZoneIdentifierTask_Unblocked), unblockedCount),
                });
            }
            else if (failedFiles.Count == 0 && failedDirectories.Count == 0)
            {
                ReportMessage(this, new TaskMessageEventArgs()
                {
                    Level = MessageLevel.Info,
                    // RemoveZoneIdentifierTask_NoBlockedFiles: No files are marked as downloaded from the Internet. No action required.
                    Text = LocalizedText.FromResource(nameof(TextResource.RemoveZoneIdentifierTask_NoBlockedFiles)),
                });
            }
        }

        /// <summary>
        /// The files that the renderer task reads or overwrites in the game directory.
        /// </summary>
        private static IEnumerable<string> GetDeploymentFiles(string gameDir)
        {
            yield return Path.Combine(gameDir, "Resources", Constants.CnCDDrawDllName);
            yield return Path.Combine(gameDir, "Resources", Constants.CnCDDrawIniName);
            yield return Path.Combine(gameDir, "ddraw.dll");
            yield return Path.Combine(gameDir, "ddraw.ini");
        }

        /// <summary>
        /// Removes the Zone.Identifier alternate data stream from every file below the given
        /// directory, except the ones that were already handled. Enumeration failures are
        /// recorded instead of aborting the whole scan.
        /// </summary>
        /// <returns>the number of files that were unblocked.</returns>
        private static int ScanDirectories(string gameDir, HashSet<string> skippedFiles, List<string> failedFiles, List<string> failedDirectories)
        {
            int unblockedCount = 0;

            // Enumerate the directory tree manually. Directory.GetFiles(..., AllDirectories)
            // finishes the whole recursive enumeration before returning and throws as soon as
            // a single subdirectory cannot be enumerated, which would abort the entire scan.
            var pendingDirectories = new Queue<string>();
            pendingDirectories.Enqueue(gameDir);
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
                    // Files that were already handled before the general scan are not visited again.
                    if (skippedFiles.Contains(file)) { continue; }

                    // Files that other tasks access are additionally guarded by their own lock.
                    object fileLock = Locks.GetSharedFileLock(file);
                    if (fileLock is null)
                    {
                        if (ProcessFile(file, failedFiles)) { unblockedCount++; }
                    }
                    else
                    {
                        lock (fileLock)
                        {
                            if (ProcessFile(file, failedFiles)) { unblockedCount++; }
                        }
                    }
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

            return unblockedCount;
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
                int ret = RemoveZoneIdentifier(file);
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
                            ret = RemoveZoneIdentifier(file);
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
        /// Removes the Zone.Identifier alternate data stream from a file.
        /// </summary>
        /// <returns>0 on success, otherwise the Win32 error code.</returns>
        private static int RemoveZoneIdentifier(string filePath)
        {
            string zoneIdentifier = filePath + ":Zone.Identifier";
            bool success = NativeMethods.DeleteFile(zoneIdentifier);
            return success ? 0 : Marshal.GetLastWin32Error();
        }
    }
}
