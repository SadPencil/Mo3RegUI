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
            foreach (string avExe in new string[]
                {
                    Path.Combine(new string[]{ p.GameDir,"cncnet5.dll"}),
                    Path.Combine(new string[]{ p.GameDir, "cncnet5mo.dll"}),
                    Path.Combine(new string[]{ p.GameDir, "ares.dll"}),
                    Path.Combine(new string[]{ p.GameDir, "Blowfish.dll"}),
                    Path.Combine(new string[]{ p.GameDir, "Syringe.exe"}),
                    Path.Combine(new string[]{ p.GameDir, "Map Editor", "FinalAlert2MO.exe"}),
                    Path.Combine(new string[]{ p.GameDir, "Map Editor", "Syringe.exe"}),
                    Path.Combine(new string[]{ p.GameDir, "Resources","ddraw_dxwnd.dll"}),
                })
            {
                if (!File.Exists(avExe))
                {
                    throw new Exception("游戏文件不全。请检查杀毒软件日志，将游戏目录添加到杀毒软件白名单中，或者更换其他杀毒软件，然后重新安装游戏。");
                }
            }
        }
    }
}
