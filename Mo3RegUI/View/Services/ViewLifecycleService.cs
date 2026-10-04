using Mo3RegUI.MVVMContract.ViewServices;
using System;
using System.ComponentModel;
using System.Windows;

namespace Mo3RegUI.View.Services
{
    /// <summary>
    /// The View side of <see cref="IViewLifecycleService"/>. <see cref="Attach"/> is the only
    /// member that is not part of the contract: the composition root calls it to connect the
    /// window's Closing event to the ViewModel. Keeping it here means the window's code-behind
    /// stays empty.
    /// </summary>
    public class ViewLifecycleService : IViewLifecycleService
    {
        public event EventHandler<CancelEventArgs> Closing;

        /// <summary>
        /// Forwards the window's Closing event to <see cref="Closing"/>. The ViewModel may set
        /// <see cref="CancelEventArgs.Cancel"/> to keep the window open.
        /// </summary>
        public void Attach(Window window) => window.Closing += this.Window_Closing;

        public void Shutdown(int exitCode) => Application.Current?.Shutdown(exitCode);

        private void Window_Closing(object sender, CancelEventArgs e) => this.Closing?.Invoke(sender, e);
    }
}
