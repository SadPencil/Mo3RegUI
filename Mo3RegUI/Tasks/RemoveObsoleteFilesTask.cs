using Mo3RegUI.LocalizationResources;
using System;
using System.IO;

namespace Mo3RegUI.Tasks
{
    public class RemoveObsoleteFilesTaskParameter : ITaskParameter
    {
        public string GameDir;
    }
    public class RemoveObsoleteFilesTask : ITask
    {
        // RemoveObsoleteFilesTask_Description: Remove Obsolete Files
        public string Description => TextResource.RemoveObsoleteFilesTask_Description;
        public event EventHandler<TaskMessageEventArgs> ReportMessage;

        public void DoWork(ITaskParameter p)
        {
            if (p is RemoveObsoleteFilesTaskParameter pp)
            {
                this._DoWork(pp);
            }
            else { throw new ArgumentException(); }
        }
        private void _DoWork(RemoveObsoleteFilesTaskParameter p)
        {
            foreach (string exePath in new string[] {
                    Path.Combine(p.GameDir, "wsock32.dll"),
                })
            {
                if (File.Exists(exePath))
                {
                    try
                    {
                        File.Delete(exePath);
                    }
                    catch (Exception ex)
                    {
                        ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Error, Text = ex.Message });
                    }
                }
            }
        }
    }
}