using Mo3RegUI.MvvmContract.ViewServices;
using System.Windows;

namespace Mo3RegUI.View.Services
{
    /// <summary>
    /// Shows the dialogs the ViewModel asks for. This is the View side of
    /// <see cref="IDialogService"/>; it contains no decision about what to ask, only how to show
    /// it.
    /// </summary>
    public class DialogService : IDialogService
    {
        public void ShowInformation(string title, string message) =>
            ShowMessageBox(message, title, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);

        public void ShowError(string title, string message) =>
            ShowMessageBox(message, title, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);

        public bool ConfirmYesNoCancel(string title, string message) =>
            ShowMessageBox(message, title, MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation, MessageBoxResult.No) == MessageBoxResult.Yes;

        public string ShowSaveFileDialog(string title, string filter, string defaultExtension, string defaultFileName)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog()
            {
                Title = title,
                Filter = filter,
                DefaultExt = defaultExtension,
                FileName = defaultFileName,
                OverwritePrompt = true,
            };

            var owner = GetOwner();
            return (owner is null ? dialog.ShowDialog() : dialog.ShowDialog(owner)) == true
                ? dialog.FileName
                : null;
        }

        private static MessageBoxResult ShowMessageBox(string message, string title, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult)
        {
            var owner = GetOwner();
            return owner is null
                ? MessageBox.Show(message, title, button, icon, defaultResult)
                : MessageBox.Show(owner, message, title, button, icon, defaultResult);
        }

        private static Window GetOwner() => Application.Current?.MainWindow;
    }
}
