using Microsoft.VisualBasic.Devices;
using Mo3RegUI.LocalizationResources;
using System;

namespace Mo3RegUI.Tasks
{
    public class BasicInfoTaskParameter : ITaskParameter
    {
    }
    public class BasicInfoTask : ITask
    {
        // BasicInfoTask_Description: Check System Basic Info
        public string Description => TextResource.BasicInfoTask_Description;
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

            // BasicInfoTask_OperatingSystem: Operating System: {0} {1}
            ReportMessage(this, new TaskMessageEventArgs()
            {
                Level = MessageLevel.Info,
                Text = string.Format(TextResource.BasicInfoTask_OperatingSystem, computerInfo.OSFullName, computerInfo.OSVersion)
            });
            // BasicInfoTask_CurrentAnsiCodePage: Current ANSI Code Page: {0}
            ReportMessage(this, new TaskMessageEventArgs()
            {
                Level = MessageLevel.Info,
                Text = string.Format(TextResource.BasicInfoTask_CurrentAnsiCodePage, codepage.ToString())
            });
            // BasicInfoTask_PhysicalMemory: Physical Memory: Total: {0:0.##} GB, Available: {1:0.##} GB
            ReportMessage(this, new TaskMessageEventArgs()
            {
                Level = MessageLevel.Info,
                Text = string.Format(
                    TextResource.BasicInfoTask_PhysicalMemory,
                    ((double)computerInfo.TotalPhysicalMemory) / 1024 / 1024 / 1024,
                    ((double)computerInfo.AvailablePhysicalMemory) / 1024 / 1024 / 1024),
            });

        }
    }
}
