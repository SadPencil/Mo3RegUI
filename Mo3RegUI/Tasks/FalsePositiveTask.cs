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
        public string Description => "游戏文件检查（粗略）";
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
                    throw new Exception($"游戏文件不全。找不到 {avExeReplaced} 文件。请检查杀毒软件日志，将游戏目录添加到杀毒软件白名单中，或者更换其他杀毒软件，然后重新安装游戏。");
                }
            }
        }
    }
}
