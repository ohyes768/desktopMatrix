using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using DesktopWidget.Models;
using System.Diagnostics;

namespace DesktopWidget.Services
{
    public class TaskManager
    {
        private readonly DatabaseService _dbService;

        public ObservableCollection<TaskItem> Q1Tasks { get; private set; }
        public ObservableCollection<TaskItem> Q2Tasks { get; private set; }
        public ObservableCollection<TaskItem> Q3Tasks { get; private set; }
        public ObservableCollection<TaskItem> Q4Tasks { get; private set; }

        public event EventHandler<TaskChangedEventArgs> TaskChanged;

        public TaskManager()
        {
            _dbService = new DatabaseService();

            Q1Tasks = new ObservableCollection<TaskItem>();
            Q2Tasks = new ObservableCollection<TaskItem>();
            Q3Tasks = new ObservableCollection<TaskItem>();
            Q4Tasks = new ObservableCollection<TaskItem>();
        }

        public async Task InitializeAsync()
        {
            await LoadTasksAsync();
        }

        private async Task LoadTasksAsync()
        {
            var allTasks = await _dbService.GetAllTasksAsync();

            Q1Tasks.Clear();
            Q2Tasks.Clear();
            Q3Tasks.Clear();
            Q4Tasks.Clear();

            foreach (var task in allTasks)
            {
                AddTaskToQuadrant(task);
            }
        }

        public async Task<TaskItem> AddTaskAsync(string title, QuadrantType quadrant)
        {
            if (string.IsNullOrWhiteSpace(title))
                return null;

            var task = new TaskItem(title, quadrant);

            try
            {
                task.Id = await _dbService.InsertTaskAsync(task);

                if (task.Id > 0)
                {
                    AddTaskToQuadrant(task);
                    TriggerTaskChanged(TaskChangedEventArgs.Added(task));
                    return task;
                }
            }
            catch (Exception ex)
            {
                // 记录错误但不抛出，让调用者处理
                System.Diagnostics.Debug.WriteLine($"添加任务失败: {ex.Message}");
            }

            return null;
        }

        // 简化的同步版本
        public TaskItem AddTaskSync(string title, QuadrantType quadrant)
        {
            MainWindow.LogStatic($"TaskManager.AddTaskSync 开始 - 标题: '{title}', 象限: {quadrant}");

            if (string.IsNullOrWhiteSpace(title))
            {
                MainWindow.LogStatic("TaskManager.AddTaskSync 失败: 标题为空");
                return null;
            }

            var task = new TaskItem(title, quadrant);
            MainWindow.LogStatic($"创建了 TaskItem: 标题='{task.Title}', 象限={task.Quadrant}");

            try
            {
                MainWindow.LogStatic("调用 DatabaseService.InsertTaskSync");
                // 直接同步调用数据库
                task.Id = _dbService.InsertTaskSync(task);
                MainWindow.LogStatic($"数据库返回ID: {task.Id}");

                if (task.Id > 0)
                {
                    MainWindow.LogStatic("添加任务到象限");

                    // 必须在UI线程中修改ObservableCollection
                    if (System.Windows.Application.Current.Dispatcher.CheckAccess())
                    {
                        // 当前已在UI线程，直接添加
                        AddTaskToQuadrant(task);
                    }
                    else
                    {
                        // 在后台线程，需要切换到UI线程
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            AddTaskToQuadrant(task);
                        });
                    }

                    MainWindow.LogStatic("触发任务变更事件");
                    TriggerTaskChanged(TaskChangedEventArgs.Added(task));
                    MainWindow.LogStatic($"TaskManager.AddTaskSync 成功 - ID: {task.Id}");
                    return task;
                }
                else
                {
                    MainWindow.LogStatic("TaskManager.AddTaskSync 失败: 数据库返回ID <= 0");
                }
            }
            catch (Exception ex)
            {
                MainWindow.LogStatic($"TaskManager.AddTaskSync 异常: {ex.Message}");
                MainWindow.LogStatic($"TaskManager.AddTaskSync 堆栈: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    MainWindow.LogStatic($"TaskManager.AddTaskSync 内部异常: {ex.InnerException.Message}");
                }
            }

