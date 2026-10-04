namespace Mo3RegUI.MvvmContract.ViewServices
{
    /// <summary>
    /// The dialogs a ViewModel may ask for. The ViewModel never touches a WPF dialog itself; it
    /// states what it wants to tell or ask the user and the View shows it.
    /// </summary>
    public interface IDialogService
    {
        void ShowInformation(string title, string message);

        void ShowError(string title, string message);

        /// <summary>
        /// Shows a Yes/No/Cancel question with "No" as the default answer. Returns <c>true</c>
        /// only when the user chose "Yes"; both "No" and "Cancel" return <c>false</c>.
        /// </summary>
        bool ConfirmYesNoCancel(string title, string message);

        /// <summary>
        /// Shows a save-file dialog and returns the chosen path, or <c>null</c> when the user
        /// cancelled.
        /// </summary>
        string ShowSaveFileDialog(string title, string filter, string defaultExtension, string defaultFileName);
    }
}
