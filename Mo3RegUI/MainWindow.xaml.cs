using Mo3RegUI.Tasks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Mo3RegUI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            this.Messages = this.DataContext as MessagesViewModel;
        }

        public readonly MessagesViewModel Messages;

        private class MainWorkerProgressReport
        {
            public string StdOut = string.Empty;
            public string StdErr = string.Empty;
            public bool UseMessageBoxWarning = false;
        }

        private TaskManager mainTaskManager = null;

        private void Window_Initialized(object sender, EventArgs e)
        {
            // Run tasks
            string gameDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            // Ensure the program runs at the game folder
#if !DEBUG
            foreach (string exePath in new string[] {
                    Path.Combine(gameDir, Constants.GameExeName),
                    Path.Combine(gameDir, Constants.LauncherExeName),
                })
            {
                if (!File.Exists(exePath))
                {
                    MessageBox.Show(this,
                        $"{Constants.AppName} 可能不在游戏目录。请确保将 {Constants.AppName} 的文件复制到游戏目录后再执行。找不到 {exePath} 文件。",
                        "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    Environment.Exit(1);
                    return;
                }
            }
#endif

            // Run tasks in parallel
            var tasks = new List<TaskInstance>()
            {
                new TaskInstance(){Task = new BasicInfoTask(), Parameter = new BasicInfoTaskParameter()},
                new TaskInstance(){Task = new RuntimeComponentTask(), Parameter = new RuntimeComponentTaskParameter()},
                new TaskInstance(){Task = new EncodingCheckTask(), Parameter = new EncodingCheckTaskParameter()},
                new TaskInstance(){Task = new PathCheckTask(), Parameter = new PathCheckTaskParameter(){ GameDir = gameDir}},
                new TaskInstance(){Task = new ProcessMitigationTask(), Parameter = new ProcessMitigationTaskParameter(){ GameDir = gameDir}},
                new TaskInstance(){Task = new Ra2RegTask(), Parameter = new Ra2RegTaskParameter(){ GameDir = gameDir}},
                new TaskInstance(){Task = new QResTask(), Parameter = new QResTaskParameter(){ GameDir = gameDir}},
                new TaskInstance(){Task = new FirewallSettingTask(), Parameter = new FirewallSettingTaskParameter(){ GameDir = gameDir}},
                new TaskInstance(){Task = new DDrawDLLTask(), Parameter = new DDrawDLLTaskParameter()},
                new TaskInstance(){Task = new RemoveObsoleteFilesTask(), Parameter = new RemoveObsoleteFilesTaskParameter(){ GameDir = gameDir}},
                new TaskInstance(){Task = new XboxGameBarTask(), Parameter = new XboxGameBarTaskParameter()},
                new TaskInstance(){Task = new CompatibilitySettingTask(), Parameter = new CompatibilitySettingTaskParameter(){ GameDir = gameDir}},
                new TaskInstance(){Task = new AffinityTask(), Parameter = new AffinityTaskParameter(){ GameDir = gameDir}},
                new TaskInstance(){Task = new NetworkInterfaceTask(), Parameter = new NetworkInterfaceTaskParameter()},
                new TaskInstance(){Task = new FalsePositiveTask(), Parameter = new FalsePositiveTaskParameter(){ GameDir = gameDir}},
             };

            if (Constants.DirectXRuntimeTaskEnabled)
            {
                tasks.Add(new TaskInstance() { Task = new DirectXRuntimeTask(), Parameter = new DirectXRuntimeTaskParameter() });
            }
            if (Constants.FirstRunTaskEnabled)
            {
                tasks.Add(new TaskInstance() { Task = new FirstRunTask(), Parameter = new FirstRunTaskParameter() { GameDir = gameDir } });
            }
            if (Constants.ChinaNetworkTaskEnabled)
            {
                tasks.Add(new TaskInstance() { Task = new ChinaNetworkTask(), Parameter = new ChinaNetworkTaskParameter() { GameDir = gameDir } });
            }
            if (Constants.RendererTaskEnabled)
            {
                tasks.Add(new TaskInstance() { Task = new RendererTask(), Parameter = new RendererTaskParameter() { GameDir = gameDir } });
            }
            if (Constants.ResolutionTaskEnabled)
            {
                tasks.Add(new TaskInstance() { Task = new ResolutionTask(), Parameter = new ResolutionTaskParameter() { GameDir = gameDir } });
            }
            if (Constants.UserNameTaskEnabled)
            {
                tasks.Add(new TaskInstance() { Task = new UserNameTask(), Parameter = new UserNameTaskParameter() { GameDir = gameDir } });
            }

            this.mainTaskManager = new TaskManager(tasks);
            this.mainTaskManager.ReportMessage += (task_sender, task_e) =>
            {
                string desc = (task_sender as ITask).Description;
                this.Messages.Add(new MessageItemViewModel(category: desc, level: task_e.Level, text: task_e.Text));
            };
            this.mainTaskManager.TaskCompleted += (manager_sender, task_e) =>
            {
                int waitCount = (manager_sender as TaskManager).WaitCount;

                if (waitCount == 0)
                {
                    this.Title = $"{Constants.AppName}";
                    MessageBox.Show(this, "执行完毕。请仔细阅读深红色的警告和错误文字，再关闭此窗口。", "执行完毕", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    this.Title = $"{Constants.AppName} [ 剩余任务: {waitCount} ]";
                }
            };
            this.mainTaskManager.RunAsync();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if ((this.mainTaskManager?.WaitCount).GetValueOrDefault() > 0)
            {
                var ret = MessageBox.Show(this,
                    $"{Constants.AppName} 正在设置兼容性和配置游戏选项，且尚未运行完毕。确定要中止注册机的运行吗？",
                    "警告", MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation, MessageBoxResult.No);
                if (ret != MessageBoxResult.Yes)
                {
                    e.Cancel = true;
                }
            }
        }

        private void GitHubUrlButton_Click(object sender, RoutedEventArgs e)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo(Constants.RepoUri)
            };
            _ = process.Start();
        }
    }

}
