using Microsoft.Win32;
using System;
using System.Globalization;

namespace Mo3RegUI.Tasks
{
    public class XboxGameBarTaskParameter : ITaskParameter
    {
    }
    public class XboxGameBarTask : ITask
    {
        public string Description => "检查 Xbox 游戏栏";
        public event EventHandler<TaskMessageEventArgs> ReportMessage;

        public void DoWork(ITaskParameter p)
        {
            if (p is XboxGameBarTaskParameter pp)
            {
                this._DoWork(pp);
            }
            else { throw new ArgumentException(); }
        }
        private void _DoWork(XboxGameBarTaskParameter p)
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

            bool gameBarEnabled = IsGameBarEnabled();
            if (gameBarEnabled)
            {
                ReportMessage(this, new TaskMessageEventArgs()
                {
                    Level = MessageLevel.Warning,
                    Text = "游戏栏已开启，部分电脑和部分渲染补丁下可能会出现游戏部分区域无法点击的情况。请在“开始菜单”→“设置”→“游戏”→“Xbox Game Bar”下找到相关设置，将游戏栏关闭。",
                });
            }

        }

        private static bool IsGameBarEnabled()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"System\GameConfigStore", false);
                object name = key?.GetValue("GameDVR_Enabled");
                return name != null && Convert.ToInt32(name, CultureInfo.InvariantCulture) == 1;

            }
            catch (Exception)
            {
            }
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\GameDVR", false);
                object name = key?.GetValue("AppCaptureEnabled");
                return name != null && Convert.ToInt32(name, CultureInfo.InvariantCulture) == 1;
            }
            catch (Exception)
            {
            }
            return false;
        }
    }
}

