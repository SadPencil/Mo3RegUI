using Mo3RegUI.LocalizationResources;
using System;
using System.IO;

namespace Mo3RegUI.Tasks
{
    public class FalsePositiveTaskParameter : ITaskParameter
    {
        public string GameDir;
    }
    public class FalsePositiveTask : ITask
    {
        // FalsePositiveTask_Description: Check Game Files (Rough)
        public string Description => TextResource.FalsePositiveTask_Description;
        public event EventHandler<TaskMessageEventArgs> ReportMessage;

        public void DoWork(ITaskParameter p)
        {
            if (p is FalsePositiveTaskParameter pp)
            {
                this._DoWork(pp);
            }
            else { throw new ArgumentException(); }
        }
        private void _DoWork(FalsePositiveTaskParameter p)
        {
            foreach (string avExe in Constants.VulnerableAvExes)
            {
                string avExeReplaced = avExe.Replace('/', Path.DirectorySeparatorChar);
                if (!File.Exists(Path.Combine(p.GameDir, avExeReplaced)))
                {
                    // FalsePositiveTask_FileNotFound: Game files are incomplete. File {0} not found. Please check your antivirus software log, ...
                    throw new Exception(string.Format(TextResource.FalsePositiveTask_FileNotFound, avExeReplaced));
                }
            }
        }
    }
}
