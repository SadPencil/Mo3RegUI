using System;
using System.Globalization;
using System.IO;

namespace Mo3RegUI.Tasks
{
    public class ChinaNetworkTaskParameter : ITaskParameter
    {
        public string GameDir;
    }
    public class ChinaNetworkTask : ITask
    {
        public string Description => "关闭 Discord 功能";
        public event EventHandler<TaskMessageEventArgs> ReportMessage;

        public void DoWork(ITaskParameter p)
        {
            if (p is ChinaNetworkTaskParameter pp)
            {
                this._DoWork(pp);
            }
            else { throw new ArgumentException(); }
        }
        private void _DoWork(ChinaNetworkTaskParameter p)
        {
            if (RegionInfo.CurrentRegion.ThreeLetterISORegionName == "CHN")
            {
                lock (Locks.RA2MO_INI)
                {
                    MyIniParserHelper.EditIniFile(Path.Combine(p.GameDir, Constants.GameConfigIniName), ini =>
                    {
                        var section = MyIniParserHelper.GetSectionOrNew(ini, "MultiPlayer");
                        section["DiscordIntegration"] = "False";
                    });
                }
                ReportMessage(this, new TaskMessageEventArgs()
                {
                    Level = MessageLevel.Info,
                    Text = "成功关闭 Discord 功能。",
                });
            }
            else
            {
                ReportMessage(this, new TaskMessageEventArgs()
                {
                    Level = MessageLevel.Info,
                    Text = "不关闭 Discord 功能。",
                });
            }
        }
    }
}
