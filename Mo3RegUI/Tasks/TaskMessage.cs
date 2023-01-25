using System;

namespace Mo3RegUI.Tasks
{
    public class TaskMessageEventArgs : EventArgs
    {
        public MessageLevel Level;
        public string Text;
    }
}
