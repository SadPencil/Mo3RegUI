using Microsoft.VisualBasic.Devices;
using System;

namespace Mo3RegUI.Tasks
{
    public class BasicInfoTaskParameter : ITaskParameter
    {
    }
    public class BasicInfoTask : ITask
    {
        public string Description => "检查系统基本信息";
        public event EventHandler<TaskMessageEventArgs> ReportMessage;

        public void DoWork(ITaskParameter p)
        {
            if (p is BasicInfoTaskParameter pp)
            {
                this._DoWork(pp);
            }
            else { throw new ArgumentException(); }
        }
        private void _DoWork(BasicInfoTaskParameter p)
        {
            var computerInfo = new ComputerInfo();
            uint codepage = NativeMethods.GetACP();

            ReportMessage(this, new TaskMessageEventArgs()
            {
                Level = MessageLevel.Info,
                Text = "操作系统: " + computerInfo.OSFullName + " " + computerInfo.OSVersion
            });
            ReportMessage(this, new TaskMessageEventArgs()
            {
                Level = MessageLevel.Info,
                Text = "当前 ANSI 代码页: " + codepage.ToString()
            });
            ReportMessage(this, new TaskMessageEventArgs()
            {
                Level = MessageLevel.Info,
                Text = string.Format(
                    "物理内存: 总计: {0:0.##} GB，可用: {1:0.##} GB",
                    ((double)computerInfo.TotalPhysicalMemory) / 1024 / 1024 / 1024,
                    ((double)computerInfo.AvailablePhysicalMemory) / 1024 / 1024 / 1024),
            });

        }
    }
}
