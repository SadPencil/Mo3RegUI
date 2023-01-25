using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Mo3RegUI.Tasks
{
    public class TaskManager
    {
        private readonly List<TaskInstance> Tasks; // .NET 4.0 does not support IReadOnlyList
        public int WaitCount { get; private set; } = 0;
        private List<BackgroundWorker> Workers;
        public event EventHandler<TaskMessageEventArgs> ReportMessage;
        public event EventHandler<TaskCompletedEventArgs> TaskCompleted;
        public TaskManager(List<TaskInstance> tasks) => this.Tasks = tasks.ToList();

        public void RunAsync()
        {
            if (this.WaitCount != 0)
            {
                throw new Exception("Existing tasks are running.");
            }

            this.Workers = new List<BackgroundWorker>();
            foreach (var task in this.Tasks)
            {
                var worker = new BackgroundWorker() { WorkerReportsProgress = true };
                this.Workers.Add(worker);

                this.WaitCount++;

                worker.RunWorkerCompleted += (object worker_sender, RunWorkerCompletedEventArgs worker_e) =>
                {
                    this.WaitCount--;
                    if (worker_e.Error is not null)
                    {
                        this.ReportMessage(task.Task, new TaskMessageEventArgs() { Level = MessageLevel.Critical, Text = "执行失败：" + worker_e.Error.Message });
                    }
                    else
                    {
                        this.ReportMessage(task.Task, new TaskMessageEventArgs() { Level = MessageLevel.Info, Text = "执行结束。" });
                    }
                    this.TaskCompleted(this, new TaskCompletedEventArgs() { TaskInstance = task });
                };

                worker.ProgressChanged += (object worker_sender, ProgressChangedEventArgs worker_e) =>
                {
                    var message = worker_e.UserState as TaskMessageEventArgs;
                    this.ReportMessage(task.Task, message);
                };

                worker.DoWork += (object worker_sender, DoWorkEventArgs worker_e) =>
                {
                    task.Task.ReportMessage += (task_sender, task_e) =>
                    {
                        worker.ReportProgress(0, task_e);
                    };
#if DEBUG         
                    try
                    {
                        task.Task.DoWork(task.Parameter);
                    }
                    catch (Exception ex)
                    {
                        worker.ReportProgress(0, new TaskMessageEventArgs() { Level = MessageLevel.Critical, Text = ex.Message });
                    }
#else
                    task.Task.DoWork(task.Parameter);
#endif
                };
            }
            foreach (var worker in this.Workers)
            {
                worker.RunWorkerAsync();
            }
        }

    }
}
