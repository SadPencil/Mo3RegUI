using Mo3RegUI.LocalizationResources;
using System;

namespace Mo3RegUI.Tasks
{

    public class EncodingCheckTaskParameter : ITaskParameter
    {
    }
    public class EncodingCheckTask : ITask
    {
        // EncodingCheckTask_Description: Check System ANSI Code Page
        public string Description => TextResource.EncodingCheckTask_Description;

        public event EventHandler<TaskMessageEventArgs> ReportMessage;

        public void DoWork(ITaskParameter p)
        {
            if (p is EncodingCheckTaskParameter pp)
            {
                this._DoWork(pp);
            }
            else { throw new ArgumentException(); }
        }
        private void _DoWork(EncodingCheckTaskParameter p)
        {
            uint codepage = NativeMethods.GetACP();
            if (codepage == 65001)
            {
                // EncodingCheckTask_Utf8Warning: The current ANSI code page is UTF-8. This is a good practice, but unfortunately, ...
                string message = TextResource.EncodingCheckTask_Utf8Warning;
                ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Warning, Text = message });
            }
        }
    }
}
