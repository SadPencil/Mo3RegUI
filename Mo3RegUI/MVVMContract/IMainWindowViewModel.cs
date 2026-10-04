using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace Mo3RegUI.MVVMContract
{
    /// <summary>
    /// Everything the main window is allowed to know about its ViewModel. The View binds to the
    /// observable properties and invokes the commands; it never reaches past this interface into
    /// the concrete ViewModel.
    /// </summary>
    public interface IMainWindowViewModel : INotifyPropertyChanged
    {
        /// <summary>The window title, including the remaining-task counter while tasks run.</summary>
        string WindowTitle { get; }

        /// <summary>
        /// The collected messages. Read-only so that the View cannot change what the ViewModel
        /// owns; it still raises collection-change notifications, which is what the ListView and
        /// the grouping in the View rely on.
        /// </summary>
        ReadOnlyObservableCollection<IMessageItem> Messages { get; }

        /// <summary>Kept disabled until every task has finished, so a log is never incomplete.</summary>
        bool AreSaveLogButtonsEnabled { get; }

        /// <summary>Visible for a UI that is served by the neutral (English) resources.</summary>
        bool IsSaveLogButtonVisible { get; }

        /// <summary>Visible next to the current-language button for a translated UI.</summary>
        bool IsSaveLogEnglishButtonVisible { get; }

        bool IsSaveLogCurrentLanguageButtonVisible { get; }

        /// <summary>Starts the task run. Invoked once by the composition root.</summary>
        ICommand InitializeCommand { get; }

        ICommand SaveLogEnglishCommand { get; }

        ICommand SaveLogCurrentLanguageCommand { get; }

        ICommand OpenRepositoryCommand { get; }
    }
}
