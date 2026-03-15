using Mo3RegUI.LocalizationResources;
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
        // ResolutionTask_Description: Set Resolution
        public string Description => TextResource.ResolutionTask_Description;
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
            // ResolutionTask_DesktopResolution: Desktop resolution detected: {0}×{1}.
            ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Info, Text = string.Format(TextResource.ResolutionTask_DesktopResolution, hostResolution.Width, hostResolution.Height) });

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
            // ResolutionTask_SetGameResolution: Game resolution set to {0}×{1}.
            ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Info, Text = string.Format(TextResource.ResolutionTask_SetGameResolution, finalResolution.Width, finalResolution.Height) });
        }
    }
}
