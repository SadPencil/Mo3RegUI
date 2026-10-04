using Mo3RegUI.MVVMContract;
using System;

namespace Mo3RegUI.ViewModel.Tasks
{
    public class TaskMessageEventArgs : EventArgs
    {
        public MessageLevel Level;

        /// <summary>
        /// The message in a culture-independent form. It is resolved to a string only when it is
        /// displayed or written to a log.
        /// </summary>
        public LocalizedText Text;
    }
}
