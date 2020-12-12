//#define SPEEDCONTROL

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Principal;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            TextRange rangeOfText = new TextRange(this.MainTextBox.Document.ContentEnd, this.MainTextBox.Document.ContentEnd)
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
            TextRange rangeOfText = new TextRange(this.MainTextBox.Document.ContentEnd, this.MainTextBox.Document.ContentEnd)
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
            TextRange rangeOfText = new TextRange(this.MainTextBox.Document.ContentEnd, this.MainTextBox.Document.ContentEnd)
            {
                Text = text + Environment.NewLine
            };
            rangeOfText.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Regular);

            // set the current caret position to the end
            this.MainTextBox.CaretPosition = this.MainTextBox.Document.ContentEnd;
            // scroll it automatically
            this.MainTextBox.ScrollToEnd();
        }
        public MainWindow()
        {
            this.InitializeComponent();
        }
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

        struct Resolution
        {
            public int Width;
            public int Height;
        }

        //https://www.codeproject.com/Tips/1184124/Get-Target-Screen-Size-of-the-Hosting-Window-in-WP
        private Resolution GetHostingScreenSize(string devicename)
        {

            var hdc = NativeMethods.CreateDC(devicename, "", "", IntPtr.Zero);

            // Returns Height of the screen
            int DESKTOPVERTRES = NativeMethods.GetDeviceCaps(hdc, (int) DeviceCap.DESKTOPVERTRES);

            // Returns width of the screen
            int DESKTOPHORZRES = NativeMethods.GetDeviceCaps(hdc, (int) DeviceCap.DESKTOPHORZRES);

            // Uncomment this code, if user wants to get additional data.
            // int VERTRES = NativeMethods.GetDeviceCaps(hdc, (int)DeviceCap.VERTRES);
            // int LOGPIXELSY = NativeMethods.GetDeviceCaps(hdc, (int)DeviceCap.LOGPIXELSY);
            // int HORZRES = NativeMethods.GetDeviceCaps(hdc, (int)DeviceCap.HORZRES);
            // LogMessage(string.Format(string.Format("VERTRES : {0}, DESKTOPVERTRES: {1}, LOGPIXELSY: {2}, HORZRES: {3}, DESKTOPHORZRES: {4}",
            //    VERTRES,
            //    DESKTOPVERTRES,
            //    LOGPIXELSY,
            //    HORZRES,
            //    DESKTOPHORZRES));

            return new Resolution() { Width = DESKTOPHORZRES, Height = DESKTOPVERTRES };
        }
        class MainWorkerProgressReport
        {
            public string StdOut = string.Empty;
            public string StdErr = string.Empty;
            public bool UseMessageBoxWarning = false;
        }

        private BackgroundWorker mainWorker = null;

        private void Window_Initialized(object sender, EventArgs e)
        {

            Debug.WriteLine(System.Windows.Forms.Screen.FromHandle(new System.Windows.Interop.WindowInteropHelper(Window.GetWindow(this)).Handle).DeviceName);

            var Resolution = this.GetHostingScreenSize(System.Windows.Forms.Screen.FromHandle(new System.Windows.Interop.WindowInteropHelper(Window.GetWindow(this)).Handle).DeviceName);
            //System.Diagnostics.Debug.WriteLine(this.Resolution.Height);

            mainWorker = new BackgroundWorker()
            {
                WorkerReportsProgress = true
            };

            mainWorker.ProgressChanged += (object worker_sender, ProgressChangedEventArgs worker_e) =>
            {
                var text = worker_e.UserState as MainWorkerProgressReport;
                if (!String.IsNullOrEmpty(text.StdOut))
                {
                    this.MainTextAppendNormal(text.StdOut);
                }
                if (!String.IsNullOrEmpty(text.StdErr))
                {
                    this.MainTextAppendError(text.StdErr);
                }
                if (text.UseMessageBoxWarning)
                {
                    MessageBox.Show(this, text.StdOut + text.StdErr, "消息", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            };


            mainWorker.RunWorkerCompleted += (object worker_sender, RunWorkerCompletedEventArgs worker_e) =>
            {
                if (worker_e.Error is null)
                {
                    this.MainTextAppendNormal("执行完毕。请检查输出文字是否有异常，然后关闭此窗口。");
                    //MessageBox.Show(this, "执行完毕", "执行完毕", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    this.MainTextAppendError(worker_e.Error.Message);
                    MessageBox.Show(this, worker_e.Error.Message, "执行失败", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };


            mainWorker.DoWork += (object worker_sender, DoWorkEventArgs worker_e) =>
            {
                var worker = worker_sender as BackgroundWorker;

                string ExePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                List<string> GameExes = new List<string>()
                {
                    System.IO.Path.Combine(ExePath, "gamemd.exe"),
                    //System.IO.Path.Combine(ExePath, "MentalOmegaClient.exe"),
                    System.IO.Path.Combine(new string[]{ ExePath,"Syringe.exe"}),
                    //System.IO.Path.Combine(new string[]{ ExePath,"Resources","clientdx.exe"}),
                    //System.IO.Path.Combine(new string[]{ ExePath,"Resources","clientogl.exe"}),
                    //System.IO.Path.Combine(new string[]{ ExePath,"Resources","clientxna.exe"}),
                };
                List<string> GameExeWithoutDpiAwareness = new List<string>()
                {
                    System.IO.Path.Combine(ExePath, "MentalOmegaClient.exe"),
                    System.IO.Path.Combine(new string[]{ ExePath,"Resources","clientdx.exe"}),
                    System.IO.Path.Combine(new string[]{ ExePath,"Resources","clientogl.exe"}),
                    System.IO.Path.Combine(new string[]{ ExePath,"Resources","clientxna.exe"}),
                };
                List<string> AvExes = new List<string>()
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

                //检查注册机是否在游戏目录
                if (!System.IO.File.Exists(MainExePath))
                {
                    throw new Exception("注册机可能不在游戏目录。请确保将注册机的文件复制到游戏目录后再执行。找不到 MentalOmegaClient.exe 文件。");
                }

                // 检查路径转换为 ANSI 后是否大于 130 字节
                {
                    if (( Encoding.Convert(Encoding.Unicode, Encoding.Default, Encoding.Unicode.GetBytes(ExePath)) ).Count() > 130)
                    {
                        throw new Exception("当前游戏目录的路径较长。游戏可能无法正常运行。");
                    }
                }
                //注册 blowfish.dll 文件；写入红警2注册表
                worker.ReportProgress(0, new MainWorkerProgressReport() { StdOut = "---- 注册 blowfish.dll 文件，导入 Red Alert 2 安装信息到注册表 ----" });
                {
                    //与原版相同，直接调用原版注册机
                    System.Diagnostics.Process process = new System.Diagnostics.Process()
                    {
                        StartInfo = new System.Diagnostics.ProcessStartInfo()
                        {
                            FileName = System.IO.Path.Combine(ExePath, "ra2reg.exe"),
                            WorkingDirectory = ExePath,
                            RedirectStandardInput = false,
                            RedirectStandardOutput = false,
                            RedirectStandardError = false,
                            UseShellExecute = true,
                            CreateNoWindow = false,
                            WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                        }
                    };
                    try
                    {
                        process.Start();
                        process.WaitForExit();
                        if (process.ExitCode != 0)
                        {
                            throw new Exception("进程返回 " + process.ExitCode + "。执行失败。");
                        }
                    }
                    catch (Exception ex)
                    {
                        worker.ReportProgress(0, new MainWorkerProgressReport() { StdErr = ex.Message });
                    }

                    //string stdout = process.StandardOutput.ReadToEnd();
                    //if (!String.IsNullOrWhiteSpace(stdout))
                    //{
                    //    worker.ReportProgress(0, new MainWorkerProgressReport() { StdOut = stdout });
                    //}

                    //string stderr = process.StandardError.ReadToEnd();
                    //if (!String.IsNullOrWhiteSpace(stderr))
                    //{
                    //    worker.ReportProgress(0, new MainWorkerProgressReport() { StdErr = stderr });
                    //}
                }
                //注册表：设置兼容性：管理员 高DPI感知
                worker.ReportProgress(0, new MainWorkerProgressReport() { StdOut = "---- 设置兼容性：高 DPI 感知、以管理员权限运行 ----" });
                {
                    string compatibilitySetting = "~ RUNASADMIN HIGHDPIAWARE";
                    foreach (var exeName in GameExes)
                    {
                        using (var registryKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers"))
                        {
                            registryKey.SetValue(exeName, compatibilitySetting);
                        }
                    }
                }
                {
                    string compatibilitySetting = "~ RUNASADMIN";
                    foreach (var exeName in GameExeWithoutDpiAwareness)
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
                    //计算路径哈希
                    StringBuilder ExePathHash32 = new StringBuilder();
                    {
                        byte[] bytes = Encoding.UTF8.GetBytes(ExePath);
                        byte[] hash = System.Security.Cryptography.SHA256.Create().ComputeHash(bytes);

                        for (int i = 0; i < Math.Min(hash.Length, 8); i++)
                        {
                            ExePathHash32.Append(hash[i].ToString("X2", CultureInfo.InvariantCulture));
                        }
                    }
                    //删除之前的规则（如果有）
                    {
                        System.Diagnostics.Process process = new System.Diagnostics.Process()
                        {
                            StartInfo = new System.Diagnostics.ProcessStartInfo()
                            {
                                FileName = "netsh.exe",
                                Arguments = "advfirewall firewall delete rule name=\"Mental-Omega-Game-Exception-" + ExePathHash32 + "\" dir=in",
                                RedirectStandardInput = false,
                                RedirectStandardOutput = false,
                                RedirectStandardError = false,
                                UseShellExecute = true,
                                CreateNoWindow = false,
                                WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
                            }
                        };
                        process.Start();
                        process.WaitForExit();

                        //注意，此条不检查返回值。因为允许相关条目不存在。

                        //if (process.ExitCode != 0)
                        //{
                        //    throw new Exception("进程返回值 " + process.ExitCode + "。执行失败。");
                        //} 
                    }

                    foreach (var exeName in GameExes)
                    {
                        System.Diagnostics.Process process = new System.Diagnostics.Process()
                        {
                            StartInfo = new System.Diagnostics.ProcessStartInfo()
                            {
                                FileName = "netsh.exe",
                                Arguments = "advfirewall firewall add rule name=\"Mental-Omega-Game-Exception-" + ExePathHash32 + "\" dir=in action=allow program=\"" + exeName + "\"",
                                RedirectStandardInput = false,
                                RedirectStandardOutput = false,
                                RedirectStandardError = false,
                                UseShellExecute = true,
                                CreateNoWindow = false,
                                WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
                            }
                        };
                        process.Start();
                        process.WaitForExit();

                        if (process.ExitCode != 0)
                        {
                            worker.ReportProgress(0, new MainWorkerProgressReport() { StdErr = "进程返回值 " + process.ExitCode + "。执行失败。" });
                        }
                        //string stdout = process.StandardOutput.ReadToEnd();
                        //if (!String.IsNullOrWhiteSpace(stdout))
                        //{
                        //    worker.ReportProgress(0, new MainWorkerProgressReport() { StdOut = stdout });
                        //}

                        //string stderr = process.StandardError.ReadToEnd();
                        //if (!String.IsNullOrWhiteSpace(stderr))
                        //{
                        //    worker.ReportProgress(0, new MainWorkerProgressReport() { StdErr = stderr });
                        //}
                    }
                }
                //INI：设置分辨率    
                worker.ReportProgress(0, new MainWorkerProgressReport() { StdOut = "---- 设置游戏分辨率为 " + Resolution.Width.ToString(CultureInfo.InvariantCulture) + "×" + Resolution.Height.ToString(CultureInfo.InvariantCulture) + " ----" });
                {
                    var ra2MoIniFile = new MadMilkman.Ini.IniFile();
                    var iniPath = System.IO.Path.Combine(ExePath, "RA2MO.INI");
                    ra2MoIniFile.Load(iniPath);
                    //多屏的朋友，对不住了，只获取当前屏幕
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
                //注意 Environment.OSVersion 只能用于判断系统是 XP、Vista、Win7还是 Win8+，分不出Win8/8.1/10，因为都返回6.2。
                if (Environment.OSVersion.Version.Major >= 6 && Environment.OSVersion.Version.Minor >= 2)
                {
                    worker.ReportProgress(0, new MainWorkerProgressReport() { StdOut = "---- 设置渲染补丁为 TS-DDRAW ----" });
                    {
                        bool success = true;
                        try
                        {
                            System.IO.File.Copy(System.IO.Path.Combine(new string[] { ExePath, "Resources", "ts_ddraw.dll" }), System.IO.Path.Combine(ExePath, "ddraw.dll"), true);
                            System.IO.File.Copy(System.IO.Path.Combine(new string[] { ExePath, "Resources", "ddraw-auto.ini" }), System.IO.Path.Combine(ExePath, "ddraw.ini"), true);
                        }
                        catch (Exception ex)
                        {
                            worker.ReportProgress(0, new MainWorkerProgressReport() { StdErr = "部署渲染补丁时遇到问题。" + ex.Message });
                            success = false;
                        }
                        if (success)
                        {
                            var ra2MoIniFile = new MadMilkman.Ini.IniFile();
                            var iniPath = System.IO.Path.Combine(ExePath, "RA2MO.INI");
                            ra2MoIniFile.Load(iniPath);
                            {
                                var key = this.FindOrNewIniKey(ra2MoIniFile, "Compatibility", "Renderer");
                                key.Value = "TS_DDraw";
                            }
                            ra2MoIniFile.Save(iniPath);
                        }
                    }

                    worker.ReportProgress(0, new MainWorkerProgressReport() { StdOut = "提示：如果有需要，可以从心灵终结客户端内更改渲染补丁设置。" });
                }
                else
                {
                    worker.ReportProgress(0, new MainWorkerProgressReport() { StdOut = "---- 不设置渲染补丁 ----" });

                    worker.ReportProgress(0, new MainWorkerProgressReport() { StdOut = "提示：如果有需要，可以从心灵终结客户端内更改渲染补丁设置。" });
                }

                //#if SPEEDCONTROL
                //                //INI：设置 SPEEDCONTROL
                //                worker.ReportProgress(0, new MainWorkerProgressReport() { StdOut = "---- 6. 设置战役调速 ----" });
                //                {

                //                    worker.ReportProgress(0, new MainWorkerProgressReport() { StdErr = "设置战役调速是一种修改游戏文件的行为。你不能将修改后的游戏用于 SPEEDRUN 速通挑战等用途。如果不需要战役调速，请使用普通版注册机。", UseMessageBoxWarning = true });

                //                    var clientDefinitionIniFile = new MadMilkman.Ini.IniFile();
                //                    var iniPath = System.IO.Path.Combine(new string[] { ExePath, "Resources", "ClientDefinitions.ini" });
                //                    clientDefinitionIniFile.Load(iniPath);
                //                    {
                //                        var key = this.FindOrNewIniKey(clientDefinitionIniFile, "Settings", "ExtraCommandLineParams");
                //                        var options = key.Value.Split(new char[] { ' ' }).ToList();
                //                        for (int i = 0; i < options.Count; ++i)
                //                        {
                //                            options[i] = options[i].ToUpper();
                //                        }
                //                        //注意 Remove 是移除 值 匹配的，不是 Key 匹配的
                //                        // while (options.Remove("-LOG")) { }
                //                        while (options.Remove("-SPEEDCONTROL")) { }
                //                        options.Add("-SPEEDCONTROL");
                //                        var newOptions = new StringBuilder();
                //                        foreach (var option in options)
                //                        {
                //                            newOptions.Append(" " + option);
                //                        }

                //                        key.Value = newOptions.ToString();
                //                    }
                //                    clientDefinitionIniFile.Save(iniPath);
                //                }

                //#endif

                worker.ReportProgress(0, new MainWorkerProgressReport() { StdOut = "---- 设置玩家昵称 ----" });
                {
                    // 从 RA2MO.ini 的 [MultiPlayer] 的 Handle 获取用户名 
                    var ra2MoIniFile = new MadMilkman.Ini.IniFile();
                    var iniPath = System.IO.Path.Combine(ExePath, "RA2MO.INI");
                    ra2MoIniFile.Load(iniPath);
                    var key = this.FindOrNewIniKey(ra2MoIniFile, "MultiPlayer", "Handle");
                    if (String.IsNullOrWhiteSpace(key.Value))
                    {
                        key.Value = GetWindowsUserName();
                    }

                    key.Value = key.Value.Trim();

                    if (!IsAsciiString(key.Value))
                    {
                        worker.ReportProgress(0, new MainWorkerProgressReport() { StdErr = "注意，当前玩家昵称 \"" + key.Value + "\" 包含非 ASCII 字符。如果玩家昵称完全不包含任何 ASCII 字符，Ares 3.0 将会崩溃。" });
                    }

                    key.Value = GetAsciiString(key.Value);

                    // valid ascii char: 32 <= char <=127 ; remove other chars
                    key.Value = new string(key.Value.ToList().Where(c => c >= 32 && c <= 127).ToArray());

                    if (String.IsNullOrWhiteSpace(key.Value))
                        key.Value = "NewPlayer";

                    ra2MoIniFile.Save(iniPath);
                    worker.ReportProgress(0, new MainWorkerProgressReport() { StdOut = "将玩家昵称设置为 \"" + key.Value + "\"。" });
                }



                worker.ReportProgress(0, new MainWorkerProgressReport() { StdOut = "---- 检查网络环境 ----" });
                //检测是否为多网卡环境，弹出局域网联机提示
                Dictionary<string, List<System.Net.IPAddress>> InterfaceIPv4s = new Dictionary<string, List<System.Net.IPAddress>>();

                foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    List<System.Net.IPAddress> IPv4s = new List<System.Net.IPAddress>();
                    if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                    {

                        foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
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
                            ips.Append(Environment.NewLine + kv.Key + " --- " + vv.ToString());
                        }
                    }
                    worker.ReportProgress(0, new MainWorkerProgressReport()
                    {
                        StdErr = "您的电脑有多张网卡，列表如下。与好友面对面作战时，您可能无法在局域网联机大厅中看到其他玩家。要规避这个问题，请在局域网联机时临时禁用其他网卡，仅保留连接到局域网的网卡。"
                        // ips 开头就是换行符
                        + ips
                    });
                }

                ////检测地编兼容性
                //worker.ReportProgress(0, new MainWorkerProgressReport() { StdOut = "---- 8. 检查地图编辑器兼容性 ----" });
                //{
                //    try
                //    {
                //        string finalAlertIniPath = System.IO.Path.Combine(new string[] { ExePath, "Map Editor", "FinalAlert.ini" });
                //        if (System.IO.File.Exists(finalAlertIniPath))
                //            System.IO.File.Delete(finalAlertIniPath);
                //    }
                //    catch (Exception ex)
                //    {
                //        worker.ReportProgress(0, new MainWorkerProgressReport() { StdErr = "删除文件 FinalAlert.ini 时遇到问题。" + ex.Message });
                //    }
                //    //var wrongPath = Encoding.Default.GetString(Encoding.Unicode.GetBytes(ExePath));
                //    ////这样比较不行
                //    //if (!Encoding.Default.GetString(Encoding.Unicode.GetBytes(ExePath)).Equals(ExePath))
                //    //{
                //    //    worker.ReportProgress(0, new MainWorkerProgressReport()
                //    //    {
                //    //        StdErr = "当前游戏目录的路径包含了 ASCII 字符集之外的字符。由于心灵终结客户端本身的问题，自带的地图编辑器将无法使用。" + Environment.NewLine +
                //    //        "以下是实际的游戏目录的路径（UTF-16 LE 编码）：" + ExePath + Environment.NewLine + "由于心灵终结客户端每次都尝试用 UTF-8 编码改写 FinalAlert.ini 文件，而 Final Alert 则总是假定 FinalAlert.ini 是 ANSI 编码，它看起来像这样：" + wrongPath + Environment.NewLine +
                //    //        "因此，建议路径只包含英文字母、数字、普通符号、空格。"
                //    //    });

                //    //}

                //    foreach (char c in ExePath.ToArray())
                //    {
                //        if (( c > 127 ) || ( c < 32 ))
                //        {
                //            worker.ReportProgress(0, new MainWorkerProgressReport() { StdErr = "当前游戏路径中包含字符“" + c.ToString() + "”。由于心灵终结客户端本身的问题，自带的地图编辑器将无法使用。" });

                //            break;
                //        }
                //    }
                //}
                worker.ReportProgress(0, new MainWorkerProgressReport() { StdOut = "---- 检查已安装的运行时组件 ----" });
                worker.ReportProgress(0, new MainWorkerProgressReport() { StdOut = "提示：可以从 https://dotnet.microsoft.com/download 下载最新的 .NET 运行时。" });

                {
                    ////vc++2012 x86
                    //try
                    //{
                    //    using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Classes\Installer\Dependencies\{33d1fd90-4274-48a1-9bc1-97e33d9c2d6f}", false))
                    //    {
                    //        var name = key.GetValue("DisplayName");
                    //        if (name == null)
                    //        {
                    //            worker.ReportProgress(0, new MainWorkerProgressReport() { StdErr = "Microsoft Visual C++ 2012 Redistributable (x86) 未安装 。" });
                    //        }
                    //        else
                    //        {
                    //            worker.ReportProgress(0, new MainWorkerProgressReport() { StdOut = name.ToString() + " 已安装。" });
                    //        }
                    //    };
                    //}
                    //catch (Exception ex)
                    //{
                    //    worker.ReportProgress(0, new MainWorkerProgressReport() { StdErr = "Microsoft Visual C++ 2012 Redistributable (x86) 可能未安装 。" + ex.Message });
                    //}

                    //.net 3.5
                    try
                    {
                        using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v3.5", false))
                        {
                            var name = key.GetValue("Version");
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
                            var versionValue = key.GetValue("Version");
                            if (versionValue == null)
                            {
                                worker.ReportProgress(0, new MainWorkerProgressReport() { StdErr = ".NET Framework 4 未安装。" });
                            }
                            else
                            {
                                worker.ReportProgress(0, new MainWorkerProgressReport() { StdOut = ".NET Framework " + versionValue.ToString() + " 已安装。" });
                            }

                            // 检查 .NET 4.5
                            var installValue = key.GetValue("Release");
                            if (installValue != null)
                            {
                                // https://docs.microsoft.com/en-us/dotnet/framework/migration-guide/how-to-determine-which-versions-are-installed#net_d
                                isDotNet45Installed = ( Convert.ToInt32(installValue.ToString()) >= NET_FRAMEWORK_45_RELEASE_KEY );
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

                //注意 Environment.OSVersion 只能用于判断系统是 XP、Vista、Win7还是 Win8+，分不出Win8/8.1/10，因为都返回6.2。
                {
                    worker.ReportProgress(0, new MainWorkerProgressReport() { StdOut = "---- 检查 Windows 10 游戏栏是否关闭 ----" });
                    bool gameBarEnabled = false;
                    try
                    {
                        using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"System\GameConfigStore", false))
                        {
                            var name = key.GetValue("GameDVR_Enabled");
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
                            var name = key.GetValue("AppCaptureEnabled");
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


                //检测易报毒文件是否还存在，弹出卸载杀毒软件提示
                worker.ReportProgress(0, new MainWorkerProgressReport() { StdOut = "---- 检查游戏文件完整性（粗略） ----" });
                {
                    bool fileAllExists = true;
                    foreach (var avExe in AvExes)
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

            //this.MainTextAppendGreen("此为开发版本，非正式版！开发版本号：201906121840");
            this.MainTextAppendGreen("Mental Omega 3.3.5 注册机");
            this.MainTextAppendGreen("Version: 1.6.1");
            this.MainTextAppendGreen("Author: 伤心的笔");


            mainWorker.RunWorkerAsync();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (( this.mainWorker?.IsBusy ).GetValueOrDefault())
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

        // from https://github.com/CnCNet/dta-mg-client-launcher/
        private static bool IsXNAFramework4Installed()
        {
            try
            {
                RegistryKey HKLM_32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                RegistryKey xnaKey = HKLM_32.OpenSubKey("SOFTWARE\\Microsoft\\XNA\\Framework\\v4.0");

                string installValue = xnaKey.GetValue("Installed").ToString();

                if (installValue == "1")
                    return true;
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
                RegistryKey ndpKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full");

                string installValue = ndpKey.GetValue("Release").ToString();

                // https://docs.microsoft.com/en-us/dotnet/framework/migration-guide/how-to-determine-which-versions-are-installed#net_d
                if (Convert.ToInt32(installValue) >= NET_FRAMEWORK_45_RELEASE_KEY)
                    return true;
            }
            catch
            {

            }

            return false;
        }

        string GetWindowsUserName()
        {
            return WindowsIdentity.GetCurrent().Name.Split(new char[] { '\\' }, 2)[1];
        }
        string GetAsciiString(string str)
        {
            byte[] asciiBytes = System.Text.Encoding.ASCII.GetBytes(str);
            var asciiStr = System.Text.Encoding.ASCII.GetString(asciiBytes);
            return asciiStr;
        }
        bool IsAsciiString(string str)
        {
            var asciiStr = GetAsciiString(str);
            return ( String.Compare(str, asciiStr, false, CultureInfo.InvariantCulture) == 0 );
        }

    }

}
