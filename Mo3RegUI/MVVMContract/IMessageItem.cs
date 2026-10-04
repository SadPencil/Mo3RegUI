using System.Globalization;

namespace Mo3RegUI.MVVMContract
{
    /// <summary>
    /// One line in the message list, as far as the View is concerned. The concrete item lives in
    /// the ViewModel; the View only binds to the members declared here.
    /// </summary>
    /// <remarks>
    /// The text and the category are kept in a culture-independent form so that the very same
    /// message can be shown in the UI and exported in English afterwards. The culture-aware
    /// getters exist for the log exporter, which needs to render the item in a chosen language.
    /// </remarks>
    public interface IMessageItem
    {
        /// <summary>Resource key of the name of the task that produced this message.</summary>
        string CategoryResourceKey { get; }

        MessageLevel Level { get; }

        /// <summary>The category in the culture the UI currently uses.</summary>
        string Category { get; }

        /// <summary>The message in the culture the UI currently uses.</summary>
        string Text { get; }

        string GetCategory(CultureInfo culture);

        string GetText(CultureInfo culture);
    }
}
