using Mo3RegUI.LocalizationResources;
using Mo3RegUI.MVVMContract;
using Mo3RegUI.MVVMContract.Mvvm;
using Mo3RegUI.MVVMContract.ViewServices;
using Mo3RegUI.ViewModel.Tasks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Input;

namespace Mo3RegUI.ViewModel
{
    /// <summary>
    /// The main window's ViewModel. All of the logic that used to live in MainWindow.xaml.cs is
    /// here: deciding which tasks to run, collecting their messages, keeping the save buttons
    /// disabled until the run is complete, exporting the log and asking for confirmation before
    /// the window closes while tasks are still running.
    /// The View is injected with this type only through <see cref="IMainWindowViewModel"/>, and
    /// everything it needs from the View is reached through the interfaces in
    /// <c>Mo3RegUI.MVVMContract.ViewServices</c>.
    /// </summary>
    public class MainWindowViewModel : ObservableObject, IMainWindowViewModel
    {
        private readonly IViewLifecycleService lifecycleService;
        private readonly IDialogService dialogService;
        private readonly IUrlService urlService;
        private readonly MessagesViewModel messages = new MessagesViewModel();

        /// <summary>
        /// The directory the program runs from, which is also the game directory. Captured once
        /// so that the exported log can mention it.
        /// </summary>
        private readonly string gameDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        private TaskManager mainTaskManager;
        private string windowTitle = Constants.AppName;
        private bool areSaveLogButtonsEnabled;

        public MainWindowViewModel(
            IViewLifecycleService lifecycleService,
            IDialogService dialogService,
            IUrlService urlService)
        {
            this.lifecycleService = lifecycleService ?? throw new ArgumentNullException(nameof(lifecycleService));
            this.dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            this.urlService = urlService ?? throw new ArgumentNullException(nameof(urlService));

            this.Messages = new ReadOnlyObservableCollection<IMessageItem>(this.messages);

            this.InitializeCommand = new RelayCommand(this.Initialize);
            this.SaveLogEnglishCommand = new RelayCommand(() => this.SaveLog(Localization.EnglishCulture));
            this.SaveLogCurrentLanguageCommand = new RelayCommand(() => this.SaveLog(Localization.CurrentUICulture));
            this.OpenRepositoryCommand = new RelayCommand(() => this.urlService.OpenUrl(Constants.RepoUri));

            // A UI that is already served by the neutral (English) resources needs a single
            // button because both logs would be identical. Every other UI language offers an
            // English log next to the one in the current language, since a support request is
            // usually read in English.
            bool isNeutral = Localization.IsCurrentCultureNeutral;
            this.IsSaveLogButtonVisible = isNeutral;
            this.IsSaveLogEnglishButtonVisible = !isNeutral;
            this.IsSaveLogCurrentLanguageButtonVisible = !isNeutral;

            this.lifecycleService.Closing += this.Window_Closing;
        }

        public string WindowTitle
        {
            get => this.windowTitle;
            private set => this.SetProperty(ref this.windowTitle, value, nameof(this.WindowTitle));
        }

        public ReadOnlyObservableCollection<IMessageItem> Messages { get; }

        public bool AreSaveLogButtonsEnabled
        {
            get => this.areSaveLogButtonsEnabled;
            private set => this.SetProperty(ref this.areSaveLogButtonsEnabled, value, nameof(this.AreSaveLogButtonsEnabled));
        }

        public bool IsSaveLogButtonVisible { get; }

        public bool IsSaveLogEnglishButtonVisible { get; }

        public bool IsSaveLogCurrentLanguageButtonVisible { get; }

        public ICommand InitializeCommand { get; }

        public ICommand SaveLogEnglishCommand { get; }

        public ICommand SaveLogCurrentLanguageCommand { get; }

        public ICommand OpenRepositoryCommand { get; }

