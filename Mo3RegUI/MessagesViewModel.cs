using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Mo3RegUI
{
    public class MessagesViewModel : ObservableCollection<MessageItemViewModel>, INotifyPropertyChanged
    {
        public MessagesViewModel() { }
    }
}
