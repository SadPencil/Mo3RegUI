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
                    var optionSection = MyIniParserHelper.GetSectionOrNew(ini, "Options");
                    optionSection["IsFirstRun"] = "False";

                    var audioSection = MyIniParserHelper.GetSectionOrNew(ini, "Audio");
                    if (audioSection.ContainsKey("ClientVolume"))
                    {
                        bool valueIsNumber = double.TryParse(audioSection["ClientVolume"], out double value);
                        if (!valueIsNumber || value > 1 || value < 0)
                        {
                            audioSection["ClientVolume"] = "0.8"; // The volume must be between 0 and 1. The value shipped with MO 3.3.0 is wrong.
                        }
                    }
                });
            }
        }
    }
}
