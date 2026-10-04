using System;

namespace Mo3RegUI.ViewModel.Tasks
{
    /// <summary>
    /// An exception whose message must be translated only when it is turned into log output.
    /// Use it instead of <c>new Exception(TextResource.SomeMessage)</c>: the plain form resolves
    /// the resource at run time, which would leak the UI language into an English log.
    /// </summary>
    public class LocalizedException : Exception
    {
        /// <summary>
        /// The message in a culture-independent form.
        /// </summary>
        public LocalizedText Text { get; }

        public LocalizedException(LocalizedText text)
            : base(text?.Resolve(Localization.CurrentUICulture)) => this.Text = text;
    }
}
