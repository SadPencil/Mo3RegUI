using Microsoft.Win32;
using Mo3RegUI.LocalizationResources;
using System;
using System.Globalization;

namespace Mo3RegUI.Tasks
{
    public class XboxGameBarTaskParameter : ITaskParameter
    {
    }
    public class XboxGameBarTask : ITask
    {
        // XboxGameBarTask_Description: Check Xbox Game Bar
        public string Description => TextResource.XboxGameBarTask_Description;
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
                    // Task_OsVersionTooLow_NoFeature: Windows version is too low. This feature is not available.
                    Text = TextResource.Task_OsVersionTooLow_NoFeature,
                });
                return;
            }

            bool gameBarEnabled = IsGameBarEnabled();
            if (gameBarEnabled)
            {
                ReportMessage(this, new TaskMessageEventArgs()
                {
                    Level = MessageLevel.Warning,
                    // XboxGameBarTask_GameBarEnabled: Game Bar is enabled, which may cause certain areas of the game to be unclickable ...
                    Text = TextResource.XboxGameBarTask_GameBarEnabled,
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

