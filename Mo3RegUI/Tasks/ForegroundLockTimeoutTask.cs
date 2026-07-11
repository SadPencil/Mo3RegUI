using Microsoft.Win32;
using Mo3RegUI.LocalizationResources;
using System;
using System.Globalization;

namespace Mo3RegUI.Tasks
{
    public class ForegroundLockTimeoutTaskParameter : ITaskParameter
    {
    }
    public class ForegroundLockTimeoutTask : ITask
    {
        // ForegroundLockTimeoutTask_Description: Check Foreground Lock Timeout
        public string Description => TextResource.ForegroundLockTimeoutTask_Description;
        public event EventHandler<TaskMessageEventArgs> ReportMessage;

        public void DoWork(ITaskParameter p)
        {
            if (p is ForegroundLockTimeoutTaskParameter pp)
            {
                this._DoWork(pp);
            }
            else { throw new ArgumentException(); }
        }
        private void _DoWork(ForegroundLockTimeoutTaskParameter p)
        {
            using var hkcu = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default);
            using var key = hkcu.OpenSubKey(@"Control Panel\Desktop", writable: true);
            object val = key?.GetValue("ForegroundLockTimeout");
            int valDword = val is null ? 0 : Convert.ToInt32(val, CultureInfo.InvariantCulture);
            int valDefault = 0x30d40;
            if (valDword >= valDefault)
            {
                // ForegroundLockTimeoutTask_SufficientTimeout: Foreground lock timeout is no less than {0} milliseconds. No action required.
                ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Info, Text = string.Format(TextResource.ForegroundLockTimeoutTask_SufficientTimeout, valDefault) });
            }
            else
            {
                // ForegroundLockTimeoutTask_InsufficientTimeout: Foreground lock timeout is less than {0} milliseconds, which may occasionally cause the game to return to the desktop. Fixing...
                ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Warning, Text = string.Format(TextResource.ForegroundLockTimeoutTask_InsufficientTimeout, valDefault) });
                key.SetValue("ForegroundLockTimeout", valDefault, RegistryValueKind.DWord);
                // ForegroundLockTimeoutTask_FixedTimeout: Fixed successfully. Foreground lock timeout has been set to {0} milliseconds.
                ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Info, Text = string.Format(TextResource.ForegroundLockTimeoutTask_FixedTimeout, valDefault) });
            }
        }

    }
}
