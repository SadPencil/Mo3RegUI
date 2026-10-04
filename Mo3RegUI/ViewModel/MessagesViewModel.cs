using Mo3RegUI.MVVMContract;
using System.Collections.ObjectModel;

namespace Mo3RegUI.ViewModel
{
    /// <summary>
    /// The mutable collection of messages that <see cref="MainWindowViewModel"/> fills while the
    /// tasks run. The View only ever sees it through the read-only
    /// <see cref="IMainWindowViewModel.Messages"/> property.
    /// </summary>
    public class MessagesViewModel : ObservableCollection<IMessageItem>
    {
        public MessagesViewModel() { }
    }
}
