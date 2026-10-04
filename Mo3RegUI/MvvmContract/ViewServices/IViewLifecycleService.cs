using System;
using System.ComponentModel;

namespace Mo3RegUI.MvvmContract.ViewServices
{
    /// <summary>
    /// Notifies the ViewModel about window lifecycle events and lets it shut the application
    /// down. This is how the View reports "the user is trying to close the window" without ever
    /// calling the ViewModel directly, and how the ViewModel asks the window to close.
    /// </summary>
    public interface IViewLifecycleService
    {
        /// <summary>
        /// Raised while the window is closing. Setting <see cref="CancelEventArgs.Cancel"/> keeps
        /// the window open.
        /// </summary>
        event EventHandler<CancelEventArgs> Closing;

        /// <summary>Shuts the application down with the given exit code.</summary>
        void Shutdown(int exitCode);
    }
}
