using Mo3RegUI.LocalizationResources;
using System;
using System.IO;

namespace Mo3RegUI.Tasks
{
    public class ClientVolumeTaskParameter : ITaskParameter
    {
        public string GameDir;
    }
    public class ClientVolumeTask : ITask
    {
        // ClientVolumeTask_Description: Dismiss First Run Dialog
        public string Description => TextResource.ClientVolumeTask_Description;
        public event EventHandler<TaskMessageEventArgs> ReportMessage;

        public void DoWork(ITaskParameter p)
        {
            if (p is ClientVolumeTaskParameter pp)
            {
                this._DoWork(pp);
            }
            else { throw new ArgumentException(); }
        }
        private void _DoWork(ClientVolumeTaskParameter p)
        {
            lock (Locks.RA2MO_INI)
            {
                MyIniParserHelper.EditIniFile(Path.Combine(p.GameDir, Constants.GameConfigIniName), ini =>
                {
                    // The client now relies on IsFurstRun to apply the translation files. Therefore, we don't set it to false anymore.
                    // var optionSection = MyIniParserHelper.GetSectionOrNew(ini, "Options");
                    // optionSection["IsFirstRun"] = "False";

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
