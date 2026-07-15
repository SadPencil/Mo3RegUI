using Mo3RegUI.LocalizationResources;
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
        // QResTask_Description: Check QRes High DPI Scaling Issue
        public string Description => TextResource.QResTask_Description;
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
                    // QResTask_UnfixedQRes: Unpatched QRes detected. When not using a renderer or using an outdated renderer, ...
                    ReportMessage(this, new TaskMessageEventArgs()
                    {
                        Level = MessageLevel.Warning,
                        Text = TextResource.QResTask_UnfixedQRes,
                    });

                }
            }
            catch (Exception ex)
            {
                // QResTask_CheckFailed: QRes check failed. {0}When not using a renderer or using an outdated renderer, ...
                ReportMessage(this, new TaskMessageEventArgs()
                {
                    Level = MessageLevel.Warning,
                    Text = string.Format(TextResource.QResTask_CheckFailed, ex.Message),
                });

            }
        }
        private static byte[] HexToByteArray(string hex) => Enumerable.Range(0, hex.Length)
            .Where(x => x % 2 == 0)
            .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
            .ToArray();

    }
}
