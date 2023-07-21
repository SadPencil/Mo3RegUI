using System;

namespace Mo3RegUI.Tasks
{
    public class SpeakerNumTaskParameter : ITaskParameter
    {
    }
    public class SpeakerNumTask : ITask
    {
        public string Description => "检查声音输出设备";
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
            var num = NativeMethods.waveOutGetNumDevs();
            if (num == 0)
            {
                throw new Exception("当前系统不存在任何声音输出设备。请连接耳机或音箱后再进入游戏，否则游戏有可能会崩溃。");
            }
            ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Info, Text = $"找到了 {num} 个声音输出设备。" });

        }
    }
}
