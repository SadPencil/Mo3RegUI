using Mo3RegUI.MVVMContract;
using Mo3RegUI.View;
using Mo3RegUI.View.Services;
using Mo3RegUI.ViewModel;
using System.Windows;

namespace Mo3RegUI.Exe
{
    /// <summary>
    /// The composition root and the only place that knows both the View and the ViewModel. The
    /// View never creates its ViewModel and the ViewModel never creates a View; this class
    /// creates both, hands the ViewModel the View-side services it is allowed to use, and then
    /// connects the window lifecycle before starting the task run through a command.
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Implemented on the View side; the ViewModel only ever sees the contract interfaces.
            var lifecycleService = new ViewLifecycleService();
            var dialogService = new DialogService();
            var urlService = new UrlService();

            IMainWindowViewModel viewModel = new MainWindowViewModel(lifecycleService, dialogService, urlService);

            var window = new MainWindow(viewModel);
            lifecycleService.Attach(window);

            this.MainWindow = window;
            // Show the window before starting the tasks so that every message box the ViewModel
            // may have to show already has an owner.
            window.Show();

            viewModel.InitializeCommand.Execute(null);
        }
    }
}
