using Microsoft.Win32;
using Mo3RegUI.LocalizationResources;
using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Mo3RegUI.Tasks
{
    public class PathCheckTaskParameter : ITaskParameter
    {
        public string GameDir;
    }
    public class PathCheckTask : ITask
    {
        // PathCheckTask_Description: Check Game Path Format
        public string Description => TextResource.PathCheckTask_Description;
        public event EventHandler<TaskMessageEventArgs> ReportMessage;

        public void DoWork(ITaskParameter p)
        {
            if (p is PathCheckTaskParameter pp)
            {
                this._DoWork(pp);
            }
            else { throw new ArgumentException(); }
        }
        private void _DoWork(PathCheckTaskParameter p)
        {
            DoPathLengthCheck(p.GameDir);
            DoLongPathAwarenessCheck(p.GameDir);
        }

        private void DoPathLengthCheck(string gameDir)
        {
            // Make sure path length is smaller than 130 bytes (after converting to ANSI)
            if (Encoding.Convert(Encoding.Unicode, Encoding.Default, Encoding.Unicode.GetBytes(gameDir)).Count() > 130)
            {
                // PathCheckTask_PathTooLong: The current game directory path is too long. The game may not run normally.
                throw new Exception(TextResource.PathCheckTask_PathTooLong);
            }

            // Make sure path does not contain "%"
            if (gameDir.Contains(@"%"))
            {
                // PathCheckTask_PathContainsPercent: The current game directory path contains the special character % (percent sign). Windows Firewall may not handle this correctly.
                string message = TextResource.PathCheckTask_PathContainsPercent;
                ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Error, Text = message });
            }
        }

        private void DoLongPathAwarenessCheck(string gameDir)
        {
            // Windows honors the LongPathsEnabled registry value since Windows 10 version 1607 (build 14393).
            if (Environment.OSVersion.Version.CompareTo(new Version(10, 0, 14393)) < 0)
            {
                ReportMessage(this, new TaskMessageEventArgs()
                {
                    Level = MessageLevel.Warning,
                    // LongPathAwarenessTask_OsVersionTooLow: Long path support requires Windows 10 version 1607 or later, and the current system version is too low to enable it. The custom maps downloaded by the client may be stored in paths that exceed the Windows path length limit, and the game may fail to read them.
                    Text = TextResource.LongPathAwarenessTask_OsVersionTooLow,
                });
                return;
            }

            const string keyPath = @"SYSTEM\CurrentControlSet\Control\FileSystem";
            const string valueName = "LongPathsEnabled";

            bool longPathsEnabled;
            using (var key = Registry.LocalMachine.OpenSubKey(keyPath))
            {
                object val = key?.GetValue(valueName);
                longPathsEnabled = val is not null && Convert.ToInt32(val, CultureInfo.InvariantCulture) == 1;
            }

            if (longPathsEnabled)
            {
                ReportMessage(this, new TaskMessageEventArgs()
                {
                    Level = MessageLevel.Info,
                    // LongPathAwarenessTask_AlreadyEnabled: Long path support is enabled. No action required.
                    Text = TextResource.LongPathAwarenessTask_AlreadyEnabled,
                });
                return;
            }

            using var writableKey = Registry.LocalMachine.OpenSubKey(keyPath, writable: true);
            if (writableKey is null)
            {
                ReportMessage(this, new TaskMessageEventArgs()
                {
                    Level = MessageLevel.Error,
                    // LongPathAwarenessTask_RegistryKeyUnavailable: Failed to open the registry key for long path support.
                    Text = TextResource.LongPathAwarenessTask_RegistryKeyUnavailable,
                });
                return;
            }

            ReportMessage(this, new TaskMessageEventArgs()
            {
                Level = MessageLevel.Warning,
                // LongPathAwarenessTask_Disabled: Long path support is not enabled. The custom maps downloaded by the client may be stored in paths that exceed the Windows path length limit, which may prevent the game from reading them. Enabling long path support...
                Text = TextResource.LongPathAwarenessTask_Disabled,
            });
            writableKey.SetValue(valueName, 1, RegistryValueKind.DWord);
            ReportMessage(this, new TaskMessageEventArgs()
            {
                Level = MessageLevel.Info,
                // LongPathAwarenessTask_Fixed: Fixed successfully. Long path support has been enabled. Please restart the computer to make the change take effect.
                Text = TextResource.LongPathAwarenessTask_Fixed,
            });
        }
    }
}
