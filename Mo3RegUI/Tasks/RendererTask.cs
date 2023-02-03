using System;
using System.IO;

namespace Mo3RegUI.Tasks
{
    public class RendererTaskParameter : ITaskParameter
    {
        public string GameDir;
    }
    public class RendererTask : ITask
    {
        public string Description => "设置渲染补丁";
        public event EventHandler<TaskMessageEventArgs> ReportMessage;

        public void DoWork(ITaskParameter p)
        {
            if (p is RendererTaskParameter pp)
            {
                this._DoWork(pp);
            }
            else { throw new ArgumentException(); }
        }
        private void _DoWork(RendererTaskParameter p)
        {
            if (Environment.OSVersion.Version.Major >= 7 || (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor >= 2))
            {
                ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Info, Text = "设置渲染补丁为 CnC-DDraw。" });

                // Set "singlecpu=false" to support multi-core. Renderer should not determine the affinity but CnC-DDraw did. So the option is turned off in this task.
                lock (Locks.CnC_DDraw_INI)
                {
                    MyIniParserHelper.EditIniFile(Path.Combine(p.GameDir, "Resources", Constants.CnCDDrawIniName), ini =>
                    {
                        var section = MyIniParserHelper.GetSectionOrNew(ini, "ddraw");
                        section["singlecpu"] = "false";
                    });
                }

                // Apply CnC-DDraw
                bool success = true;
                try
                {
                    File.Copy(Path.Combine(p.GameDir, "Resources", Constants.CnCDDrawDllName), Path.Combine(p.GameDir, "ddraw.dll"), true);
                    File.Copy(Path.Combine(p.GameDir, "Resources", Constants.CnCDDrawIniName), Path.Combine(p.GameDir, "ddraw.ini"), true);
                }
                catch (Exception ex)
                {
                    ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Warning, Text = "部署渲染补丁时遇到问题。" + ex.Message });
                    success = false;
                }
                if (success)
                {
                    lock (Locks.RA2MO_INI)
                    {
                        MyIniParserHelper.EditIniFile(Path.Combine(p.GameDir, Constants.GameConfigIniName), ini =>
                        {
                            var section = MyIniParserHelper.GetSectionOrNew(ini, "Compatibility");
                            section["Renderer"] = "CnC_DDraw";
                        });
                    }
                }

            }
            else
            {
                ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Info, Text = "不设置渲染补丁。" });
            }

            ReportMessage(this, new TaskMessageEventArgs()
            {
                Level = MessageLevel.Info,
                Text = $"提示：如果有需要，可以从 {Constants.GameName} 客户端内更改渲染补丁设置。\n在 Windows 8/10/11 系统上建议始终使用现代的渲染补丁，如 TS-DDraw、CnC-DDraw。"
            });

        }
    }
}
