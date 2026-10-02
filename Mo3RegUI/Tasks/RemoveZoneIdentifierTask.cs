using Mo3RegUI.LocalizationResources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

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
            var failedMessages = new List<string>();
            int unblockedCount = 0;

            foreach (string file in Directory.GetFiles(p.GameDir, "*", SearchOption.AllDirectories))
            {
                int ret = RemoveZoneIdentifier(file);
                if (ret == 0)
                {
                    unblockedCount++;
                    continue;
                }

                // The file does not carry a Zone.Identifier stream. This is the normal case.
                if (ret == NativeConstants.ERROR_FILE_NOT_FOUND) { continue; }

                // The file system does not support alternate data streams (e.g. exFAT).
                if (ret == NativeConstants.ERROR_INVALID_NAME) { continue; }

                // Try again, but temporarily remove the read-only attribute.
                if (ret == NativeConstants.ERROR_ACCESS_DENIED)
                {
                    var info = new FileInfo(file);
                    if (info.IsReadOnly)
                    {
                        info.IsReadOnly = false;
                        ret = RemoveZoneIdentifier(file);
                        info.IsReadOnly = true;
                        if (ret == 0)
                        {
                            unblockedCount++;
                            continue;
                        }
                    }
                }

                failedMessages.Add($"{file}: {new Win32Exception(ret).Message} ({ret})");
            }

            if (failedMessages.Count > 0)
            {
                ReportMessage(this, new TaskMessageEventArgs()
                {
                    Level = MessageLevel.Warning,
                    // RemoveZoneIdentifierTask_FailedFiles: Failed to unblock the following files:
                    Text = string.Format(TextResource.RemoveZoneIdentifierTask_FailedFiles, string.Join("\n", failedMessages)),
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
            else if (failedMessages.Count == 0)
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
