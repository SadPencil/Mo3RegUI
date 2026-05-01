using Mo3RegUI.LocalizationResources;
using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Mo3RegUI.Tasks
{
    public class FirewallSettingTaskParameter : ITaskParameter
    {
        public string GameDir;
    }
    public class FirewallSettingTask : ITask
    {
        // FirewallSettingTask_Description: Set Firewall Exception
        public string Description => TextResource.FirewallSettingTask_Description;
        public event EventHandler<TaskMessageEventArgs> ReportMessage;

        public void DoWork(ITaskParameter p)
        {
            if (p is FirewallSettingTaskParameter pp)
            {
                this._DoWork(pp);
            }
            else { throw new ArgumentException(); }
        }
        private void _DoWork(FirewallSettingTaskParameter p)
        {
            if (!(Environment.OSVersion.Version.Major >= 6))
            {
                // FirewallSettingTask_OsVersionTooLow: Windows version is too low. Please set Windows Firewall manually.
                ReportMessage(this, new TaskMessageEventArgs()
                {
                    Level = MessageLevel.Error,
                    Text = TextResource.FirewallSettingTask_OsVersionTooLow,
                });
                return;
            }

            foreach (string exePath in new string[] {
                    Path.Combine(p.GameDir, Constants.GameExeName),
                })
            {
                string ExePathHash32 = GetExePathHashHex32(exePath);

                // Remove old firewall exceptions matching the hash

                // Ignore the potential errors since the item can either exist or not
                // ConsoleCommandManager.RunConsoleCommand("netsh.exe", "advfirewall firewall delete rule name=\"Mo3RegUI-" + ExePathHash32 + "\" dir=in", out _, out _, out _);
                ConsoleCommandManager.RunConsoleCommand("cmd.exe", "/c \"chcp 65001 > NUL && netsh advfirewall firewall delete rule name=\\\"Mo3RegUI-" + ExePathHash32 + "\\\" dir=in\"", out _, out _, out _);

                // Add firewall exception
                // ConsoleCommandManager.RunConsoleCommand("netsh.exe", "advfirewall firewall add rule name=\"Mo3RegUI-" + ExePathHash32 + "\" dir=in action=allow program=\"" + exePath + "\"", out int exitCode, out string stdOut, out string stdErr);
                ConsoleCommandManager.RunConsoleCommand("cmd.exe", "/c \"chcp 65001 > NUL && netsh advfirewall firewall add rule name=\\\"Mo3RegUI-" + ExePathHash32 + "\\\" dir=in action=allow program=\\\"" + exePath + "\\\"\"", out int exitCode, out string stdOut, out string stdErr);

                if (!string.IsNullOrWhiteSpace(stdOut))
                {
                    ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Info, Text = stdOut.Trim() });
                }

                if (!string.IsNullOrWhiteSpace(stdErr))
                {
                    ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Warning, Text = stdErr.Trim() });
                }

                if (exitCode != 0)
                {
                    // Task_ProcessExitCodeFailure: Process returned exit code {0}. Execution failed.
                    string message = string.Format(TextResource.Task_ProcessExitCodeFailure, exitCode);
                    ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Error, Text = message });
                }
            }
        }

        private static string GetExePathHashHex32(string path)
        {
            byte[] digest;
            using (var hash = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(path);
                digest = hash.ComputeHash(bytes);
            }

            string exePathHash32 = ByteArrayToHex(digest);
            if (exePathHash32.Length > 16)
            {
                exePathHash32 = exePathHash32.Substring(0, 16);
            }

            return exePathHash32;
        }

        private static string ByteArrayToHex(byte[] arr)
        {
            var ret = new StringBuilder();
            for (int i = 0; i < arr.Length; i++)
            {
                _ = ret.Append(arr[i].ToString("X2", CultureInfo.InvariantCulture));
            }

            return ret.ToString();
        }
    }
}
