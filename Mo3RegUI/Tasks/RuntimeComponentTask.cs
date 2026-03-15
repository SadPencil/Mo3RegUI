using Microsoft.Win32;
using Mo3RegUI.LocalizationResources;
using System;
using System.Globalization;

namespace Mo3RegUI.Tasks
{
    public class RuntimeComponentTaskParameter : ITaskParameter
    {
    }
    public class RuntimeComponentTask : ITask
    {
        // RuntimeComponentTask_Description: Check Runtime Components
        public string Description => TextResource.RuntimeComponentTask_Description;
        public event EventHandler<TaskMessageEventArgs> ReportMessage;

        public void DoWork(ITaskParameter p)
        {
            if (p is RuntimeComponentTaskParameter pp)
            {
                this._DoWork(pp);
            }
            else { throw new ArgumentException(); }
        }

        private const int NET_FRAMEWORK_4_5_RELEASE_KEY = 378389;
        private const int NET_FRAMEWORK_4_5_1_RELEASE_KEY = 378675;
        private const int NET_FRAMEWORK_4_5_2_RELEASE_KEY = 379893;
        private const int NET_FRAMEWORK_4_6_RELEASE_KEY = 393295;
        private const int NET_FRAMEWORK_4_6_1_RELEASE_KEY = 394254;
        private const int NET_FRAMEWORK_4_6_2_RELEASE_KEY = 394802;
        private const int NET_FRAMEWORK_4_7_RELEASE_KEY = 460798;
        private const int NET_FRAMEWORK_4_7_1_RELEASE_KEY = 461308;
        private const int NET_FRAMEWORK_4_7_2_RELEASE_KEY = 461808;
        private const int NET_FRAMEWORK_4_8_RELEASE_KEY = 528040;
        private const int NET_FRAMEWORK_4_8_1_RELEASE_KEY = 533320;

        private void _DoWork(RuntimeComponentTaskParameter p)
        {
            string net4 = GetInstalledNetFramework4VersionString();
            if (net4 is null)
            {
                ReportMessage(this, new TaskMessageEventArgs()
                {
                    Level = MessageLevel.Error,
                    // RuntimeComponentTask_DotNet4NotInstalled: .NET Framework 4 is not installed.
                    Text = TextResource.RuntimeComponentTask_DotNet4NotInstalled,
                });
            }
            else
            {
                ReportMessage(this, new TaskMessageEventArgs()
                {
                    Level = MessageLevel.Info,
                    // RuntimeComponentTask_DotNet4Installed: .NET Framework {0} is installed.
                    Text = string.Format(TextResource.RuntimeComponentTask_DotNet4Installed, net4),
                });
            }

            int? net45plus = GetInstalledNetFramework45VersionNumber();
            bool isDotNet45Installed = net45plus.GetValueOrDefault() >= NET_FRAMEWORK_4_8_RELEASE_KEY;
            if (!isDotNet45Installed)
            {
                ReportMessage(this, new TaskMessageEventArgs()
                {
                    Level = MessageLevel.Error,
                    // RuntimeComponentTask_DotNet4VersionTooLow: The current .NET Framework 4 version is lower than 4.8.
                    Text = TextResource.RuntimeComponentTask_DotNet4VersionTooLow,
                });
            }

            bool isXna4Installed = IsXNAFramework4Installed();
            if (isXna4Installed)
            {
                ReportMessage(this, new TaskMessageEventArgs()
                {
                    Level = MessageLevel.Info,
                    // RuntimeComponentTask_Xna4Installed: XNA Framework 4.0 is installed.
                    Text = TextResource.RuntimeComponentTask_Xna4Installed,
                });
            }
            else
            {
                ReportMessage(this, new TaskMessageEventArgs()
                {
                    Level = MessageLevel.Info,
                    // RuntimeComponentTask_Xna4NotInstalled: XNA Framework 4.0 is not installed.
                    Text = TextResource.RuntimeComponentTask_Xna4NotInstalled,
                });
            }
        }

        private static int? GetInstalledNetFramework45VersionNumber()
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full", false);
                object installValue = key?.GetValue("Release");
                return installValue != null ? (int)installValue : null;
            }
            catch (Exception)
            {
                return null;
            }
        }
        private static string GetInstalledNetFramework4VersionString()
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full", false);
                object versionValue = key?.GetValue("Version");
                return versionValue == null ? null : versionValue as string;
            }
            catch (Exception)
            {
                return null;
            }
        }
        private static string GetInstalledNetFramework35VersionString()
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v3.5", false);
                object name = key?.GetValue("Version");
                return name == null ? null : name as string;
            }
            catch (Exception)
            {
                return null;
            }
        }
        private static bool IsXNAFramework4Installed()
        {
            try
            {
                var HKLM_32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                var xnaKey = HKLM_32.OpenSubKey(@"SOFTWARE\Microsoft\XNA\Framework\v4.0");
                object installValue = xnaKey.GetValue("Installed");
                return installValue is not null && Convert.ToInt32(installValue, CultureInfo.InvariantCulture) == 1;
            }
            catch
            {
                return false;
            }

        }
    }
}
