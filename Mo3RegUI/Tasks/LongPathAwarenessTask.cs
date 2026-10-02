using Microsoft.Win32;
using Mo3RegUI.LocalizationResources;
using System;
using System.Globalization;

namespace Mo3RegUI.Tasks
{
    public class LongPathAwarenessTaskParameter : ITaskParameter
    {
    }
    public class LongPathAwarenessTask : ITask
    {
        // LongPathAwarenessTask_Description: Check Long Path Support
        public string Description => TextResource.LongPathAwarenessTask_Description;
        public event EventHandler<TaskMessageEventArgs> ReportMessage;

        public void DoWork(ITaskParameter p)
        {
            if (p is LongPathAwarenessTaskParameter pp)
            {
                this._DoWork(pp);
            }
            else { throw new ArgumentException(); }
        }
        private void _DoWork(LongPathAwarenessTaskParameter p)
        {
            // Windows honors the LongPathsEnabled registry value since Windows 10 version 1607 (build 14393).
            if (Environment.OSVersion.Version.CompareTo(new Version(10, 0, 14393)) < 0)
            {
                ReportMessage(this, new TaskMessageEventArgs()
                {
                    Level = MessageLevel.Warning,
                    // LongPathAwarenessTask_OsVersionTooLow: Long path support requires Windows 10 version 1607 or later, and the current system version is too low to enable it. Please install the game in a directory with a short path; otherwise, the custom maps downloaded by the client may be stored in paths that exceed the Windows path length limit, and the game may fail to read them.
                    Text = TextResource.LongPathAwarenessTask_OsVersionTooLow,
                });
                return;
            }

            using var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\FileSystem", writable: true);
            object val = key?.GetValue("LongPathsEnabled");
            bool longPathsEnabled = val is not null && Convert.ToInt32(val, CultureInfo.InvariantCulture) == 1;
            if (longPathsEnabled)
            {
                ReportMessage(this, new TaskMessageEventArgs()
                {
                    Level = MessageLevel.Info,
                    // LongPathAwarenessTask_AlreadyEnabled: Long path support is enabled. No action required.
                    Text = TextResource.LongPathAwarenessTask_AlreadyEnabled,
                });
            }
            else
            {
                ReportMessage(this, new TaskMessageEventArgs()
                {
                    Level = MessageLevel.Warning,
                    // LongPathAwarenessTask_Disabled: Long path support is not enabled. The custom maps downloaded by the client may be stored in paths that exceed the Windows path length limit, which may prevent the game from reading them. Enabling long path support...
                    Text = TextResource.LongPathAwarenessTask_Disabled,
                });
                key.SetValue("LongPathsEnabled", 1, RegistryValueKind.DWord);
                ReportMessage(this, new TaskMessageEventArgs()
                {
                    Level = MessageLevel.Info,
                    // LongPathAwarenessTask_Fixed: Fixed successfully. Long path support has been enabled. Please restart the computer to make the change take effect.
                    Text = TextResource.LongPathAwarenessTask_Fixed,
                });
            }
        }
    }
}
