using Mo3RegUI.Tasks;
using System.ComponentModel;

namespace Mo3RegUI
{
    public class MessageItemViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _category;
        public string Category
        {
            get => this._category;
            set
            {
                this._category = value;
                this.PropertyChanged(this, new PropertyChangedEventArgs("Category"));
            }
        }

        private MessageLevel _level;
        public MessageLevel Level
        {
            get => this._level;
            set
            {
                this._level = value;
                this.PropertyChanged(this, new PropertyChangedEventArgs("Level"));
            }
        }

        private string _text;
        public string Text
        {
            get => this._text;
            set
            {
                this._text = value;
                this.PropertyChanged(this, new PropertyChangedEventArgs("Text"));
            }
        }

        public MessageItemViewModel(string category, MessageLevel level, string text)
        {
            this._category = category;
            this._level = level;
            this._text = text;
        }

        public override string ToString() => $"[{this.Level}][{this.Category}]{this.Text}";

    }
}
