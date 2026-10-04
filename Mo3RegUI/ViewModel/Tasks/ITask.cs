using System;

namespace Mo3RegUI.ViewModel.Tasks
{
    public interface ITask
    {
        event EventHandler<TaskMessageEventArgs> ReportMessage;
        void DoWork(ITaskParameter p);

        /// <summary>
        /// Resource key of the human-readable name of this task. It is stored as a key rather
        /// than a formatted string so that the category of a message can also be translated when
        /// the messages are exported in another language.
        /// </summary>
        string DescriptionResourceKey { get; }
    }
}
