using Mo3RegUI.LocalizationResources;
using System;
using System.Globalization;

namespace Mo3RegUI
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
        /// Whether <paramref name="culture"/> really shows English. Every translation declares
        /// its own culture name in <c>Localization_CultureName</c>; a locale that has no
        /// translation falls back to the neutral resources and therefore declares <c>en-US</c>,
        /// which is exactly what we want to detect. Using this dedicated marker avoids both the
        /// inconsistent satellite probing of <see cref="System.Resources.ResourceManager"/> and
        /// any dependence on the wording of ordinary UI strings.
        /// </summary>
        public static bool IsCultureEnglish(CultureInfo culture) =>
            string.Equals(
                GetDeclaredCultureName(culture),
                GetDeclaredCultureName(EnglishCulture),
                StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// The culture name a translation declares for itself. The value drives
        /// <see cref="IsCultureEnglish"/>, so a translation of it would silently change how the
        /// program behaves; a value that is not ASCII is rejected loudly instead, which catches
        /// the mistake during translation rather than in the field.
        /// </summary>
        private static string GetDeclaredCultureName(CultureInfo culture)
        {
            string declaredName = GetString(nameof(TextResource.Localization_CultureName), culture);
            if (!IsAscii(declaredName))
            {
                throw new InvalidOperationException(
                    $"The resource \"{nameof(TextResource.Localization_CultureName)}\" must stay an ASCII culture name such as \"zh-Hans\"; the resources for \"{culture.Name}\" contain \"{declaredName}\". This resource identifies the language for the program and must not be translated.");
            }

            return declaredName;
        }

        private static bool IsAscii(string text)
        {
            foreach (char c in text)
            {
                if (c > 0x7F)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Whether the UI is currently showing English. In that case a separate
        /// "current language" log would be identical to the English one, so only a single save
        /// button is offered.
        /// </summary>
        public static bool IsCurrentCultureEnglish => IsCultureEnglish(CurrentUICulture);
    }
}
