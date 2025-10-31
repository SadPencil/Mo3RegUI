using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Path = System.IO.Path;

namespace Mo3RegUI.Tasks
{

    public class ResolutionTaskParameter : ITaskParameter
    {
        public string GameDir;
    }
    public class ResolutionTask : ITask
    {
        public string Description => "设置分辨率";
        public event EventHandler<TaskMessageEventArgs> ReportMessage;

        public void DoWork(ITaskParameter p)
        {
            if (p is ResolutionTaskParameter pp)
            {
                this._DoWork(pp);
            }
            else { throw new ArgumentException(); }
        }
        private void _DoWork(ResolutionTaskParameter p)
        {
            ScreenResolution hostResolution = ScreenResolution.GetDesktopScreenResolution();
            ScreenResolution maxResolution = "1920x1200";
            ScreenResolution fallbackResolutionIfTooLarge = "1920x1080";

            ScreenResolution finalResolution;
            {
                if (maxResolution.Fits(hostResolution))
                    finalResolution = hostResolution;
                else
                    finalResolution = fallbackResolutionIfTooLarge;
            }

            lock (Locks.RA2MO_INI)
            {
                MyIniParserHelper.EditIniFile(Path.Combine(p.GameDir, Constants.GameConfigIniName), ini =>
                {
                    var videoSection = MyIniParserHelper.GetSectionOrNew(ini, "Video");
                    videoSection["ScreenWidth"] = finalResolution.Width.ToString(CultureInfo.InvariantCulture);
                    videoSection["ScreenHeight"] = finalResolution.Height.ToString(CultureInfo.InvariantCulture);
                });
            }
            ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Info, Text = "设置游戏分辨率为 " + finalResolution.Width.ToString() + "×" + finalResolution.Height.ToString() + "。" });
        }
    }
}
