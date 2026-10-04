using Mo3RegUI.MvvmContract;
using Mo3RegUI.ViewModel.Infrastructure;
using Mo3RegUI.LocalizationResources;
using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Mo3RegUI.ViewModel.Tasks
{
    public class FirewallSettingTaskParameter : ITaskParameter
    {
        public string GameDir;
    }
    public class FirewallSettingTask : ITask
    {
        // FirewallSettingTask_Description: Set Firewall Exception
        public string DescriptionResourceKey => nameof(TextResource.FirewallSettingTask_Description);
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
                    Text = LocalizedText.FromResource(nameof(TextResource.FirewallSettingTask_OsVersionTooLow)),
                });
                return;
            }

            foreach (string exePath in new string[] {
                    Path.Combine(p.GameDir, Constants.GameExeName),
                    Path.Combine(p.GameDir, "Resources","clientdx.exe"),
                    Path.Combine(p.GameDir, "Resources","clientogl.exe"),
                    Path.Combine(p.GameDir, "Resources","clientxna.exe"),
                })
            {
                foreach (string direction in new string[] { "in", "out" })
                {
                    string ExePathHash32 = GetExePathHashHex32(exePath);

                    // Remove old firewall exceptions matching the hash. Ignore the potential errors since the item can either exist or not
                    ConsoleCommandManager.RunConsoleCommand("cmd.exe", "/c \"chcp 65001 > NUL && netsh advfirewall firewall delete rule name=\\\"Mo3RegUI-" + direction + "-" + ExePathHash32 + "\\\" dir=" + direction + "\"", out _, out _, out _);

                    // Add firewall exception
                    ConsoleCommandManager.RunConsoleCommand("cmd.exe", "/c \"chcp 65001 > NUL && netsh advfirewall firewall add rule name=\\\"Mo3RegUI-" + direction + "-" + ExePathHash32 + "\\\" dir=" + direction + " action=allow program=\\\"" + exePath + "\\\"\"", out int exitCode, out string stdOut, out string stdErr);

                    if (!string.IsNullOrWhiteSpace(stdOut))
                    {
                        // External command output; not translatable.
                        ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Info, Text = LocalizedText.FromLiteral(stdOut.Trim()) });
                    }

                    if (!string.IsNullOrWhiteSpace(stdErr))
                    {
                        // External command output; not translatable.
                        ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Warning, Text = LocalizedText.FromLiteral(stdErr.Trim()) });
                    }

                    if (exitCode != 0)
                    {
                        // Task_ProcessExitCodeFailure: Process returned exit code {0}. Execution failed.
                        ReportMessage(this, new TaskMessageEventArgs()
                        {
                            Level = MessageLevel.Error,
                            Text = LocalizedText.FromResource(nameof(TextResource.Task_ProcessExitCodeFailure), exitCode),
                        });
                    }
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
