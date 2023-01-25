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
        public string Description => "设置防火墙例外项";
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
                ReportMessage(this, new TaskMessageEventArgs()
                {
                    Level = MessageLevel.Error,
                    Text = "Windows 版本过低。请手动设置 Windows 防火墙。",
                });
                return;
            }

            foreach (string exePath in new string[] {
                    Path.Combine(p.GameDir, "gamemd.exe"),
                })
            {
                string ExePathHash32 = GetExePathHashHex32(exePath);

                // Remove old firewall exceptions matching the hash

                // Ignore the potential errors since the item can either exist or not
                ConsoleCommandManager.RunConsoleCommand(
                    "netsh.exe", "advfirewall firewall delete rule name=\"Exception-" + ExePathHash32 + "\" dir=in",
                    out _, out _, out _);

                // Add firewall exception
                ConsoleCommandManager.RunConsoleCommand(
                    "netsh.exe", "advfirewall firewall add rule name=\"Exception-" + ExePathHash32 + "\" dir=in action=allow program=\"" + exePath + "\"",
                    out int exitCode, out string stdOut, out string stdErr);

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
                    string message = $"进程返回值 {exitCode}。执行失败。";
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
