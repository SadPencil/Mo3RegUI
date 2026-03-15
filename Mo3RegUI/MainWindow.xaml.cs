using Mo3RegUI.LocalizationResources;
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
                    // MainWindow_Not_In_Game_Directory_Title: Error
                    // MainWindow_Not_In_Game_Directory_Message: {0} may not be in the game directory. Please ensure that the files of {0} are copied to the game directory before running. File {1} not found.
                    MessageBox.Show(this,
                        string.Format(TextResource.MainWindow_Not_In_Game_Directory_Message, Constants.AppName, exePath),
                        TextResource.MainWindow_Not_In_Game_Directory_Title, MessageBoxButton.OK, MessageBoxImage.Error);
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
                new TaskInstance(){Task = new FirstRunTask(), Parameter = new FirstRunTaskParameter(){ GameDir = gameDir}},
                new TaskInstance(){Task = new UserNameTask(), Parameter = new UserNameTaskParameter(){ GameDir = gameDir}},
                new TaskInstance(){Task = new ResolutionTask(), Parameter = new ResolutionTaskParameter(){ GameDir = gameDir}},
                new TaskInstance(){Task = new RendererTask(), Parameter = new RendererTaskParameter(){ GameDir = gameDir}},
                new TaskInstance(){Task = new SpeakerNumTask(), Parameter = new SpeakerNumTaskParameter()},
                new TaskInstance(){Task = new DDrawDLLTask(), Parameter = new DDrawDLLTaskParameter()},
                new TaskInstance(){Task = new RemoveObsoleteFilesTask(), Parameter = new RemoveObsoleteFilesTaskParameter(){ GameDir = gameDir}},
                new TaskInstance(){Task = new XboxGameBarTask(), Parameter = new XboxGameBarTaskParameter()},
                new TaskInstance(){Task = new ForegroundLockTimeoutTask(), Parameter = new ForegroundLockTimeoutTaskParameter()},
                new TaskInstance(){Task = new CompatibilitySettingTask(), Parameter = new CompatibilitySettingTaskParameter(){ GameDir = gameDir}},
                new TaskInstance(){Task = new ChinaNetworkTask(), Parameter = new ChinaNetworkTaskParameter(){ GameDir = gameDir}},
                new TaskInstance(){Task = new NetworkInterfaceTask(), Parameter = new NetworkInterfaceTaskParameter()},
                new TaskInstance(){Task = new FalsePositiveTask(), Parameter = new FalsePositiveTaskParameter(){ GameDir = gameDir}},
             };

            if (Constants.CheckDirectXRuntime)
            {
                tasks.Add(new TaskInstance() { Task = new DirectXRuntimeTask(), Parameter = new DirectXRuntimeTaskParameter() });
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
                    // Program_Execution_Complete_Title: Execution Complete
                    // Program_Execution_Complete_Message: Execution complete. Please read the warnings and errors in dark red carefully before closing this window.
                    MessageBox.Show(this, TextResource.Program_Execution_Complete_Message, TextResource.Program_Execution_Complete_Title, MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // MainWindow_Title_With_Wait_Count: {0} [ Remaining Tasks: {1} ]
                    this.Title = string.Format(TextResource.MainWindow_Title_With_Wait_Count, Constants.AppName, waitCount);
                }
            };
            this.mainTaskManager.RunAsync();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if ((this.mainTaskManager?.WaitCount).GetValueOrDefault() > 0)
            {
                // MainWindow_Closing_Warning_Title: Warning
                // MainWindow_Closing_Warning_Message: {0} is setting compatibility and configuring game options, and has not finished yet. Are you sure you want to abort the registry tool?
                var ret = MessageBox.Show(this,
                    string.Format(TextResource.MainWindow_Closing_Warning_Message, Constants.AppName),
                    TextResource.MainWindow_Closing_Warning_Title, MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation, MessageBoxResult.No);
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
