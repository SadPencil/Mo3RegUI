using Mo3RegUI.LocalizationResources;
using System;
using System.IO;

namespace Mo3RegUI.Tasks
{
    public class RendererTaskParameter : ITaskParameter
    {
        public string GameDir;
    }
    public class RendererTask : ITask
    {
        // RendererTask_Description: Set Renderer
        public string Description => TextResource.RendererTask_Description;
        public event EventHandler<TaskMessageEventArgs> ReportMessage;

        public void DoWork(ITaskParameter p)
        {
            if (p is RendererTaskParameter pp)
            {
                this._DoWork(pp);
            }
            else { throw new ArgumentException(); }
        }
        private void _DoWork(RendererTaskParameter p)
        {
            if (Environment.OSVersion.Version.Major >= 7 || (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor >= 2))
            {
                // RendererTask_SetToCnCDDraw: Setting renderer to CnC-DDraw.
                ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Info, Text = TextResource.RendererTask_SetToCnCDDraw });

                // Set "singlecpu=false" to support multi-core. Renderer should not determine the affinity but CnC-DDraw did. So the option is turned off in this task.
                lock (Locks.CnC_DDraw_INI)
                {
                    bool singleCpuNeedsFix = true;
                    MyIniParserHelper.ReadIniFile(Path.Combine(p.GameDir, "Resources", Constants.CnCDDrawIniName), ini =>
                    {
                        var section = MyIniParserHelper.GetSectionOrNew(ini, "ddraw");
                        singleCpuNeedsFix = string.Compare(section["singlecpu"], "false", StringComparison.OrdinalIgnoreCase) != 0;
                    });

                    if (singleCpuNeedsFix)
                    {
                        MyIniParserHelper.EditIniFile(Path.Combine(p.GameDir, "Resources", Constants.CnCDDrawIniName), ini =>
                        {
                            var section = MyIniParserHelper.GetSectionOrNew(ini, "ddraw");
                            section["singlecpu"] = "false";
                        });
                    }
                }

                // Apply CnC-DDraw
                bool success = true;
                try
                {
                    string destDDrawDllPath = Path.Combine(p.GameDir, "ddraw.dll");
                    string destDDrawIniPath = Path.Combine(p.GameDir, "ddraw.ini");

                    // The unblocking task processes the source and the deployed file under these
                    // locks, so serialize each copy with it.
                    lock (Locks.CnC_DDraw_DLL)
                    {
                        lock (Locks.DDraw_DLL)
                        {
                            var destDDrawDllFile = new FileInfo(destDDrawDllPath);
                            if (destDDrawDllFile.Exists && destDDrawDllFile.IsReadOnly)
                            {
                                destDDrawDllFile.IsReadOnly = false;
                            }

                            File.Copy(Path.Combine(p.GameDir, "Resources", Constants.CnCDDrawDllName), destDDrawDllPath, true);
                        }
                    }

                    lock (Locks.CnC_DDraw_INI)
                    {
                        lock (Locks.DDraw_INI)
                        {
                            var destDDrawIniFile = new FileInfo(destDDrawIniPath);
                            if (destDDrawIniFile.Exists && destDDrawIniFile.IsReadOnly)
                            {
                                destDDrawIniFile.IsReadOnly = false;
                            }

                            File.Copy(Path.Combine(p.GameDir, "Resources", Constants.CnCDDrawIniName), destDDrawIniPath, true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // RendererTask_DeploymentError: Problem encountered while deploying renderer. {0}
                    ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Warning, Text = string.Format(TextResource.RendererTask_DeploymentError, ex.Message) });
                    success = false;
                }
                if (success)
                {
                    lock (Locks.RA2MO_INI)
                    {
                        MyIniParserHelper.EditIniFile(Path.Combine(p.GameDir, Constants.GameConfigIniName), ini =>
                        {
                            var section = MyIniParserHelper.GetSectionOrNew(ini, "Compatibility");
                            section["Renderer"] = "CnC_DDraw";
                        });
                    }
                }

            }
            else
            {
                // RendererTask_NoRenderer: Not setting renderer.
                ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Info, Text = TextResource.RendererTask_NoRenderer });
            }

            // RendererTask_Hint: Tip: If needed, renderer settings can be changed from within the {0} client. ...
            ReportMessage(this, new TaskMessageEventArgs()
            {
                Level = MessageLevel.Info,
                Text = string.Format(TextResource.RendererTask_Hint, Constants.GameName)
            });

        }
    }
}
