using Microsoft.Win32;
using System;
using System.IO;

namespace Mo3RegUI.Tasks
{
    public class CompatibilitySettingTaskParameter : ITaskParameter
    {
        public string GameDir;
    }
    public class CompatibilitySettingTask : ITask
    {
        public string Description => "设置程序兼容性";
        public event EventHandler<TaskMessageEventArgs> ReportMessage;

        public void DoWork(ITaskParameter p)
        {
            if (p is CompatibilitySettingTaskParameter pp)
            {
                this._DoWork(pp);
            }
            else { throw new ArgumentException(); }
        }
        private void _DoWork(CompatibilitySettingTaskParameter p)
        {
            foreach (string exePath in new string[] {
                    Path.Combine(p.GameDir, Constants.GameExeName),
                    Path.Combine(p.GameDir, "Syringe.exe"),
                })
            {
                using var registryKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers");
                registryKey.SetValue(exePath, "~ RUNASADMIN HIGHDPIAWARE");
            }

            foreach (string exePath in new string[] {
                    Path.Combine(p.GameDir, Constants.LauncherExeName),
                })
            {
                using var registryKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers");
                registryKey.SetValue(exePath, "~ RUNASADMIN DPIUNAWARE");
            }
        }
    }
}
