using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Principal;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Mo3RegUI
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        private void MainTextAppendError(string text)
        {
            var rangeOfText = new TextRange(this.MainTextBox.Document.ContentEnd, this.MainTextBox.Document.ContentEnd)
            {
                Text = text + Environment.NewLine
            };
            rangeOfText.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Red);
            rangeOfText.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);

            // set the current caret position to the end
            this.MainTextBox.CaretPosition = this.MainTextBox.Document.ContentEnd;
            // scroll it automatically
            this.MainTextBox.ScrollToEnd();

        }
        private void MainTextAppendGreen(string text)
        {
            var rangeOfText = new TextRange(this.MainTextBox.Document.ContentEnd, this.MainTextBox.Document.ContentEnd)
            {
                Text = text + Environment.NewLine
            };
            rangeOfText.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Green);
            rangeOfText.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Regular);

            // set the current caret position to the end
            this.MainTextBox.CaretPosition = this.MainTextBox.Document.ContentEnd;
            // scroll it automatically
            this.MainTextBox.ScrollToEnd();

        }

        private void MainTextAppendNormal(string text)
        {
            var rangeOfText = new TextRange(this.MainTextBox.Document.ContentEnd, this.MainTextBox.Document.ContentEnd)
            {
                Text = text + Environment.NewLine
            };
            rangeOfText.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Regular);

            // set the current caret position to the end
            this.MainTextBox.CaretPosition = this.MainTextBox.Document.ContentEnd;
            // scroll it automatically
            this.MainTextBox.ScrollToEnd();
        }
        public MainWindow() => this.InitializeComponent();
        /// <summary>
        /// 从 INI 文件中找到第一个匹配的 Section 中的匹配的 Key。如果找不到，则创建。
        /// 注意：即使存在多个相同的 Key，也只返回第一个找到的。
        /// </summary>
        /// <param name="iniFile"></param>
        /// <param name="sectionName"></param>
        /// <param name="keyName"></param>
        /// <returns></returns>
        private MadMilkman.Ini.IniKey FindOrNewIniKey(MadMilkman.Ini.IniFile iniFile, string sectionName, string keyName)
        {
            foreach (var section in iniFile.Sections)
            {
                if (section.Name == sectionName)
                {
                    foreach (var key in section.Keys)
                    {
                        if (key.Name == keyName)
                        {
                            return key;
                        }
                    }
                    //Section 下找不到对应的 Key
                    return section.Keys.Add(keyName);
                }
            }
            //找不到 Section
            var newSection = iniFile.Sections.Add(sectionName);
            return newSection.Keys.Add(keyName);

        }

        /// <summary>
        /// 从 INI 文件中找到第一个匹配的 Section 中的匹配的 Key。如果找不到，则返回 null。
        /// 注意：即使存在多个相同的 Key，也只返回第一个找到的。
        /// </summary>
        /// <param name="iniFile"></param>
        /// <param name="sectionName"></param>
        /// <param name="keyName"></param>
        /// <returns></returns>
        private MadMilkman.Ini.IniKey FindIniKey(MadMilkman.Ini.IniFile iniFile, string sectionName, string keyName)
        {
            foreach (var section in iniFile.Sections)
            {
                if (section.Name == sectionName)
                {
                    foreach (var key in section.Keys)
                    {
                        if (key.Name == keyName)
                        {
                            return key;
                        }
                    }
                }
            }
            return null;
        }

        private struct Resolution
        {
            public int Width;
            public int Height;
        }

        private Resolution GetHostingScreenSize()
        {
            // 注意，必须在 manifest 中声明 DPI awareness
            int monitor_width = NativeMethods.GetSystemMetrics(NativeConstants.SM_CXSCREEN);
            int monitor_height = NativeMethods.GetSystemMetrics(NativeConstants.SM_CYSCREEN);

            return new Resolution() { Width = monitor_width, Height = monitor_height };
        }

        private class MainWorkerProgressReport
        {
            public string StdOut = string.Empty;
            public string StdErr = string.Empty;
            public bool UseMessageBoxWarning = false;
        }

        private BackgroundWorker mainWorker = null;

        private void Window_Initialized(object sender, EventArgs e)
        {
            var Resolution = this.GetHostingScreenSize();

            this.mainWorker = new BackgroundWorker()
            {
                WorkerReportsProgress = true
            };

            this.mainWorker.ProgressChanged += (object worker_sender, ProgressChangedEventArgs worker_e) =>
            {
                var text = worker_e.UserState as MainWorkerProgressReport;
                if (!string.IsNullOrEmpty(text.StdOut))
                {
                    this.MainTextAppendNormal(text.StdOut);
                }
                if (!string.IsNullOrEmpty(text.StdErr))
                {
                    this.MainTextAppendError(text.StdErr);
                }
                if (text.UseMessageBoxWarning)
                {
                    _ = MessageBox.Show(this, text.StdOut + text.StdErr, "消息", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            };

            this.mainWorker.RunWorkerCompleted += (object worker_sender, RunWorkerCompletedEventArgs worker_e) =>
            {
                if (worker_e.Error is null)
                {
                    this.MainTextAppendNormal("执行完毕。请检查输出文字是否有异常，然后关闭此窗口。");
                    //MessageBox.Show(this, "执行完毕", "执行完毕", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    this.MainTextAppendError(worker_e.Error.Message);
                    _ = MessageBox.Show(this, worker_e.Error.Message, "执行失败", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };

            this.mainWorker.DoWork += (object worker_sender, DoWorkEventArgs worker_e) =>
            {
                var worker = worker_sender as BackgroundWorker;

                string ExePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                var GameExes = new List<string>()
                {
                    System.IO.Path.Combine(ExePath, "gamemd.exe"),
                    //System.IO.Path.Combine(ExePath, "MentalOmegaClient.exe"),
                    System.IO.Path.Combine(new string[]{ ExePath,"Syringe.exe"}),
                    //System.IO.Path.Combine(new string[]{ ExePath,"Resources","clientdx.exe"}),
                    //System.IO.Path.Combine(new string[]{ ExePath,"Resources","clientogl.exe"}),
                    //System.IO.Path.Combine(new string[]{ ExePath,"Resources","clientxna.exe"}),
                };
                var GameExeWithoutDpiAwareness = new List<string>()
                {
                    System.IO.Path.Combine(ExePath, "MentalOmegaClient.exe"),
                    System.IO.Path.Combine(new string[]{ ExePath,"Resources","clientdx.exe"}),
                    System.IO.Path.Combine(new string[]{ ExePath,"Resources","clientogl.exe"}),
                    System.IO.Path.Combine(new string[]{ ExePath,"Resources","clientxna.exe"}),
                };
                var AvExes = new List<string>()
                {
                    System.IO.Path.Combine(new string[]{ ExePath,"cncnet5.dll"}),
                    System.IO.Path.Combine(new string[]{ ExePath,"cncnet5mo.dll"}),
                    System.IO.Path.Combine(new string[]{ ExePath,"ares.dll"}),
                    System.IO.Path.Combine(new string[]{ ExePath,"Syringe.exe"}),
                    System.IO.Path.Combine(new string[]{ ExePath, "Map Editor", "FinalAlert2MO.exe"}),
                    System.IO.Path.Combine(new string[]{ ExePath, "Map Editor", "Syringe.exe"}),
                    System.IO.Path.Combine(new string[]{ ExePath,"Resources","ddraw_dxwnd.dll"}),
                };

                string MainExePath = System.IO.Path.Combine(ExePath, "MentalOmegaClient.exe");
                string QResPath = System.IO.Path.Combine(ExePath, "qres.dat");
                byte[] QResOldVersionSha2 = HexToByteArray("D9BB2BFA4A3F1FADA6514E1AE7741439C3B85530F519BBABC03B4557B5879138");

                void RunConsoleCommandWithEcho(string command, string argument, out int exitCode, out string stdOut, out string stdErr)
                {
                    RunConsoleCommand(command, argument, out exitCode, out stdOut, out stdErr);
                    if (!string.IsNullOrWhiteSpace(stdOut))
                    {
                        worker.ReportProgress(0, new MainWorkerProgressReport() { StdOut = stdOut.Trim() });
                    }

                    if (!string.IsNullOrWhiteSpace(stdErr))
                    {
                        worker.ReportProgress(0, new MainWorkerProgressReport() { StdErr = stdErr.Trim() });
                    }

                    if (exitCode != 0)
                    {
                        throw new Exception($"进程返回值 { exitCode }。执行失败。");
                    }
                }

                // 开始
                uint codepage = NativeMethods.GetACP();
                worker.ReportProgress(0, new MainWorkerProgressReport() { StdOut = "操作系统: " + Environment.OSVersion.ToString() });
                worker.ReportProgress(0, new MainWorkerProgressReport() { StdOut = "当前 ANSI 代码页: " + codepage.ToString() });

                //检查注册机是否在游戏目录
                if (!System.IO.File.Exists(MainExePath))
                {
                    throw new Exception("注册机可能不在游戏目录。请确保将注册机的文件复制到游戏目录后再执行。找不到 MentalOmegaClient.exe 文件。");
                }

                // .NET Framework and .NET Core
                //OutputEncoding = Encoding.Default;
                //OutputEncoding = CodePagesEncodingProvider.Instance.GetEncoding(System.Globalization.CultureInfo.CurrentCulture.TextInfo.ANSICodePage);

                worker.ReportProgress(0, new MainWorkerProgressReport() { StdOut = "---- 检查游戏目录的路径长度和特殊字符 ----" });
                // 检查路径转换为 ANSI 后是否大于 130 字节
                {
                    if ((Encoding.Convert(Encoding.Unicode, Encoding.Default, Encoding.Unicode.GetBytes(ExePath))).Count() > 130)
                    {
                        throw new Exception("当前游戏目录的路径较长。游戏可能无法正常运行。");
                    }
                }
                // 检查特殊字符
                if (ExePath.Contains(@"%"))
                {
                    worker.ReportProgress(0, new MainWorkerProgressReport() { StdErr = "当前游戏目录的路径包含特殊字符 %（百分号）。Windows 防火墙可能无法正确处理这种情况。", UseMessageBoxWarning = true });
                }
                // 检查默认 ANSI 编码
                if (codepage == 65001)
                {
                    worker.ReportProgress(0, new MainWorkerProgressReport() { StdErr = "当前 ANSI 代码页为 UTF-8。这是一个好做法，但不幸的是，红警 2 游戏中将无法正常输入非英文字符，地图编辑器等组件可能也无法正常显示包含非英文字符的名称，在高 DPI 下游戏界面更可能出现按钮错位、重叠问题。" });
                }

                //注册 blowfish.dll 文件；写入红警2注册表
                worker.ReportProgress(0, new MainWorkerProgressReport() { StdOut = "---- 注册 blowfish.dll 文件，导入 Red Alert 2 安装信息到注册表 ----" });
                {
                    //与原版相同，直接调用原版注册机
                    RunConsoleCommandWithEcho(System.IO.Path.Combine(ExePath, "ra2reg.exe"), string.Empty, out int exitCode, out string stdOut, out string stdErr);
                }
                //注册表：设置兼容性：管理员 高DPI感知
                worker.ReportProgress(0, new MainWorkerProgressReport() { StdOut = "---- 设置兼容性：高 DPI 感知、以管理员权限运行 ----" });
                {
                    string compatibilitySetting = "~ RUNASADMIN HIGHDPIAWARE";
                    foreach (string exeName in GameExes)
                    {
                        using (var registryKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers"))
                        {
                            registryKey.SetValue(exeName, compatibilitySetting);
                        }
                    }
                }
                {
                    string compatibilitySetting = "~ RUNASADMIN";
                    foreach (string exeName in GameExeWithoutDpiAwareness)
                    {
                        using (var registryKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers"))
                        {
                            registryKey.SetValue(exeName, compatibilitySetting);
                        }
                    }
                }

                //添加防火墙例外
                //C:\Windows\System32\netsh.exe advfirewall firewall add rule name="Mental Omega Game Exception" dir=in action=allow program="C:\path\to\exe"
                worker.ReportProgress(0, new MainWorkerProgressReport() { StdOut = "---- 设置高级安全 Windows 防火墙例外项 ----" });
                if (Environment.OSVersion.Version.Major < 6)
                {
                    worker.ReportProgress(0, new MainWorkerProgressReport() { StdErr = "您当前的操作系统 " + Environment.OSVersion.ToString() + " 过于古老，跳过此步。请手动设置 Windows 防火墙例外。" });
                }
                else
                {
                    //计算路径的哈希  
                    string ExePathHash32 = GetExePathHashHex32(ExePath);

                    //删除之前的规则（如果有） 
                    {
                        // 不显示执行结果和报错，因为条目可以不存在
                        RunConsoleCommand("netsh.exe", "advfirewall firewall delete rule name=\"Mental-Omega-Game-Exception-" + ExePathHash32 + "\" dir=in",
                            out _, out _, out _);
                    }

                    foreach (string exeName in GameExes)
                    {
                        RunConsoleCommandWithEcho("netsh.exe", "advfirewall firewall add rule name=\"Mental-Omega-Game-Exception-" + ExePathHash32 + "\" dir=in action=allow program=\"" + exeName + "\"",
                            out int exitCode, out string stdOut, out string stdErr);
                    }
                }
                //INI：设置分辨率    
                worker.ReportProgress(0, new MainWorkerProgressReport() { StdOut = "---- 设置游戏分辨率为 " + Resolution.Width.ToString(CultureInfo.InvariantCulture) + "×" + Resolution.Height.ToString(CultureInfo.InvariantCulture) + " ----" });
                {
                    var ra2MoIniFile = new MadMilkman.Ini.IniFile();
                    string iniPath = System.IO.Path.Combine(ExePath, "RA2MO.INI");
                    ra2MoIniFile.Load(iniPath);
                    //多屏时只获取主屏幕
                    {
                        var key = this.FindOrNewIniKey(ra2MoIniFile, "Video", "ScreenWidth");
                        key.Value = Resolution.Width.ToString(CultureInfo.InvariantCulture);
                    }
                    {
                        var key = this.FindOrNewIniKey(ra2MoIniFile, "Video", "ScreenHeight");
                        key.Value = Resolution.Height.ToString(CultureInfo.InvariantCulture);
                    }
                    ra2MoIniFile.Save(iniPath);
                }

                //INI：设置渲染补丁 TS-DDRAW
                //手动拷贝 TS-DDRAW 的文件，然后再设置 INI
                if (Environment.OSVersion.Version.Major >= 7 || (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor >= 2))
                {
                    worker.ReportProgress(0, new MainWorkerProgressReport() { StdOut = "---- 设置渲染补丁为 CnC_DDraw ----" });
                    {
                        bool success = true;
                        try
                        {
                            System.IO.File.Copy(System.IO.Path.Combine(new string[] { ExePath, "Resources", "cnc-ddraw.dll" }), System.IO.Path.Combine(ExePath, "ddraw.dll"), true);
                            System.IO.File.Copy(System.IO.Path.Combine(new string[] { ExePath, "Resources", "cnc-ddraw.ini" }), System.IO.Path.Combine(ExePath, "ddraw.ini"), true);
                        }
                        catch (Exception ex)
                        {
                            worker.ReportProgress(0, new MainWorkerProgressReport() { StdErr = "部署渲染补丁时遇到问题。" + ex.Message });
                            success = false;
                        }
                        if (success)
                        {
                            var ra2MoIniFile = new MadMilkman.Ini.IniFile();
                            string iniPath = System.IO.Path.Combine(ExePath, "RA2MO.INI");
                            ra2MoIniFile.Load(iniPath);
                            {
                                var key = this.FindOrNewIniKey(ra2MoIniFile, "Compatibility", "Renderer");
                                key.Value = "CnC_DDraw";
                            }
                            ra2MoIniFile.Save(iniPath);
                        }
                    }
                }
                else
                {
                    worker.ReportProgress(0, new MainWorkerProgressReport() { StdOut = "---- 不设置渲染补丁 ----" });
                }
                worker.ReportProgress(0, new MainWorkerProgressReport() { StdOut = "提示：如果有需要，可以从心灵终结客户端内更改渲染补丁设置。" + Environment.NewLine + "在 Windows 8/10/11 系统上建议始终使用现代的渲染补丁，如 TS_DDraw、CnC_DDraw。" });

                // 检查 QRes
                using (var hash = System.Security.Cryptography.SHA256.Create())
                {
                    try
                    {
                        using (var file = new FileStream(QResPath, FileMode.Open))
                        {
                            byte[] digest = hash.ComputeHash(file);
                            if (digest.SequenceEqual(QResOldVersionSha2))
                            {
                                worker.ReportProgress(0, new MainWorkerProgressReport() { StdErr = "检测到未修复的 QRes。在不使用渲染补丁或使用古老的渲染补丁时，在高 DPI 显示器下以窗口化模式运行游戏可能会出现分辨率错误或“Screen mode not found”的错误提示。建议更新 qres.dat 程序或尽可能使用现代的渲染补丁。" });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        worker.ReportProgress(0, new MainWorkerProgressReport() { StdErr = "检测 QRes 失败。" + ex.Message + "在不使用渲染补丁或使用古老的渲染补丁时，以窗口化模式运行游戏可能会出现问题。建议更新 qres.dat 程序或尽可能使用现代的渲染补丁。" });
                    }
                }
                worker.ReportProgress(0, new MainWorkerProgressReport() { StdOut = "---- 设置 CPU 相关性 ----" });
                {

                    //                    worker.ReportProgress(0, new MainWorkerProgressReport() { StdErr = "设置战役调速是一种修改游戏文件的行为。你不能将修改后的游戏用于 SPEEDRUN 速通挑战等用途。如果不需要战役调速，请使用普通版注册机。", UseMessageBoxWarning = true });

                    var clientDefinitionIniFile = new MadMilkman.Ini.IniFile();
                    string iniPath = System.IO.Path.Combine(new string[] { ExePath, "Resources", "ClientDefinitions.ini" });
                    clientDefinitionIniFile.Load(iniPath);
                    {
                        var key = this.FindOrNewIniKey(clientDefinitionIniFile, "Settings", "ExtraCommandLineParams");
                        var options = key.Value.Split(new char[] { ' ' }).ToList();

                        // 删除CPU相关性调试
                        do
                        {
                            int? toBeDeleted = null;
                            for (int i = 0; i < options.Count; ++i)
                            {
                                if (options[i].ToUpper().StartsWith("-AFFINITY:"))
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
                        // 添加相关性
                        int cpuCount = Math.Min(Environment.ProcessorCount, 24);
                        int affinity = (1 << cpuCount) - 1;
                        options.Add("-AFFINITY:" + affinity.ToString(CultureInfo.InvariantCulture));
                        //构建command line
                        var newOptions = new StringBuilder();
                        foreach (string option in options)
                        {
                            _ = newOptions.Append(" " + option);
                        }

                        key.Value = newOptions.ToString();
                    }
                    clientDefinitionIniFile.Save(iniPath);
                }

                //#endif

                worker.ReportProgress(0, new MainWorkerProgressReport() { StdOut = "---- 设置玩家昵称 ----" });
                {
                    // 从 RA2MO.ini 的 [MultiPlayer] 的 Handle 获取用户名 
                    var ra2MoIniFile = new MadMilkman.Ini.IniFile();
                    string iniPath = System.IO.Path.Combine(ExePath, "RA2MO.INI");
                    ra2MoIniFile.Load(iniPath);
                    var key = this.FindOrNewIniKey(ra2MoIniFile, "MultiPlayer", "Handle");
                    if (string.IsNullOrWhiteSpace(key.Value))
                    {
                        key.Value = this.GetWindowsUserName();
                    }

                    key.Value = key.Value.Trim();

                    if (!this.IsAsciiString(key.Value))
                    {
                        worker.ReportProgress(0, new MainWorkerProgressReport() { StdErr = "注意，当前玩家昵称 \"" + key.Value + "\" 包含非 ASCII 字符。如果玩家昵称完全不包含任何 ASCII 字符，Ares 3.0 将会崩溃。" });
                    }

                    key.Value = this.GetAsciiString(key.Value);

                    // valid ascii char: 32 <= char <=127 ; remove other chars
                    key.Value = new string(key.Value.ToList().Where(c => c >= 32 && c <= 127).ToArray());

                    if (string.IsNullOrWhiteSpace(key.Value))
                    {
                        key.Value = "NewPlayer";
                    }

                    ra2MoIniFile.Save(iniPath);
                    worker.ReportProgress(0, new MainWorkerProgressReport() { StdOut = "将玩家昵称设置为 \"" + key.Value + "\"。" });
                }

                worker.ReportProgress(0, new MainWorkerProgressReport() { StdOut = "---- 检查网络环境 ----" });
                //检测是否为多网卡环境，弹出局域网联机提示
                var InterfaceIPv4s = new Dictionary<string, List<System.Net.IPAddress>>();

                foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    var IPv4s = new List<System.Net.IPAddress>();
                    if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                    {

                        foreach (var ip in ni.GetIPProperties().UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            {
                                IPv4s.Add(ip.Address);
                            }
                        }
                    }
                    if (IPv4s.Count > 0)
                    {
                        InterfaceIPv4s.Add(ni.Name, IPv4s);
                    }
                }

                if (InterfaceIPv4s.Count > 1)
                {
                    var ips = new StringBuilder();
                    foreach (var kv in InterfaceIPv4s)
                    {
                        foreach (var vv in kv.Value)
                        {
                            _ = ips.Append(Environment.NewLine + kv.Key + " --- " + vv.ToString());
                        }
                    }
                    worker.ReportProgress(0, new MainWorkerProgressReport()
                    {
                        StdErr = "您的电脑有多张网卡，列表如下。与好友面对面作战时，您可能无法在局域网联机大厅中看到其他玩家。要规避这个问题，请在局域网联机时临时禁用其他网卡，仅保留连接到局域网的网卡。"
                        // ips 开头就是换行符
                        + ips
                    });
                }

                worker.ReportProgress(0, new MainWorkerProgressReport() { StdOut = "---- 检查已安装的运行时组件 ----" });
                worker.ReportProgress(0, new MainWorkerProgressReport() { StdOut = "提示：可以从 https://dotnet.microsoft.com/download 下载最新的 .NET 运行时。" });

                {
                    //.net 3.5
                    try
                    {
                        using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v3.5", false))
                        {
                            object name = key?.GetValue("Version");
                            if (name == null)
                            {
                                worker.ReportProgress(0, new MainWorkerProgressReport() { StdErr = ".NET Framework 3.5 未安装。" });
                            }
                            else
                            {
                                worker.ReportProgress(0, new MainWorkerProgressReport() { StdOut = ".NET Framework " + name.ToString() + " 已安装。" });
                            }
                        };
                    }
                    catch (Exception ex)
                    {
                        worker.ReportProgress(0, new MainWorkerProgressReport() { StdErr = ".NET Framework 3.5 可能未安装。" + ex.Message });
                    }
                    //.net 4.0 full （非client profile）
                    bool isDotNet45Installed = false;
                    try
                    {
                        using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full", false))
                        {
                            object versionValue = key?.GetValue("Version");
                            if (versionValue == null)
                            {
                                worker.ReportProgress(0, new MainWorkerProgressReport() { StdErr = ".NET Framework 4 未安装。" });
                            }
                            else
                            {
                                worker.ReportProgress(0, new MainWorkerProgressReport() { StdOut = ".NET Framework " + versionValue.ToString() + " 已安装。" });
                            }

                            // 检查 .NET 4.5
                            object installValue = key?.GetValue("Release");
                            if (installValue != null)
                            {
                                // https://docs.microsoft.com/en-us/dotnet/framework/migration-guide/how-to-determine-which-versions-are-installed#net_d
                                isDotNet45Installed = (Convert.ToInt32(installValue.ToString()) >= NET_FRAMEWORK_45_RELEASE_KEY);
                            }

                            if (!isDotNet45Installed)
                            {
                                worker.ReportProgress(0, new MainWorkerProgressReport() { StdOut = "当前 .NET Framework 4 的版本号低于 4.5。" });
                            }

                        };
                    }
                    catch (Exception ex)
                    {
                        worker.ReportProgress(0, new MainWorkerProgressReport() { StdErr = ".NET Framework 4 可能未安装。" + ex.Message });
                    }

                    //xna framework
                    bool isXna4Installed = IsXNAFramework4Installed();
                    if (IsXNAFramework4Installed())
                    {
                        worker.ReportProgress(0, new MainWorkerProgressReport() { StdOut = "XNA Framework 4.0 已安装。" });
                    }
                    else
                    {
                        worker.ReportProgress(0, new MainWorkerProgressReport() { StdOut = "XNA Framework 4.0 未安装。" });
                    }

                    if (!isXna4Installed && !isDotNet45Installed)
                    {
                        worker.ReportProgress(0, new MainWorkerProgressReport() { StdErr = "当前 .NET Framework 4 的版本号低于 4.5，且 XNA Framework 4.0 未安装。客户端可能无法正常运行。" });
                    }
                }

                // 游戏栏
                if (Environment.OSVersion.Version.Major >= 10)
                {
                    worker.ReportProgress(0, new MainWorkerProgressReport() { StdOut = "---- 检查 Windows 10/11 游戏栏是否关闭 ----" });
                    bool gameBarEnabled = false;
                    try
                    {
                        using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"System\GameConfigStore", false))
                        {
                            object name = key?.GetValue("GameDVR_Enabled");
                            if (name != null && Convert.ToInt32(name) == 1)
                            {
                                //游戏栏已开启
                                gameBarEnabled = true;
                            }
                            else
                            {
                                //游戏栏可能未开启
                            }
                        };
                    }
                    catch (Exception)
                    {
                        //游戏栏可能未开启 
                    }
                    try
                    {
                        using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\GameDVR", false))
                        {
                            object name = key?.GetValue("AppCaptureEnabled");
                            if (name != null && Convert.ToInt32(name) == 1)
                            {
                                //游戏栏已开启
                                gameBarEnabled = true;
                            }
                            else
                            {
                                //游戏栏可能未开启
                            }
                        };
                    }
                    catch (Exception)
                    {
                        //游戏栏可能未开启 
                    }
                    if (gameBarEnabled)
                    {
                        worker.ReportProgress(0, new MainWorkerProgressReport() { StdErr = "游戏栏已开启，部分电脑和部分渲染补丁下可能会出现游戏部分区域无法点击的情况。请在“开始菜单”→“设置”→“游戏”→“Xbox Game Bar”下找到相关设置，将游戏栏关闭。" });
                    }
                }

                //强制映像虚拟化
                if (Environment.OSVersion.Version.Major >= 10)
                {
                    worker.ReportProgress(0, new MainWorkerProgressReport() { StdOut = "---- 检查 Windows 10/11 强制映像虚拟化 ----" });

                    bool hasASLRTurnedOffForGamemd = false;
                    {
                        RunConsoleCommandWithEcho("powershell.exe", "-Command \"Set-Processmitigation -Name gamemd.exe -Disable ForceRelocateImages\"", out int exitCode, out _, out _);

                        if (exitCode != 0)
                        {
                            worker.ReportProgress(0, new MainWorkerProgressReport() { StdErr = "尝试为 gamemd.exe 关闭强制映像虚拟化失败。" });
                        }
                        else
                        {
                            //worker.ReportProgress(0, new MainWorkerProgressReport() { StdOut = "成功为 gamemd.exe 关闭了强制映像虚拟化。" });
                            hasASLRTurnedOffForGamemd = true;
                        }
                    }

                    {
                        RunConsoleCommand("powershell.exe", "-Command \"((Get-ProcessMitigation -System).ASLR.ForceRelocateImages -eq [Microsoft.Samples.PowerShell.Commands.OPTIONVALUE]::ON) -as [int]\"",
                            out int exitCode, out string stdOut, out string stdErr);

                        if (!string.IsNullOrWhiteSpace(stdErr))
                        {
                            worker.ReportProgress(0, new MainWorkerProgressReport() { StdErr = stdErr.Trim() });
                        }

                        if (exitCode != 0)
                        {
                            worker.ReportProgress(0, new MainWorkerProgressReport() { StdErr = "进程返回 " + exitCode + "。执行失败。" });
                        }

                        bool stdOutIsNumeric = int.TryParse(stdOut.Trim(), out int stdOutInt);
                        if (stdOutIsNumeric && stdOutInt == 1)
                        {
                            if (hasASLRTurnedOffForGamemd)
                            {
                                worker.ReportProgress(0, new MainWorkerProgressReport() { StdOut = "强制映像虚拟化 (强制性 ASLR) 为默认开启状态，但已经成功为 gamemd.exe 关闭了强制映像虚拟化。" });
                            }
                            else
                            {
                                worker.ReportProgress(0, new MainWorkerProgressReport()
                                {
                                    StdErr = "强制映像虚拟化 (强制性 ASLR) 为默认开启状态，并且为 gamemd.exe 关闭强制映像虚拟化失败。这可能会导致 Ares 无法正常启动。请在 “Windows 安全中心”→“应用和浏览器控制”下找到并关闭“系统设置”选项卡中的“强制映像虚拟化 (强制性 ASLR)”选项，或在“程序设置”选项卡中为游戏文件单独关闭此选项。"
                                });
                            }
                        }
                        else if (stdOutIsNumeric && stdOutInt == 0)
                        {
                            //worker.ReportProgress(0, new MainWorkerProgressReport() { StdErr = "强制映像虚拟化 (强制性 ASLR) 为默认关闭状态。" });
                        }
                        else
                        {
                            worker.ReportProgress(0, new MainWorkerProgressReport() { StdErr = "未能识别的布尔数字 " + stdOutIsNumeric + "。无法判断选项状态。" });
                        }
                    }

                }

                //检测易报毒文件是否还存在，弹出卸载杀毒软件提示
                worker.ReportProgress(0, new MainWorkerProgressReport() { StdOut = "---- 检查游戏文件完整性（粗略） ----" });
                {
                    bool fileAllExists = true;
                    foreach (string avExe in AvExes)
                    {
                        fileAllExists &= System.IO.File.Exists(avExe);
                    }

                    if (!fileAllExists)
                    {
                        throw new Exception(
                            "游戏文件不全。请检查杀毒软件日志，将游戏目录添加到杀毒软件白名单中，或者更换其他杀毒软件，然后重新安装游戏。" + Environment.NewLine
                            + "优秀杀毒软件推荐：Windows Defender、火绒安全软件、卡巴斯基、小红伞、ESET NOD32 等。");
                    }
                }

                System.Threading.Thread.Sleep(1500);

            };

            this.MainTextAppendGreen("Mental Omega 3.3.6 注册机");
            this.MainTextAppendGreen("Version: 1.7.5");
            this.MainTextAppendGreen("Author: 伤心的笔");

            this.mainWorker.RunWorkerAsync();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if ((this.mainWorker?.IsBusy).GetValueOrDefault())
            {
                var ret = MessageBox.Show(this,
                    "Mental Omega 注册机正在设置兼容性和配置游戏选项，且尚未运行完毕。确定要中止注册机的运行吗？",
                    "警告", MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation, MessageBoxResult.No);
                if (ret != MessageBoxResult.Yes)
                {
                    e.Cancel = true;
                }
            }
        }

        private static void RunConsoleCommand(string command, string argument, out int exitCode, out string stdOut, out string stdErr)
        {
            var process = new System.Diagnostics.Process()
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = command,
                    Arguments = argument,
                    RedirectStandardInput = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            bool started = process.Start();
            process.WaitForExit();

            stdOut = process.StandardOutput.ReadToEnd();
            stdErr = process.StandardError.ReadToEnd();
            exitCode = process.ExitCode;
        }

        // from https://github.com/CnCNet/dta-mg-client-launcher/
        private static bool IsXNAFramework4Installed()
        {
            try
            {
                var HKLM_32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                var xnaKey = HKLM_32.OpenSubKey("SOFTWARE\\Microsoft\\XNA\\Framework\\v4.0");

                string installValue = xnaKey.GetValue("Installed").ToString();

                if (installValue == "1")
                {
                    return true;
                }
            }
            catch
            {

            }

            return false;
        }

        private const int NET_FRAMEWORK_45_RELEASE_KEY = 378389;
        private static bool IsNetFramework45Installed()
        {
            try
            {
                var ndpKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full");

                string installValue = ndpKey.GetValue("Release").ToString();

                // https://docs.microsoft.com/en-us/dotnet/framework/migration-guide/how-to-determine-which-versions-are-installed#net_d
                if (Convert.ToInt32(installValue) >= NET_FRAMEWORK_45_RELEASE_KEY)
                {
                    return true;
                }
            }
            catch
            {

            }

            return false;
        }

        private string GetWindowsUserName() => WindowsIdentity.GetCurrent().Name.Split(new char[] { '\\' }, 2)[1];

        private string GetAsciiString(string str)
        {
            byte[] asciiBytes = System.Text.Encoding.ASCII.GetBytes(str);
            string asciiStr = System.Text.Encoding.ASCII.GetString(asciiBytes);
            return asciiStr;
        }

        private bool IsAsciiString(string str)
        {
            string asciiStr = this.GetAsciiString(str);
            return (string.Compare(str, asciiStr, false, CultureInfo.InvariantCulture) == 0);
        }

        private static byte[] HexToByteArray(string hex) => Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();

        private static string ByteArrayToHex(byte[] arr)
        {
            var ret = new StringBuilder();
            for (int i = 0; i < arr.Length; i++)
            {
                _ = ret.Append(arr[i].ToString("X2", CultureInfo.InvariantCulture));
            }

            return ret.ToString();
        }

        private static string GetExePathHashHex32(string path)
        {
            byte[] digest;
            using (var hash = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(path);
                digest = System.Security.Cryptography.SHA256.Create().ComputeHash(bytes);
            }

            string exePathHash32 = ByteArrayToHex(digest);
            if (exePathHash32.Length > 16)
            {
                exePathHash32 = exePathHash32.Substring(0, 16);
            }

            return exePathHash32;
        }
    }

}
