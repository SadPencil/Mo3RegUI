using Mo3RegUI.LocalizationResources;
using Mo3RegUI.Tasks;
using System;

namespace Mo3RegUI
{
    public class ProcessMitigationTaskParameter : ITaskParameter
    {
        public string GameDir;
    }
    public class ProcessMitigationTask : ITask
    {
        // ProcessMitigationTask_Description: Disable Mandatory Image Virtualization
        public string Description => TextResource.ProcessMitigationTask_Description;
        public event EventHandler<TaskMessageEventArgs> ReportMessage;

        public void DoWork(ITaskParameter p)
        {
            if (p is ProcessMitigationTaskParameter pp)
            {
                this._DoWork(pp);
            }
            else { throw new ArgumentException(); }
        }
        private void _DoWork(ProcessMitigationTaskParameter p)
        {
            if (!(Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= 16299))
            {
                // Task_OsVersionTooLow_NoFeature: Windows version is too low. This feature is not available.
                ReportMessage(this, new TaskMessageEventArgs()
                {
                    Level = MessageLevel.Info,
                    Text = TextResource.Task_OsVersionTooLow_NoFeature,
                });
                return;
            }

            bool hasASLRTurnedOffForGamemd;
            {
                ConsoleCommandManager.RunConsoleCommand("powershell.exe", $"-Command \"Set-ProcessMitigation -Name {Constants.GameExeName} -Disable ForceRelocateImages\"", out int exitCode, out string stdOut, out string stdErr);

                if (!string.IsNullOrWhiteSpace(stdOut))
                {
                    ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Info, Text = stdOut.Trim() });
                }

                if (!string.IsNullOrWhiteSpace(stdErr))
                {
                    ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Warning, Text = stdErr.Trim() });
                }

                hasASLRTurnedOffForGamemd = exitCode == 0;
            }

            {
                ConsoleCommandManager.RunConsoleCommand("powershell.exe", "-Command \"((Get-ProcessMitigation -System).ASLR.ForceRelocateImages -eq [Microsoft.Samples.PowerShell.Commands.OPTIONVALUE]::ON) -as [int]\"",
                      out int exitCode, out string stdOut, out string stdErr);

                // don't display stdOut

                if (!string.IsNullOrWhiteSpace(stdErr))
                {
                    ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Warning, Text = stdErr.Trim() });
                }

                if (exitCode != 0)
                {
                    // Task_ProcessExitCodeFailure: Process returned exit code {0}. Execution failed.
                    string message = string.Format(TextResource.Task_ProcessExitCodeFailure, exitCode);
                    ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Error, Text = message });
                }

                bool stdOutIsNumeric = int.TryParse(stdOut.Trim(), out int stdOutInt);
                if (stdOutIsNumeric && stdOutInt == 1)
                {
                    if (hasASLRTurnedOffForGamemd)
                    {
                        // ProcessMitigationTask_AslrDefaultOnSuccessfullyDisabled: Mandatory image virtualization (mandatory ASLR) is enabled by default, but it has been successfully disabled for {0}.
                        ReportMessage(this, new TaskMessageEventArgs()
                        {
                            Level = MessageLevel.Info,
                            Text = string.Format(TextResource.ProcessMitigationTask_AslrDefaultOnSuccessfullyDisabled, Constants.GameExeName),
                        });
                    }
                    else
                    {
                        // ProcessMitigationTask_AslrDefaultOnFailedToDisable: Mandatory image virtualization (mandatory ASLR) is enabled by default, and disabling it for {0} failed. ...
                        ReportMessage(this, new TaskMessageEventArgs()
                        {
                            Level = MessageLevel.Warning,
                            Text = string.Format(TextResource.ProcessMitigationTask_AslrDefaultOnFailedToDisable, Constants.GameExeName),
                        });
                    }
                }
                else if (stdOutIsNumeric && stdOutInt == 0)
                {
                    // ProcessMitigationTask_AslrDefaultOff: Mandatory image virtualization (mandatory ASLR) is disabled by default.
                    ReportMessage(this, new TaskMessageEventArgs()
                    {
                        Level = MessageLevel.Info,
                        Text = TextResource.ProcessMitigationTask_AslrDefaultOff,
                    });
                }
                else
                {
                    // ProcessMitigationTask_UnrecognizedBoolean: Unrecognized boolean value {0}. Unable to determine option status.
                    ReportMessage(this, new TaskMessageEventArgs()
                    {
                        Level = MessageLevel.Warning,
                        Text = string.Format(TextResource.ProcessMitigationTask_UnrecognizedBoolean, stdOutIsNumeric),
                    });
                }
            }

        }
    }
}
