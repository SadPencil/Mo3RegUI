using Mo3RegUI.Tasks;
using System;

namespace Mo3RegUI
{
    public class ProcessMitigationTaskParameter : ITaskParameter
    {
        public string GameDir;
    }
    public class ProcessMitigationTask : ITask
    {
        public string Description => "关闭强制映像虚拟化";
        public event EventHandler<TaskMessageEventArgs> ReportMessage;

        public void DoWork(ITaskParameter p)
        {
            if (p is ProcessMitigationTaskParameter pp)
            {
                this._DoWork(pp);
            }
            else { throw new ArgumentException(); }
        }
        private void _DoWork(ProcessMitigationTaskParameter p)
        {
            if (!(Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= 16299))
            {
                ReportMessage(this, new TaskMessageEventArgs()
                {
                    Level = MessageLevel.Info,
                    Text = "Windows 版本过低，无此功能。",
                });
                return;
            }

            bool hasASLRTurnedOffForGamemd = false;
            {
                ConsoleCommandManager.RunConsoleCommand("powershell.exe", $"-Command \"Set-ProcessMitigation -Name {Constants.GameExeName} -Disable ForceRelocateImages\"", out int exitCode, out string stdOut, out string stdErr);

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
                    //ReportMessage(this, new TaskMessage()
                    //{
                    //    Level = MessageLevel.Warning,
                    //    Text = "尝试为 gamemd.exe 关闭强制映像虚拟化失败。",
                    //});
                }
                else
                {
                    //ReportMessage(this, new TaskMessage()
                    //{
                    //    Level = MessageLevel.Info,
                    //    Text = "成功为 gamemd.exe 关闭了强制映像虚拟化。",
                    //});
                    hasASLRTurnedOffForGamemd = true;
                }
            }

            {
                ConsoleCommandManager.RunConsoleCommand("powershell.exe", "-Command \"((Get-ProcessMitigation -System).ASLR.ForceRelocateImages -eq [Microsoft.Samples.PowerShell.Commands.OPTIONVALUE]::ON) -as [int]\"",
                      out int exitCode, out string stdOut, out string stdErr);

                // don't display stdOut

                if (!string.IsNullOrWhiteSpace(stdErr))
                {
                    ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Warning, Text = stdErr.Trim() });
                }

                if (exitCode != 0)
                {
                    string message = $"进程返回值 {exitCode}。执行失败。";
                    ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Error, Text = message });
                }

                bool stdOutIsNumeric = int.TryParse(stdOut.Trim(), out int stdOutInt);
                if (stdOutIsNumeric && stdOutInt == 1)
                {
                    if (hasASLRTurnedOffForGamemd)
                    {
                        ReportMessage(this, new TaskMessageEventArgs()
                        {
                            Level = MessageLevel.Info,
                            Text = $"强制映像虚拟化 (强制性 ASLR) 为默认开启状态，但已经成功为 {Constants.GameExeName} 关闭了强制映像虚拟化。",
                        });
                    }
                    else
                    {
                        ReportMessage(this, new TaskMessageEventArgs()
                        {
                            Level = MessageLevel.Warning,
                            Text = $"强制映像虚拟化 (强制性 ASLR) 为默认开启状态，并且为 {Constants.GameExeName} 关闭强制映像虚拟化失败。这可能会导致 Ares 无法正常启动。请在 “Windows 安全中心”→“应用和浏览器控制”下找到并关闭“系统设置”选项卡中的“强制映像虚拟化 (强制性 ASLR)”选项，或在“程序设置”选项卡中为游戏文件单独关闭此选项。",
                        });
                    }
                }
                else if (stdOutIsNumeric && stdOutInt == 0)
                {
                    ReportMessage(this, new TaskMessageEventArgs()
                    {
                        Level = MessageLevel.Info,
                        Text = "强制映像虚拟化 (强制性 ASLR) 为默认关闭状态。",
                    });
                }
                else
                {
                    ReportMessage(this, new TaskMessageEventArgs()
                    {
                        Level = MessageLevel.Warning,
                        Text = "未能识别的布尔数字 " + stdOutIsNumeric + "。无法判断选项状态。",
                    });
                }
            }

        }
    }
}
