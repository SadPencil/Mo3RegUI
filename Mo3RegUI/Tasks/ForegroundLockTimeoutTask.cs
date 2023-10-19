using Microsoft.Win32;
using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Mo3RegUI.Tasks
{
    public class ForegroundLockTimeoutTaskParameter : ITaskParameter
    {
    }
    public class ForegroundLockTimeoutTask : ITask
    {
        public string Description => "检查前台锁定时间";
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
                ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Info, Text = $"前台锁定时间不低于 {valDefault} 毫秒。无需操作。" });
            }
            else
            {
                ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Warning, Text = $"前台锁定时间低于 {valDefault} 毫秒，有概率导致游戏中返回桌面的情况发生。正在修复……" });
                key.SetValue("ForegroundLockTimeout", valDefault, RegistryValueKind.DWord);
                ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Info, Text = $"修复成功。前台锁定时间修改为 {valDefault} 毫秒。" });
            }
        }

    }
}
