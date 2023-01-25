using System;
using System.Linq;
using System.Text;

namespace Mo3RegUI.Tasks
{
    public class PathCheckTaskParameter : ITaskParameter
    {
        public string GameDir;
    }
    public class PathCheckTask : ITask
    {
        public string Description => "检查游戏路径格式";
        public event EventHandler<TaskMessageEventArgs> ReportMessage;

        public void DoWork(ITaskParameter p)
        {
            if (p is PathCheckTaskParameter pp)
            {
                this._DoWork(pp);
            }
            else { throw new ArgumentException(); }
        }
        private void _DoWork(PathCheckTaskParameter p)
        {
            // Make sure path length is smaller than 130 bytes (after converting to ANSI)
            if (Encoding.Convert(Encoding.Unicode, Encoding.Default, Encoding.Unicode.GetBytes(p.GameDir)).Count() > 130)
            {
                throw new Exception("当前游戏目录的路径较长。游戏可能无法正常运行。");
                //string message = "当前游戏目录的路径较长。游戏可能无法正常运行。";
                //ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Critical, Text = message });
                //return; // throw new Exception(message);
            }

            // Make sure path does not contain "%"
            if (p.GameDir.Contains(@"%"))
            {
                string message = "当前游戏目录的路径包含特殊字符 %（百分号）。Windows 防火墙可能无法正确处理这种情况。";
                ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Error, Text = message });
            }
        }
    }
}
