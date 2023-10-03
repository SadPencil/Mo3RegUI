using Microsoft.Win32;
using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Mo3RegUI.Tasks
{
    public class MonitorRemovalTaskParameter : ITaskParameter
    {
    }
    public class MonitorRemovalTask : ITask
    {
        public string Description => "检查显示器断开连接时最小化窗口功能";
        public event EventHandler<TaskMessageEventArgs> ReportMessage;

        public void DoWork(ITaskParameter p)
        {
            if (p is MonitorRemovalTaskParameter pp)
            {
                this._DoWork(pp);
            }
            else { throw new ArgumentException(); }
        }
        private void _DoWork(MonitorRemovalTaskParameter p)
        {
            if (!(Environment.OSVersion.Version.Major >= 10))
            {
                ReportMessage(this, new TaskMessageEventArgs()
                {
                    Level = MessageLevel.Info,
                    Text = "Windows 版本过低，无此功能。",
                });
                return;
            }

            using var hkcu = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default);
            using var key = hkcu.OpenSubKey(@"Control Panel\Desktop", false);
            object val = key?.GetValue("MonitorRemovalRecalcBehavior");
            bool disabled = val != null && Convert.ToInt32(val, CultureInfo.InvariantCulture) == 1;
            if (disabled)
            {
                ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Info, Text = "该功能处于关闭状态。无需操作。" });

            }
            else
            {
                ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Warning, Text = "该功能处于开启状态。全屏游戏且游戏分辨率不同于桌面分辨率时，部分机器会在进入游戏后出现马上跳回桌面的情况。请在“开始菜单”→“设置”→“系统”→“屏幕”下找到“多显示器”→“在显示器断开连接时最小化窗口”复选框，将此功能关闭，并重启电脑。即使只有一台显示器，也要这样做。" });
            }
        }

    }
}
