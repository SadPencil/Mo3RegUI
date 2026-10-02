using Mo3RegUI.LocalizationResources;
using Mo3RegUI.Tasks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;

namespace Mo3RegUI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            this.Messages = this.DataContext as MessagesViewModel;
            this.SetupSaveLogButtons();
        }

        public readonly MessagesViewModel Messages;

        /// <summary>
        /// The directory the program runs from, which is also the game directory. Captured once
        /// so that the exported log can mention it.
        /// </summary>
        private readonly string gameDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

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
            string gameDir = this.gameDir;

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
                new(){Task = new BasicInfoTask(), Parameter = new BasicInfoTaskParameter()},
                new(){Task = new RuntimeComponentTask(), Parameter = new RuntimeComponentTaskParameter()},
                new(){Task = new EncodingCheckTask(), Parameter = new EncodingCheckTaskParameter()},
                new(){Task = new PathCheckTask(), Parameter = new PathCheckTaskParameter(){ GameDir = gameDir}},
                new(){Task = new ProcessMitigationTask(), Parameter = new ProcessMitigationTaskParameter(){ GameDir = gameDir}},
                new(){Task = new QResTask(), Parameter = new QResTaskParameter(){ GameDir = gameDir}},
                new(){Task = new FirewallSettingTask(), Parameter = new FirewallSettingTaskParameter(){ GameDir = gameDir}},
                new(){Task = new ClientVolumeTask(), Parameter = new ClientVolumeTaskParameter(){ GameDir = gameDir}},
                new(){Task = new UserNameTask(), Parameter = new UserNameTaskParameter(){ GameDir = gameDir}},
                new(){Task = new ResolutionTask(), Parameter = new ResolutionTaskParameter(){ GameDir = gameDir}},
                new(){Task = new RendererTask(), Parameter = new RendererTaskParameter(){ GameDir = gameDir}},
                new(){Task = new SpeakerNumTask(), Parameter = new SpeakerNumTaskParameter()},
                new(){Task = new DDrawDLLTask(), Parameter = new DDrawDLLTaskParameter()},
                new(){Task = new RemoveObsoleteFilesTask(), Parameter = new RemoveObsoleteFilesTaskParameter(){ GameDir = gameDir}},
                new(){Task = new RemoveZoneIdentifierTask(), Parameter = new RemoveZoneIdentifierTaskParameter(){ GameDir = gameDir}},
                new(){Task = new ForegroundLockTimeoutTask(), Parameter = new ForegroundLockTimeoutTaskParameter()},
                new(){Task = new CompatibilitySettingTask(), Parameter = new CompatibilitySettingTaskParameter(){ GameDir = gameDir}},
                new(){Task = new FalsePositiveTask(), Parameter = new FalsePositiveTaskParameter(){ GameDir = gameDir}},
            };

            if (!Constants.SkipNetworkInterfaceCheck)
            {
                tasks.Add(new TaskInstance() { Task = new NetworkInterfaceTask(), Parameter = new NetworkInterfaceTaskParameter() });
            }

            if (Constants.CheckDirectXRuntime)
            {
                tasks.Add(new TaskInstance() { Task = new DirectXRuntimeTask(), Parameter = new DirectXRuntimeTaskParameter() });
            }

            if (!Constants.SkipRa2RegTask)
            {
                tasks.Add(new TaskInstance() { Task = new Ra2RegTask(), Parameter = new Ra2RegTaskParameter() { GameDir = gameDir } });
            }

            if (!Constants.SkipXboxGameBarTask)
            {
                tasks.Add(new TaskInstance() { Task = new XboxGameBarTask(), Parameter = new XboxGameBarTaskParameter() });
            }

            this.mainTaskManager = new TaskManager(tasks);
            this.mainTaskManager.ReportMessage += (task_sender, task_e) =>
            {
                string categoryResourceKey = (task_sender as ITask).DescriptionResourceKey;
                this.Messages.Add(new MessageItemViewModel(categoryResourceKey: categoryResourceKey, level: task_e.Level, text: task_e.Text));
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
                // MainWindow_Closing_Warning_Message: {0} is setting compatibility and configuring game options, and has not finished yet. Are you sure you want to abort?
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

        /// <summary>
        /// An English UI needs a single button because both logs would be identical. Every other
        /// UI language offers an English log next to the one in the current language, since a
        /// support request is usually read in English.
        /// </summary>
        private void SetupSaveLogButtons()
        {
            bool isEnglish = Localization.IsCurrentCultureEnglish;
            this.SaveLogButton.Visibility = isEnglish ? Visibility.Visible : Visibility.Collapsed;
            this.SaveLogEnglishButton.Visibility = isEnglish ? Visibility.Collapsed : Visibility.Visible;
            this.SaveLogCurrentLanguageButton.Visibility = isEnglish ? Visibility.Collapsed : Visibility.Visible;
        }

        private void SaveLogButton_Click(object sender, RoutedEventArgs e) =>
            this.SaveLog(Localization.EnglishCulture);

        private void SaveLogEnglishButton_Click(object sender, RoutedEventArgs e) =>
            this.SaveLog(Localization.EnglishCulture);

        private void SaveLogCurrentLanguageButton_Click(object sender, RoutedEventArgs e) =>
            this.SaveLog(Localization.CurrentUICulture);

        /// <summary>
        /// Writes every message collected so far to a text file in <paramref name="culture"/>.
        /// The text is built after the dialog closes so that messages produced while it was open
        /// are included as well.
        /// </summary>
        private void SaveLog(CultureInfo culture)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog()
            {
                Title = Localization.GetString(nameof(TextResource.MainWindow_SaveLogButton), Localization.CurrentUICulture),
                Filter = Localization.GetString(nameof(TextResource.Log_FileDialogFilter), Localization.CurrentUICulture),
                DefaultExt = ".md",
                FileName = LogExporter.GetDefaultFileName(culture),
                OverwritePrompt = true,
            };

            if (dialog.ShowDialog(this) != true)
            {
                return;
            }

            try
            {
                string text = LogExporter.BuildLogText(this.Messages, culture, this.gameDir);
                // UTF-8 with a byte order mark so that Windows editors detect non-ASCII messages.
                File.WriteAllText(dialog.FileName, text, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    string.Format(Localization.GetString(nameof(TextResource.Log_SaveFailed_Message), Localization.CurrentUICulture), ex.Message),
                    Localization.GetString(nameof(TextResource.Log_SaveFailed_Title), Localization.CurrentUICulture),
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBox.Show(this,
                string.Format(Localization.GetString(nameof(TextResource.Log_Saved_Message), Localization.CurrentUICulture), dialog.FileName),
                Localization.GetString(nameof(TextResource.Log_Saved_Title), Localization.CurrentUICulture),
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

}
