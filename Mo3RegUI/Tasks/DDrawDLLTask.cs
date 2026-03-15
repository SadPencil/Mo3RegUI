using Microsoft.Win32;
using Mo3RegUI.LocalizationResources;
using System;
using System.Linq;

namespace Mo3RegUI.Tasks
{
    public class DDrawDLLTaskParameter : ITaskParameter
    {
    }
    public class DDrawDLLTask : ITask
    {
        // DDrawDLLTask_Description: Check Renderer Registry Entry
        public string Description => TextResource.DDrawDLLTask_Description;
        public event EventHandler<TaskMessageEventArgs> ReportMessage;

        public void DoWork(ITaskParameter p)
        {
            if (p is DDrawDLLTaskParameter pp)
            {
                this._DoWork(pp);
            }
            else { throw new ArgumentException(); }
        }

        private void _DoWork(DDrawDLLTaskParameter p)
        {
            const string dllName = "ddraw.dll";
            using (var registryKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\KnownDLLs", false))
            {
                object dllItem = registryKey.GetValue(dllName);
                if (dllItem is not null)
                {
                    // DDrawDLLTask_RegistryEntryAbnormal: {0} registry entry is abnormal. {0} should not be in KnownDLLs.
                    ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Warning, Text = string.Format(TextResource.DDrawDLLTask_RegistryEntryAbnormal, dllName) });
                }
                else
                {
                    // DDrawDLLTask_RegistryEntryNormal: {0} registry entry is normal.
                    ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Info, Text = string.Format(TextResource.DDrawDLLTask_RegistryEntryNormal, dllName) });
                }
            }
            using (var registryKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager", true))
            {
                const string keyName = "ExcludeFromKnownDlls";
                object exclusiveDlls = registryKey.GetValue(keyName);
                if (exclusiveDlls is null)
                {
                    registryKey.SetValue(keyName, new string[] { dllName }, RegistryValueKind.MultiString);
                    return;
                }

                var exclusiveDllsKind = registryKey.GetValueKind("ExcludeFromKnownDlls");
                if (exclusiveDllsKind != RegistryValueKind.MultiString)
                {
                    // DDrawDLLTask_ExcludeFromKnownDllsWrongType: ExcludeFromKnownDlls should be of type {0}, but is actually of type {1}.
                    throw new Exception(string.Format(TextResource.DDrawDLLTask_ExcludeFromKnownDllsWrongType, RegistryValueKind.MultiString, exclusiveDllsKind));
                    //string message = $"ExcludeFromKnownDlls 应为 {RegistryValueKind.MultiString} 类型，实际为 {exclusiveDllsKind}。";
                    //ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Critical, Text = message });
                    //return; // throw new Exception(message);
                }

                var exclusiveDllsArray = (exclusiveDlls as string[]).ToList();
                if (!exclusiveDllsArray.Contains(dllName))
                {
                    exclusiveDllsArray.Add(dllName);
                }

                registryKey.SetValue(keyName, exclusiveDllsArray.ToArray(), RegistryValueKind.MultiString);
                // DDrawDLLTask_AddedToExcludeFromKnownDlls: Successfully added {0} to ExcludeFromKnownDlls.
                ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Info, Text = string.Format(TextResource.DDrawDLLTask_AddedToExcludeFromKnownDlls, dllName) });
            }
        }
    }
}