        /// <summary>
        /// Validates the game directory and starts the task run. Invoked once by the composition
        /// root right after the window has been shown, so that every dialog has a window to own it.
        /// </summary>
        private void Initialize()
        {
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
                    this.dialogService.ShowError(
                        TextResource.MainWindow_Not_In_Game_Directory_Title,
                        string.Format(TextResource.MainWindow_Not_In_Game_Directory_Message, Constants.AppName, exePath));
                    this.lifecycleService.Shutdown(1);
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
                this.messages.Add(new MessageItemViewModel(categoryResourceKey: categoryResourceKey, level: task_e.Level, text: task_e.Text));
            };
            this.mainTaskManager.TaskCompleted += (manager_sender, task_e) =>
            {
                int waitCount = (manager_sender as TaskManager).WaitCount;
                this.UpdateSaveLogButtonsEnabled();

                if (waitCount == 0)
                {
                    this.WindowTitle = Constants.AppName;
                    // Program_Execution_Complete_Title: Execution Complete
                    // Program_Execution_Complete_Message: Execution complete. Please read the warnings and errors in dark red carefully before closing this window.
                    this.dialogService.ShowInformation(TextResource.Program_Execution_Complete_Title, TextResource.Program_Execution_Complete_Message);
                }
                else
                {
                    // MainWindow_Title_With_Wait_Count: {0} [ Remaining Tasks: {1} ]
                    this.WindowTitle = string.Format(TextResource.MainWindow_Title_With_Wait_Count, Constants.AppName, waitCount);
                }
            };
            this.mainTaskManager.RunAsync();
            this.UpdateSaveLogButtonsEnabled();
        }

        /// <summary>
        /// Raised by the View through <see cref="IViewLifecycleService"/> while the window closes.
        /// Aborting a running task run is a decision, so it is made here and reported back through
        /// <see cref="CancelEventArgs.Cancel"/>.
        /// </summary>
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if ((this.mainTaskManager?.WaitCount).GetValueOrDefault() > 0)
            {
                // MainWindow_Closing_Warning_Title: Warning
                // MainWindow_Closing_Warning_Message: {0} is setting compatibility and configuring game options, and has not finished yet. Are you sure you want to abort?
                bool abort = this.dialogService.ConfirmYesNoCancel(
                    TextResource.MainWindow_Closing_Warning_Title,
                    string.Format(TextResource.MainWindow_Closing_Warning_Message, Constants.AppName));
                if (!abort)
                {
                    e.Cancel = true;
                }
            }
        }

        /// <summary>
        /// Whether every task has finished. The log may only be saved afterwards, otherwise it
        /// would quietly miss the messages of the tasks that are still running.
        /// </summary>
        private bool IsExecutionComplete => this.mainTaskManager is not null && this.mainTaskManager.WaitCount == 0;

        /// <summary>
        /// Keeps the save buttons enabled only once every task has finished. Starting disabled is
        /// deliberate: the buttons must never produce an incomplete log.
        /// </summary>
        private void UpdateSaveLogButtonsEnabled() => this.AreSaveLogButtonsEnabled = this.IsExecutionComplete;

        /// <summary>
        /// Writes every message collected so far to a text file in <paramref name="culture"/>.
        /// The text is built after the dialog closes so that messages produced while it was open
        /// are included as well.
        /// </summary>
        private void SaveLog(CultureInfo culture)
        {
            if (!this.IsExecutionComplete)
            {
                throw new InvalidOperationException("The log cannot be saved before every task has finished; the save buttons stay disabled until then.");
            }

            string fileName = this.dialogService.ShowSaveFileDialog(
                Localization.GetString(nameof(TextResource.MainWindow_SaveLogButton), Localization.CurrentUICulture),
                Localization.GetString(nameof(TextResource.Log_FileDialogFilter), Localization.CurrentUICulture),
                ".md",
                LogExporter.GetDefaultFileName(culture));

            if (fileName is null)
            {
                return;
            }

            try
            {
                string text = LogExporter.BuildLogText(this.Messages, culture, this.gameDir);
                // UTF-8 with a byte order mark so that Windows editors detect non-ASCII messages.
                File.WriteAllText(fileName, text, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
            }
            catch (Exception ex)
            {
                this.dialogService.ShowError(
                    Localization.GetString(nameof(TextResource.Log_SaveFailed_Title), Localization.CurrentUICulture),
                    string.Format(Localization.GetString(nameof(TextResource.Log_SaveFailed_Message), Localization.CurrentUICulture), ex.Message));
                return;
            }

            this.dialogService.ShowInformation(
                Localization.GetString(nameof(TextResource.Log_Saved_Title), Localization.CurrentUICulture),
                string.Format(Localization.GetString(nameof(TextResource.Log_Saved_Message), Localization.CurrentUICulture), fileName));
        }
    }
}
