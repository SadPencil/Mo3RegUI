using System;

namespace Mo3RegUI.Tasks
{
    public interface ITask
    {
        event EventHandler<TaskMessageEventArgs> ReportMessage;
        void DoWork(ITaskParameter p);
        string Description { get; }
    }
}
