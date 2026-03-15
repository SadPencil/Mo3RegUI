using Mo3RegUI.LocalizationResources;
using System;

namespace Mo3RegUI.Tasks
{
    public class SpeakerNumTaskParameter : ITaskParameter
    {
    }
    public class SpeakerNumTask : ITask
    {
        // SpeakerNumTask_Description: Check Audio Output Devices
        public string Description => TextResource.SpeakerNumTask_Description;
        public event EventHandler<TaskMessageEventArgs> ReportMessage;

        public void DoWork(ITaskParameter p)
        {
            if (p is SpeakerNumTaskParameter pp)
            {
                this._DoWork(pp);
            }
            else { throw new ArgumentException(); }
        }
        private void _DoWork(SpeakerNumTaskParameter p)
        {
            uint num = NativeMethods.waveOutGetNumDevs();
            if (num == 0)
            {
                // SpeakerNumTask_NoAudioDevice: No audio output device exists on the current system. ...
            throw new Exception(TextResource.SpeakerNumTask_NoAudioDevice);
            }
            // SpeakerNumTask_AudioDevicesFound: Found {0} audio output device(s).
        ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Info, Text = string.Format(TextResource.SpeakerNumTask_AudioDevicesFound, num) });

        }
    }
}
