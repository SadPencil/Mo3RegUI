using Mo3RegUI.LocalizationResources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;

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
                        // TaskManager_ExecutionFailed: Execution failed: {0}
                        this.ReportMessage(task.Task, new TaskMessageEventArgs()
                        {
                            Level = MessageLevel.Critical,
                            Text = LocalizedText.FromResource(
                                nameof(TextResource.TaskManager_ExecutionFailed),
                                ToLocalizedText(worker_e.Error)),
                        });
                    }
                    else
                    {
                        // TaskManager_ExecutionComplete: Execution complete.
                        this.ReportMessage(task.Task, new TaskMessageEventArgs()
                        {
                            Level = MessageLevel.Info,
                            Text = LocalizedText.FromResource(nameof(TextResource.TaskManager_ExecutionComplete)),
                        });
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
                    Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
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
                        // TaskManager_ExecutionFailed: Execution failed: {0}
                        worker.ReportProgress(0, new TaskMessageEventArgs()
                        {
                            Level = MessageLevel.Critical,
                            Text = LocalizedText.FromResource(
                                nameof(TextResource.TaskManager_ExecutionFailed),
                                ToLocalizedText(ex)),
                        });
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

        /// <summary>
        /// Keeps the deferred text of a <see cref="LocalizedException"/> so that a task failure
        /// can still be rendered in English, and falls back to the plain message otherwise.
        /// </summary>
        private static LocalizedText ToLocalizedText(Exception ex) =>
            (ex as LocalizedException)?.Text ?? LocalizedText.FromLiteral(ex?.Message ?? string.Empty);

    }
}
