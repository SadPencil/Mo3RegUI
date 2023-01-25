using System;
using System.IO;
using System.Linq;

namespace Mo3RegUI.Tasks
{
    public class QResTaskParameter : ITaskParameter
    {
        public string GameDir;
    }
    public class QResTask : ITask
    {
        public string Description => "检查 QRes 高 DPI 缩放问题";
        public event EventHandler<TaskMessageEventArgs> ReportMessage;

        public void DoWork(ITaskParameter p)
        {
            if (p is QResTaskParameter pp)
            {
                this._DoWork(pp);
            }
            else { throw new ArgumentException(); }
        }
        private void _DoWork(QResTaskParameter p)
        {
            string QResPath = Path.Combine(p.GameDir, "qres.dat");
            byte[] QResOldVersionSha2 = HexToByteArray("D9BB2BFA4A3F1FADA6514E1AE7741439C3B85530F519BBABC03B4557B5879138");
            using var hash = System.Security.Cryptography.SHA256.Create();
            try
            {
                using var file = new FileStream(QResPath, FileMode.Open);
                byte[] digest = hash.ComputeHash(file);
                if (digest.SequenceEqual(QResOldVersionSha2))
                {
                    ReportMessage(this, new TaskMessageEventArgs()
                    {
                        Level = MessageLevel.Warning,
                        Text = "检测到未修复的 QRes。在不使用渲染补丁或使用古老的渲染补丁时，在高 DPI 显示器下以窗口化模式运行游戏可能会出现分辨率错误或“Screen mode not found”的错误提示。建议更新 qres.dat 程序或尽可能使用现代的渲染补丁。",
                    });

                }
            }
            catch (Exception ex)
            {
                ReportMessage(this, new TaskMessageEventArgs()
                {
                    Level = MessageLevel.Warning,
                    Text = "检测 QRes 失败。" + ex.Message + "在不使用渲染补丁或使用古老的渲染补丁时，以窗口化模式运行游戏可能会出现问题。建议更新 qres.dat 程序或尽可能使用现代的渲染补丁。",
                });

            }
        }
        private static byte[] HexToByteArray(string hex) => Enumerable.Range(0, hex.Length)
            .Where(x => x % 2 == 0)
            .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
            .ToArray();

    }
}
