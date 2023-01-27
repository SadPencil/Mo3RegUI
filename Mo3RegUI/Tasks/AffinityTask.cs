using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Mo3RegUI.Tasks
{
    public class AffinityTaskParameter : ITaskParameter
    {
        public string GameDir;
    }
    public class AffinityTask : ITask
    {
        public string Description => "设置多核 CPU 相关性";
        public event EventHandler<TaskMessageEventArgs> ReportMessage;

        public void DoWork(ITaskParameter p)
        {
            if (p is AffinityTaskParameter pp)
            {
                this._DoWork(pp);
            }
            else { throw new ArgumentException(); }
        }
        private void _DoWork(AffinityTaskParameter p)
        {
            // Note: cnc-ddraw comes with "singlecpu=true" which limits the affinity to CPU 0. This is handled in RendererTask.
            const int MAX_THREADS = 24;
            lock (Locks.ClientDefinitions_INI)
            {
                MyIniParserHelper.EditIniFile(Path.Combine(p.GameDir, "Resources", "ClientDefinitions.ini"), ini =>
                {
                    var section = MyIniParserHelper.GetSectionOrNew(ini, "Settings");
                    var options = section["ExtraCommandLineParams"].Split(new char[] { ' ' }).ToList();
                    // Remove -AFFINITY params
                    do
                    {
                        int? toBeDeleted = null;
                        for (int i = 0; i < options.Count; ++i)
                        {
                            if (options[i].ToUpper(CultureInfo.InvariantCulture).StartsWith("-AFFINITY:", StringComparison.Ordinal))
                            {
                                toBeDeleted = i;
                                break;
                            }
                        }
                        if (toBeDeleted != null)
                        {
                            options.RemoveAt(toBeDeleted.Value);
                        }
                        else
                        {
                            break;
                        }
                    } while (true);
                    // Append -AFFINITY params
                    int cpuCount = Math.Min(Environment.ProcessorCount, MAX_THREADS);
                    int affinity = (1 << cpuCount) - 1;
                    options.Add("-AFFINITY:" + affinity.ToString(CultureInfo.InvariantCulture));
                    // Build command line
                    var newOptions = new StringBuilder();
                    foreach (string option in options)
                    {
                        _ = newOptions.Append(" " + option);
                    }

                    section["ExtraCommandLineParams"] = newOptions.ToString();
                });
            }
        }
    }
}
