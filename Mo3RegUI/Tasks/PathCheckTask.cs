using Mo3RegUI.LocalizationResources;
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
        // PathCheckTask_Description: Check Game Path Format
        public string Description => TextResource.PathCheckTask_Description;
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
                // PathCheckTask_PathTooLong: The current game directory path is too long. The game may not run normally.
                throw new Exception(TextResource.PathCheckTask_PathTooLong);
                //string message = "当前游戏目录的路径较长。游戏可能无法正常运行。";
                //ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Critical, Text = message });
                //return; // throw new Exception(message);
            }

            // Make sure path does not contain "%"
            if (p.GameDir.Contains(@"%"))
            {
                // PathCheckTask_PathContainsPercent: The current game directory path contains the special character % (percent sign). Windows Firewall may not handle this correctly.
                string message = TextResource.PathCheckTask_PathContainsPercent;
                ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Error, Text = message });
            }
        }
    }
}
