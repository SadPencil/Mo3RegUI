using Mo3RegUI.LocalizationResources;
using System.Globalization;

namespace Mo3RegUI.ViewModel
{
    /// <summary>
    /// Access to the strings in <see cref="TextResource"/> for a culture other than the one the
    /// UI currently uses. The UI itself keeps binding through <see cref="TextResource"/>.
    /// </summary>
    public static class Localization
    {
        /// <summary>
        /// The culture whose resources are the neutral, English ones. Used when exporting an
        /// English log.
        /// </summary>
        public static readonly CultureInfo EnglishCulture = CultureInfo.GetCultureInfo("en-US");

        /// <summary>
        /// The culture the UI is currently rendered in. <see cref="TextResource.Culture"/> is
        /// normally <c>null</c>, in which case the resource manager falls back to
        /// <see cref="CultureInfo.CurrentUICulture"/>.
        /// </summary>
        public static CultureInfo CurrentUICulture => TextResource.Culture ?? CultureInfo.CurrentUICulture;

        /// <summary>
        /// Looks up a resource by key, returning the key itself when the resource is missing so
        /// that a broken translation is visible instead of throwing. An empty key yields an
        /// empty string, which keeps callers that group by key safe.
        /// </summary>
        public static string GetString(string resourceKey, CultureInfo culture) =>
            string.IsNullOrEmpty(resourceKey)
                ? string.Empty
                : TextResource.ResourceManager.GetString(resourceKey, culture) ?? resourceKey;

        /// <summary>
        /// Whether <paramref name="culture"/> is served by the neutral, English resources. Every
        /// translation sets <c>Localization_IsNeutralCulture = False</c>; the neutral resources
        /// hold a sentence asking the translator to do exactly that, and every value that does not
        /// parse as the boolean <c>false</c> counts as neutral. A locale that has no translation
        /// therefore falls back to the neutral resources and is reported as neutral, which is what
        /// decides that only a single save button is offered.
        /// </summary>
        public static bool IsNeutralCulture(CultureInfo culture)
        {
            string value = GetString(nameof(TextResource.Localization_IsNeutralCulture), culture);
            if (bool.TryParse(value, out bool isNeutral))
            {
                return isNeutral;
            }

            // The sentence in the neutral resources, an empty value, or anything a translator
            // wrote instead of "False" all mean "serve the neutral, English resources".
            return true;
        }

        /// <summary>
        /// Whether the UI is currently served by the neutral, English resources. In that case a
        /// separate "current language" log would be identical to the English one, so only a
        /// single save button is offered.
        /// </summary>
        public static bool IsCurrentCultureNeutral => IsNeutralCulture(CurrentUICulture);
    }
}
