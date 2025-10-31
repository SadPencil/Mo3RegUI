using Microsoft.Win32;
using System;
using System.IO;
using System.Text;

namespace Mo3RegUI.Tasks
{
    public class Ra2RegTaskParameter : ITaskParameter
    {
        public string GameDir;
    }
    public class Ra2RegTask : ITask
    {
        public string Description => "注册 Red Alert 2";
        public event EventHandler<TaskMessageEventArgs> ReportMessage;

        public void DoWork(ITaskParameter p)
        {
            if (p is Ra2RegTaskParameter pp)
            {
                this._DoWork(pp);
            }
            else { throw new ArgumentException(); }
        }
        private void _DoWork(Ra2RegTaskParameter p)
        {
            // Generate serial number
            var rng = new Random();
            char[] randomedChars = "0123456789".ToCharArray();
            int snLength = 22;
            var sb = new StringBuilder();
            for (int i = 0; i < snLength; i++)
            {
                _ = sb.Append(randomedChars[rng.Next(randomedChars.Length)]);
            }
            string sn = sb.ToString();

            using var HKLM_32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
            using var ra2Key = HKLM_32.CreateSubKey(@"SOFTWARE\Westwood\Red Alert 2");
            ra2Key.SetValue("Serial", sn);
            ra2Key.SetValue("Name", "Red Alert 2");
            ra2Key.SetValue("InstallPath", Path.Combine(p.GameDir, "RA2.EXE"));
            ra2Key.SetValue("SKU", 8448);
            ra2Key.SetValue("Version", 65542);

            using var yrKey = HKLM_32.CreateSubKey(@"SOFTWARE\Westwood\Yuri's Revenge");
            yrKey.SetValue("Serial", sn);
            yrKey.SetValue("Name", "Yuri's Revenge");
            yrKey.SetValue("InstallPath", Path.Combine(p.GameDir, "RA2MD.EXE"));
            yrKey.SetValue("SKU", 10496);
            yrKey.SetValue("Version", 65537);

            ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Info, Text = "写入注册表成功。" });

            if (Constants.RequireBlowfishRegistration)
            {
                string blowfishPath = Path.Combine(p.GameDir, "Blowfish.dll");
                // Register blowfish
                //_ = DllRegisterServer(); // this failed

                if (!File.Exists(blowfishPath))
                {
                    throw new Exception("找不到 Blowfish.dll 文件。");
                }

                ConsoleCommandManager.RunConsoleCommand("regsvr32.exe", $"/s \"{blowfishPath}\"", out int exitCode, out string stdOut, out string stdErr);

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
                else
                {
                    ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Info, Text = "注册 Blowfish.dll 成功。" });
                }
            }
            else
            {
                ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Info, Text = "无需注册 Blowfish.dll。" });
            }

        }

        //[DllImport("Blowfish.dll")]
        //private static extern int DllRegisterServer();
    }
}