            MainWindow.LogStatic("TaskManager.AddTaskSync 返回 null");
            return null;
        }

        public async Task<bool> UpdateTaskAsync(TaskItem task)
        {
            if (task == null) return false;

            var success = await _dbService.UpdateTaskAsync(task);
            if (success)
            {
                RefreshQuadrant(task.Quadrant);
                TriggerTaskChanged(TaskChangedEventArgs.Updated(task));
            }

            return success;
        }

        public async Task<bool> DeleteTaskAsync(int taskId)
        {
            var task = FindTaskById(taskId);
            if (task == null) return false;

            var success = await _dbService.DeleteTaskAsync(taskId);
            if (success)
            {
                RemoveTaskFromQuadrant(task);
                TriggerTaskChanged(TaskChangedEventArgs.Deleted(task));
            }

            return success;
        }

        // 同步版本，用于UI操作
        public bool DeleteTask(int taskId)
        {
            MainWindow.LogStatic($"TaskManager.DeleteTask 开始 - 任务ID: {taskId}");

            bool success = false;
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                var task = FindTaskById(taskId);
                if (task == null)
                {
                    MainWindow.LogStatic($"DeleteTask 失败: 找不到任务ID {taskId}");
                    return;
                }

                try
                {
                    // 同步删除数据库
                    success = _dbService.DeleteTaskSync(taskId);
                    MainWindow.LogStatic($"数据库删除结果: {success}");

                    if (success)
                    {
                        // 从界面集合中删除
                        RemoveTaskFromQuadrant(task);
                        MainWindow.LogStatic("任务从界面集合中删除成功");
                        // 触发任务变更事件
                        TriggerTaskChanged(TaskChangedEventArgs.Deleted(task));
                        MainWindow.LogStatic("触发任务删除事件");
                    }
                    else
                    {
                        MainWindow.LogStatic("数据库删除失败");
                        success = false;
                    }
                }
                catch (Exception ex)
                {
                    MainWindow.LogStatic($"DeleteTask 异常: {ex.Message}");
                    MainWindow.LogStatic($"DeleteTask 堆栈: {ex.StackTrace}");
                    success = false;
                }
            });

            return success;
        }

        public async Task<bool> MoveTaskToQuadrantAsync(int taskId, QuadrantType targetQuadrant)
        {
            var task = FindTaskById(taskId);
            if (task == null) return false;

            if (task.Quadrant == targetQuadrant) return true;

            var originalQuadrant = task.Quadrant;
            task.Quadrant = targetQuadrant;

            var success = await _dbService.UpdateTaskAsync(task);
            if (success)
            {
                RemoveTaskFromQuadrantByQuadrant(originalQuadrant, taskId);
                AddTaskToQuadrant(task);
                TriggerTaskChanged(TaskChangedEventArgs.Moved(task));
            }

            return success;
        }

        public async Task<bool> ToggleTaskCompletionAsync(int taskId)
        {
            var task = FindTaskById(taskId);
            if (task == null) return false;

            task.IsCompleted = !task.IsCompleted;
            return await UpdateTaskAsync(task);
        }

        // 同步版本，用于UI操作
        public bool ToggleTaskCompletionSync(int taskId)
        {
            MainWindow.LogStatic($"ToggleTaskCompletionSync 开始 - 任务ID: {taskId}");

            // 关键修复：确保在UI线程中进行所有操作，避免对象不同步
            bool success = false;
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                var task = FindTaskById(taskId);
                if (task == null)
                {
                    MainWindow.LogStatic($"ToggleTaskCompletionSync 失败: 找不到任务ID {taskId}");
                    return;
                }

                bool oldCompleted = task.IsCompleted;
                MainWindow.LogStatic($"原始完成状态: {oldCompleted}");

                // 直接在UI线程中修改TaskItem，确保PropertyChanged事件能触发UI更新
                task.IsCompleted = !task.IsCompleted;
                MainWindow.LogStatic($"UI更新后状态: {task.IsCompleted}");

                try
                {
                    // 同步更新数据库
                    success = _dbService.UpdateTaskSync(task);
                    MainWindow.LogStatic($"数据库更新结果: {success}");

                    if (success)
                    {
                        MainWindow.LogStatic("任务完成状态切换成功");
                        // 触发任务变更事件，确保UI更新
                        TriggerTaskChanged(TaskChangedEventArgs.Updated(task));
                    }
                    else
                    {
                        MainWindow.LogStatic("数据库更新失败，恢复原状态");
                        // 恢复原状态
                        task.IsCompleted = oldCompleted;
                        MainWindow.LogStatic($"恢复后状态: {task.IsCompleted}");
                        // 再次触发UI更新
                        TriggerTaskChanged(TaskChangedEventArgs.Updated(task));
                        success = false;
                    }
                }
                catch (Exception ex)
                {
                    MainWindow.LogStatic($"ToggleTaskCompletionSync 异常: {ex.Message}");
                    MainWindow.LogStatic($"ToggleTaskCompletionSync 堆栈: {ex.StackTrace}");
                    // 恢复原状态
                    task.IsCompleted = oldCompleted;
                    MainWindow.LogStatic($"异常恢复后状态: {task.IsCompleted}");
                    // 再次触发UI更新
                    TriggerTaskChanged(TaskChangedEventArgs.Updated(task));
                    success = false;
                }
            });

            return success;
        }

        public Task<int> GetTotalTaskCount()
        {
            return Task.FromResult(Q1Tasks.Count + Q2Tasks.Count + Q3Tasks.Count + Q4Tasks.Count);
        }

        public Task<int> GetCompletedTaskCount()
        {
            return Task.FromResult(Q1Tasks.Count(t => t.IsCompleted) +
                               Q2Tasks.Count(t => t.IsCompleted) +
                               Q3Tasks.Count(t => t.IsCompleted) +
                               Q4Tasks.Count(t => t.IsCompleted));
        }

        private void AddTaskToQuadrant(TaskItem task)
        {
            switch (task.Quadrant)
            {
                case QuadrantType.Q1:
                    Q1Tasks.Add(task);
                    break;
                case QuadrantType.Q2:
                    Q2Tasks.Add(task);
                    break;
                case QuadrantType.Q3:
                    Q3Tasks.Add(task);
                    break;
                case QuadrantType.Q4:
                    Q4Tasks.Add(task);
                    break;
            }
        }

        private void RemoveTaskFromQuadrant(TaskItem task)
        {
            switch (task.Quadrant)
            {
                case QuadrantType.Q1:
                    Q1Tasks.Remove(task);
                    break;
                case QuadrantType.Q2:
                    Q2Tasks.Remove(task);
                    break;
                case QuadrantType.Q3:
                    Q3Tasks.Remove(task);
                    break;
                case QuadrantType.Q4:
                    Q4Tasks.Remove(task);
                    break;
            }
        }

        private void RemoveTaskFromQuadrantByQuadrant(QuadrantType quadrant, int taskId)
        {
            var tasks = GetQuadrantTasks(quadrant);
            var task = tasks.FirstOrDefault(t => t.Id == taskId);
            if (task != null)
            {
                RemoveTaskFromQuadrant(task);
            }
        }

        private ObservableCollection<TaskItem> GetQuadrantTasks(QuadrantType quadrant)
        {
            return quadrant switch
            {
                QuadrantType.Q1 => Q1Tasks,
                QuadrantType.Q2 => Q2Tasks,
                QuadrantType.Q3 => Q3Tasks,
                QuadrantType.Q4 => Q4Tasks,
                _ => Q1Tasks
            };
        }

        private void RefreshQuadrant(QuadrantType quadrant)
        {
            // 可以根据需要重新加载特定象限的任务
        }

        private TaskItem FindTaskById(int taskId)
        {
            return Q1Tasks.FirstOrDefault(t => t.Id == taskId) ??
                   Q2Tasks.FirstOrDefault(t => t.Id == taskId) ??
                   Q3Tasks.FirstOrDefault(t => t.Id == taskId) ??
                   Q4Tasks.FirstOrDefault(t => t.Id == taskId);
        }

        private void TriggerTaskChanged(TaskChangedEventArgs e)
        {
            TaskChanged?.Invoke(this, e);
        }
    }

    public class TaskChangedEventArgs : EventArgs
    {
        public TaskChangeType ChangeType { get; set; }
        public TaskItem Task { get; set; }

        public static TaskChangedEventArgs Added(TaskItem task) =>
            new TaskChangedEventArgs { ChangeType = TaskChangeType.Added, Task = task };

        public static TaskChangedEventArgs Updated(TaskItem task) =>
            new TaskChangedEventArgs { ChangeType = TaskChangeType.Updated, Task = task };

        public static TaskChangedEventArgs Deleted(TaskItem task) =>
            new TaskChangedEventArgs { ChangeType = TaskChangeType.Deleted, Task = task };

        public static TaskChangedEventArgs Moved(TaskItem task) =>
            new TaskChangedEventArgs { ChangeType = TaskChangeType.Moved, Task = task };
    }

    public enum TaskChangeType
    {
        Added,
        Updated,
        Deleted,
        Moved
    }
}