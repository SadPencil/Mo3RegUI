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
        /// that a broken translation is visible instead of throwing.
        /// </summary>
        public static string GetString(string resourceKey, CultureInfo culture) =>
            TextResource.ResourceManager.GetString(resourceKey, culture) ?? resourceKey;

        /// <summary>
        /// Whether the UI is currently showing English, either because the system locale is
        /// English or because the locale has no translation and therefore falls back to the
        /// neutral resources. In that case a separate "current language" log would be identical
        /// to the English one, so only a single save button is offered.
        /// </summary>
        public static bool IsCurrentCultureEnglish
        {
            get
            {
                CultureInfo culture = CurrentUICulture;
                if (string.IsNullOrEmpty(culture.Name) || culture.TwoLetterISOLanguageName == "en")
                {
                    return true;
                }

                try
                {
                    // Walk the culture's parents looking for a satellite resource set. The neutral
                    // (English) set lives in the main assembly and is not a satellite, so any
                    // match here means the locale has its own translation.
                    for (CultureInfo current = culture; !string.IsNullOrEmpty(current.Name); current = current.Parent)
                    {
                        if (TextResource.ResourceManager.GetResourceSet(current, createIfNotExists: false, tryParents: false) is not null)
                        {
                            return false;
                        }
                    }
                }
                catch (Exception)
                {
                    // Never let a localization probe stop the program from starting.
                    return true;
                }

                return true;
            }
        }
    }
}
