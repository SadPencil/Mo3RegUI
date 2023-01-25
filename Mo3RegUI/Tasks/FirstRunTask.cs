using System;
using System.IO;

namespace Mo3RegUI.Tasks
{
    public class FirstRunTaskParameter : ITaskParameter
    {
        public string GameDir;
    }
    public class FirstRunTask : ITask
    {
        public string Description => "关闭首次运行对话框";
        public event EventHandler<TaskMessageEventArgs> ReportMessage;

        public void DoWork(ITaskParameter p)
        {
            if (p is FirstRunTaskParameter pp)
            {
                this._DoWork(pp);
            }
            else { throw new ArgumentException(); }
        }
        private void _DoWork(FirstRunTaskParameter p)
        {
            lock (Locks.RA2MO_INI)
            {
                MyIniParserHelper.EditIniFile(Path.Combine(p.GameDir, "RA2MO.INI"), ini =>
                {
                    {
                        var section = MyIniParserHelper.GetSectionOrNew(ini, "Options");
                        section["IsFirstRun"] = "False";
                    }
                    {
                        var section = MyIniParserHelper.GetSectionOrNew(ini, "Audio");
                        section["ClientVolume"] = "0.8"; // The number is between 0 to 1. The value shipped with MO 3.3.0 is wrong.
                    }
                });
            }
        }
    }
}
