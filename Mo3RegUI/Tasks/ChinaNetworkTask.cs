using Mo3RegUI.LocalizationResources;
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
        // ChinaNetworkTask_Description: Disable Discord
        public string Description => TextResource.ChinaNetworkTask_Description;
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
                // ChinaNetworkTask_DiscordDisabled: Discord has been successfully disabled.
                ReportMessage(this, new TaskMessageEventArgs()
                {
                    Level = MessageLevel.Info,
                    Text = TextResource.ChinaNetworkTask_DiscordDisabled,
                });
            }
            else
            {
                // ChinaNetworkTask_DiscordNotDisabled: Discord is not disabled.
                ReportMessage(this, new TaskMessageEventArgs()
                {
                    Level = MessageLevel.Info,
                    Text = TextResource.ChinaNetworkTask_DiscordNotDisabled,
                });
            }
        }
    }
}
