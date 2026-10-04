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
        private readonly object[] _formatArgs;
        private readonly string _literalText;

        private LocalizedText(string resourceKey, object[] formatArgs, string literalText)
        {
            this.ResourceKey = resourceKey;
            this._formatArgs = formatArgs;
            this._literalText = literalText;
        }

        /// <summary>
        /// Text that comes from the resource files, e.g. <c>FromResource(nameof(TextResource.SomeTask_Message), value)</c>.
        /// </summary>
        public static LocalizedText FromResource(string resourceKey, params object[] formatArgs) =>
            new(resourceKey, formatArgs, null);

        /// <summary>
        /// Text that is not translatable, e.g. the output of an external command or a file path.
        /// </summary>
        public static LocalizedText FromLiteral(string text) =>
            new(null, null, text);

        /// <summary>
        /// The resource key, or <c>null</c> for literal text. Useful for diagnostics.
        /// </summary>
        public string ResourceKey { get; }

        /// <summary>
        /// Renders the text in the given culture. Arguments that are themselves
        /// <see cref="LocalizedText"/> are resolved first, so a translated sentence can embed
        /// another translated string.
        /// </summary>
        public string Resolve(CultureInfo culture)
        {
            if (this.ResourceKey is null)
            {
                return this._literalText ?? string.Empty;
            }

            string format = TextResource.ResourceManager.GetString(this.ResourceKey, culture);
            if (format is null)
            {
                throw new InvalidOperationException(
                    $"The resource \"{this.ResourceKey}\" is missing from the neutral (English) resources.");
            }

            if (this._formatArgs is null || this._formatArgs.Length == 0)
            {
                return format;
            }

            object[] resolvedArgs = new object[this._formatArgs.Length];
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
