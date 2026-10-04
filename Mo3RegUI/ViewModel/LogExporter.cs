using Mo3RegUI.LocalizationResources;
using Mo3RegUI.MVVMContract;
using Mo3RegUI.ViewModel.Tasks;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Mo3RegUI.ViewModel
{
    /// <summary>
    /// Renders every message produced by the program as a Markdown log that a player can attach
    /// to a support request. One culture is passed in so that the very same messages can be
    /// dumped in English as well as in the language the UI is currently using.
    /// </summary>
    public static class LogExporter
    {
        /// <summary>
        /// Builds the whole log. Formatting is done with <paramref name="culture"/> only, never
        /// with the UI culture, so an English export stays English even on a translated system.
        /// </summary>
        public static string BuildLogText(IEnumerable<IMessageItem> messages, CultureInfo culture, string gameDir)
        {
            var items = new List<IMessageItem>(messages);

            var sb = new StringBuilder();

            _ = sb.Append("# ")
              .Append(Localization.GetString(nameof(TextResource.Constants_AppName), culture))
              .Append(' ')
              .Append(Constants.Version)
              .Append(" — ")
              .AppendLine(Localization.GetString(nameof(TextResource.Log_HeaderTitle), culture));
            _ = sb.AppendLine();

            AppendMetadata(sb, nameof(TextResource.Log_HeaderGeneratedAt), culture,
                DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
            AppendMetadata(sb, nameof(TextResource.Log_HeaderLanguage), culture, GetLanguageName(culture));
            AppendMetadata(sb, nameof(TextResource.Log_HeaderGameDirectory), culture, gameDir ?? string.Empty);
            AppendMetadata(sb, nameof(TextResource.Log_HeaderSummary), culture, GetSummaryArgs(items));
            _ = sb.AppendLine();

            // Tasks run in parallel, so a purely chronological list interleaves unrelated
            // sections. Grouping by task keeps each section together: GroupBy yields the groups
            // in the order in which their first message appeared, and keeps the messages inside
            // a group in their original order.
            foreach (var group in items.GroupBy(item => item.CategoryResourceKey))
            {
                _ = sb.AppendLine(Format(nameof(TextResource.Log_SectionHeading), culture,
                    Localization.GetString(group.Key, culture)));

                foreach (var item in group)
                {
                    string text = NormalizeLineBreaks(item.GetText(culture));
                    // One list item per message. A single newline inside a paragraph only renders
                    // as a space in Markdown, so the bullet is what keeps two consecutive
                    // messages on separate lines. The continuation lines of a multi-line message
                    // are indented to stay inside the same item.
                    // Anything more severe than an ordinary note gets its severity marker bolded,
                    // so warnings and errors stand out when the log is rendered.
                    if (item.Level > MessageLevel.Info)
                    {
                        sb.Append("- **[").Append(GetLevelName(item.Level, culture)).Append("]** ");
                    }
                    else
                    {
                        sb.Append("- [").Append(GetLevelName(item.Level, culture)).Append("] ");
                    }

                    sb.Append(text.Replace("\n", "\n    "));
                    sb.AppendLine();
                }

                _ = sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <summary>
        /// The file name suggested in the save dialog, with a timestamp and a language tag so
        /// that saving both the English and the current-language log cannot silently overwrite
        /// the other one.
        /// </summary>
        public static string GetDefaultFileName(CultureInfo culture)
        {
            string languageTag = string.IsNullOrEmpty(culture.Name) ? "en" : culture.Name;
            return $"Mo3RegUI-log-{languageTag}-{DateTime.Now:yyyyMMdd-HHmmss}.md";
        }

        /// <summary>
        /// One "- name: value" entry of the metadata list that follows the document title.
        /// </summary>
        private static void AppendMetadata(StringBuilder sb, string resourceKey, CultureInfo culture, params object[] args) =>
            sb.Append("- ").AppendLine(Format(resourceKey, culture, args));

        private static object[] GetSummaryArgs(List<IMessageItem> items)
        {
            int critical = 0, error = 0, warning = 0, info = 0, debug = 0;
            foreach (IMessageItem item in items)
            {
                switch (item.Level)
                {
                    case MessageLevel.Debug: debug++; break;
                    case MessageLevel.Info: info++; break;
                    case MessageLevel.Warning: warning++; break;
                    case MessageLevel.Error: error++; break;
                    case MessageLevel.Critical: critical++; break;
                }
            }

            return new object[] { critical, error, warning, info, debug };
        }

        private static string GetLanguageName(CultureInfo culture) =>
            Localization.IsNeutralCulture(culture)
                ? Localization.GetString(nameof(TextResource.Log_LanguageEnglish), culture)
                : culture.NativeName;

        private static string GetLevelName(MessageLevel level, CultureInfo culture) => level switch
        {
            MessageLevel.Debug => Localization.GetString(nameof(TextResource.Log_Level_Debug), culture),
            MessageLevel.Info => Localization.GetString(nameof(TextResource.Log_Level_Info), culture),
            MessageLevel.Warning => Localization.GetString(nameof(TextResource.Log_Level_Warning), culture),
            MessageLevel.Error => Localization.GetString(nameof(TextResource.Log_Level_Error), culture),
            MessageLevel.Critical => Localization.GetString(nameof(TextResource.Log_Level_Critical), culture),
            _ => level.ToString(),
        };

        private static string Format(string resourceKey, CultureInfo culture, params object[] args) =>
            string.Format(culture, Localization.GetString(resourceKey, culture), args);

        private static string NormalizeLineBreaks(string text) =>
            (text ?? string.Empty).Replace("\r\n", "\n").Replace('\r', '\n');
    }
}
