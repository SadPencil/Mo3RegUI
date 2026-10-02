using Mo3RegUI.LocalizationResources;
using System;
using System.Globalization;

namespace Mo3RegUI.Tasks
{
    /// <summary>
    /// A single piece of output text whose language is decided only when it is displayed or
    /// exported.
    /// </summary>
    /// <remarks>
    /// Tasks run in the background and must not look up localized strings at run time: doing so
    /// would capture the UI language of the moment and make it impossible to export the same
    /// output in English afterwards. A task therefore records the resource key together with
    /// the raw format arguments, and the caller resolves them with the wanted culture.
    /// </remarks>
    public class LocalizedText
    {
        private readonly string _resourceKey;
        private readonly object[] _formatArgs;
        private readonly string _literalText;

        private LocalizedText(string resourceKey, object[] formatArgs, string literalText)
        {
            this._resourceKey = resourceKey;
            this._formatArgs = formatArgs;
            this._literalText = literalText;
        }

        /// <summary>
        /// Text that comes from the resource files, e.g. <c>FromResource(nameof(TextResource.SomeTask_Message), value)</c>.
        /// </summary>
        public static LocalizedText FromResource(string resourceKey, params object[] formatArgs) =>
            new LocalizedText(resourceKey, formatArgs, null);

        /// <summary>
        /// Text that is not translatable, e.g. the output of an external command or a file path.
        /// </summary>
        public static LocalizedText FromLiteral(string text) =>
            new LocalizedText(null, null, text);

        /// <summary>
        /// The resource key, or <c>null</c> for literal text. Useful for diagnostics.
        /// </summary>
        public string ResourceKey => this._resourceKey;

        /// <summary>
        /// Renders the text in the given culture. Arguments that are themselves
        /// <see cref="LocalizedText"/> are resolved first, so a translated sentence can embed
        /// another translated string.
        /// </summary>
        public string Resolve(CultureInfo culture)
        {
            if (this._resourceKey is null)
            {
                return this._literalText ?? string.Empty;
            }

            // GetString falls back through the culture's parents to the neutral (English)
            // resources, so a translation that is merely missing is already served in English.
            string format = TextResource.ResourceManager.GetString(this._resourceKey, culture);
            if (format is null)
            {
                // Only a key that is missing from the English resources as well reaches this
                // point, which means the resources and the code disagree. That cannot happen
                // through translation, and a Debug.Assert would vanish in a release build, so
                // fail with a message that names the offending key.
                throw new InvalidOperationException(
                    $"The resource \"{this._resourceKey}\" is missing from the neutral (English) resources; resolving it for \"{culture.Name}\" has nothing to fall back to.");
            }

            if (this._formatArgs is null || this._formatArgs.Length == 0)
            {
                return format;
            }

            var resolvedArgs = new object[this._formatArgs.Length];
            for (int i = 0; i < this._formatArgs.Length; i++)
            {
                object arg = this._formatArgs[i];
                resolvedArgs[i] = arg is LocalizedText nested ? nested.Resolve(culture) : arg;
            }

            return string.Format(culture, format, resolvedArgs);
        }

        public override string ToString() => this.Resolve(Localization.CurrentUICulture);
    }
}
