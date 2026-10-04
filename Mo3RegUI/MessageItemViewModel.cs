using Mo3RegUI.Tasks;
using System.Globalization;

namespace Mo3RegUI
{
    /// <summary>
    /// One line in the message list. Both the text and its category are kept as untranslated
    /// <see cref="LocalizedText"/> / resource keys so that the same item can be shown in the UI
    /// and exported in English afterwards. Messages never change once they have been created, so
    /// the resolved strings are computed on demand instead of being cached.
    /// </summary>
    public class MessageItemViewModel
    {
        /// <summary>Resource key of the name of the task that produced this message.</summary>
        public string CategoryResourceKey { get; }

        public MessageLevel Level { get; }

        /// <summary>The message in a culture-independent form.</summary>
        public LocalizedText MessageText { get; }

        public string Category => this.GetCategory(Localization.CurrentUICulture);

        public string Text => this.GetText(Localization.CurrentUICulture);

        public MessageItemViewModel(string categoryResourceKey, MessageLevel level, LocalizedText text)
        {
            this.CategoryResourceKey = categoryResourceKey;
            this.Level = level;
            this.MessageText = text;
        }

        public string GetCategory(CultureInfo culture) => Localization.GetString(this.CategoryResourceKey, culture);

        public string GetText(CultureInfo culture) => this.MessageText.Resolve(culture);

        public override string ToString() => $"[{this.Level}][{this.Category}]{this.Text}";
    }
}
