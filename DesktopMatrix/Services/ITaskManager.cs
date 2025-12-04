using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using DesktopMatrix.Models;

namespace DesktopMatrix.Services
{
    /// <summary>
    /// 任务管理服务接口
    /// </summary>
    public interface ITaskManager
    {
        ObservableCollection<TaskItem> Q1Tasks { get; }
        ObservableCollection<TaskItem> Q2Tasks { get; }
        ObservableCollection<TaskItem> Q3Tasks { get; }
        ObservableCollection<TaskItem> Q4Tasks { get; }

        Task InitializeAsync();
        TaskItem AddTask(string title, QuadrantType quadrant);
        bool MoveTask(Guid taskId, QuadrantType targetQuadrant);
        bool DeleteTask(Guid taskId);
        List<TaskItem> GetQuadrantTasks(QuadrantType quadrant);
        List<TaskItem> SearchTasks(string keyword);
        Task SaveChangesAsync();
    }

    /// <summary>
    /// 任务管理服务实现类
    /// </summary>
    public class TaskManager : ITaskManager
    {
        private readonly IDataService _dataService;
        private TaskData _taskData;
        private System.Timers.Timer _autoSaveTimer;
        private bool _hasChanges;

        // 四个象限的任务集合
        public ObservableCollection<TaskItem> Q1Tasks { get; private set; }
        public ObservableCollection<TaskItem> Q2Tasks { get; private set; }
        public ObservableCollection<TaskItem> Q3Tasks { get; private set; }
        public ObservableCollection<TaskItem> Q4Tasks { get; private set; }

        public TaskManager(IDataService dataService)
        {
            _dataService = dataService;
            
            // 初始化集合
            Q1Tasks = new ObservableCollection<TaskItem>();
            Q2Tasks = new ObservableCollection<TaskItem>();
            Q3Tasks = new ObservableCollection<TaskItem>();
            Q4Tasks = new ObservableCollection<TaskItem>();
            
            // 设置自动保存定时器（30秒）
            _autoSaveTimer = new System.Timers.Timer(30000);
            _autoSaveTimer.Elapsed += AutoSaveTimer_Elapsed;
            _autoSaveTimer.AutoReset = true;
        }

        /// <summary>
        /// 初始化任务管理器
        /// </summary>
        public async Task InitializeAsync()
        {
            // 加载数据
            _taskData = await _dataService.LoadDataAsync();
            
            // 清空现有集合
            Q1Tasks.Clear();
            Q2Tasks.Clear();
            Q3Tasks.Clear();
            Q4Tasks.Clear();
            
            // 分类填充任务
            foreach (var task in _taskData.Tasks)
            {
                AddTaskToCollection(task);
            }
            
            // 清理过期任务
            await CleanExpiredTasksAsync();
            
            // 启动自动保存定时器
            _autoSaveTimer.Start();
        }

        /// <summary>
        /// 添加任务到对应集合
        /// </summary>
        private void AddTaskToCollection(TaskItem task)
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

        /// <summary>
        /// 从集合中移除任务
        /// </summary>
        private bool RemoveTaskFromCollection(Guid taskId)
        {
            // 查找并移除任务
            var task = FindTaskById(taskId);
            if (task == null)
            {
                return false;
            }

            switch (task.Quadrant)
            {
                case QuadrantType.Q1:
                    return Q1Tasks.Remove(task);
                case QuadrantType.Q2:
                    return Q2Tasks.Remove(task);
                case QuadrantType.Q3:
                    return Q3Tasks.Remove(task);
                case QuadrantType.Q4:
                    return Q4Tasks.Remove(task);
                default:
                    return false;
            }
        }

        /// <summary>
        /// 根据ID查找任务
        /// </summary>
        private TaskItem FindTaskById(Guid taskId)
        {
            // 在所有象限中查找
            return Q1Tasks.FirstOrDefault(t => t.Id == taskId) ??
                   Q2Tasks.FirstOrDefault(t => t.Id == taskId) ??
                   Q3Tasks.FirstOrDefault(t => t.Id == taskId) ??
                   Q4Tasks.FirstOrDefault(t => t.Id == taskId);
        }

        /// <summary>
        /// 添加新任务
        /// </summary>
        public TaskItem AddTask(string title, QuadrantType quadrant)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return null;
            }

            var task = new TaskItem(title, quadrant);

            // 添加到内存集合
            AddTaskToCollection(task);

            // 添加到数据模型
            _taskData.Tasks.Add(task);

            // 标记有变更
            _hasChanges = true;

            return task;
        }

        /// <summary>
        /// 移动任务到其他象限
        /// </summary>
        public bool MoveTask(Guid taskId, QuadrantType targetQuadrant)
        {
            var task = FindTaskById(taskId);
            if (task == null)
            {
                return false;
            }

            // 如果目标象限与当前象限相同，无需移动
            if (task.Quadrant == targetQuadrant)
            {
                return true;
            }

            // 从原象限移除
            if (!RemoveTaskFromCollection(taskId))
            {
                return false;
            }

            // 更新象限属性
            task.Quadrant = targetQuadrant;
            
            // 添加到新象限
            AddTaskToCollection(task);
            
            // 标记有变更
            _hasChanges = true;
            
            return true;
        }

        /// <summary>
        /// 删除任务
        /// </summary>
        public bool DeleteTask(Guid taskId)
        {
            var task = FindTaskById(taskId);
            if (task == null)
            {
                return false;
            }

            // 从集合中移除
            if (!RemoveTaskFromCollection(taskId))
            {
                return false;
            }

            // 从数据模型中移除
            _taskData.Tasks.RemoveAll(t => t.Id == taskId);
            
            // 标记有变更
            _hasChanges = true;
            
            return true;
        }

        /// <summary>
        /// 获取指定象限的所有任务
        /// </summary>
        public List<TaskItem> GetQuadrantTasks(QuadrantType quadrant)
        {
            switch (quadrant)
            {
                case QuadrantType.Q1:
                    return Q1Tasks.ToList();
                case QuadrantType.Q2:
                    return Q2Tasks.ToList();
                case QuadrantType.Q3:
                    return Q3Tasks.ToList();
                case QuadrantType.Q4:
                    return Q4Tasks.ToList();
                default:
                    return new List<TaskItem>();
            }
        }

        /// <summary>
        /// 搜索任务
        /// </summary>
        public List<TaskItem> SearchTasks(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return _taskData.Tasks;
            }

            keyword = keyword.ToLower();
            
            // 在所有任务中搜索
            return _taskData.Tasks
                .Where(t => t.Title.ToLower().Contains(keyword) || 
                           (t.Tags != null && t.Tags.Any(tag => tag.ToLower().Contains(keyword))))
                .ToList();
        }

        /// <summary>
        /// 清理过期任务
        /// </summary>
        private async Task CleanExpiredTasksAsync()
        {
            int autoCleanDays = _taskData.Config.AutoCleanDays;
            if (autoCleanDays <= 0)
            {
                return;
            }

            DateTime cutoffDate = DateTime.Now.AddDays(-autoCleanDays);
            
            // 查找已完成且过期的任务
            var expiredTasks = _taskData.Tasks
                .Where(t => t.IsCompleted && t.CompleteTime.HasValue && t.CompleteTime.Value < cutoffDate)
                .ToList();

            if (expiredTasks.Count == 0)
            {
                return;
            }

            // 移除过期任务
            foreach (var task in expiredTasks)
            {
                RemoveTaskFromCollection(task.Id);
                _taskData.Tasks.Remove(task);
            }

            // 保存变更
            await SaveChangesAsync();
        }

        /// <summary>
        /// 自动保存定时器事件处理
        /// </summary>
        private async void AutoSaveTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_hasChanges)
            {
                await SaveChangesAsync();
            }
        }

        /// <summary>
        /// 保存变更
        /// </summary>
        public async Task SaveChangesAsync()
        {
            if (_taskData != null)
            {
                await _dataService.SaveDataAsync(_taskData);
                
                // 每天创建一次备份
                TimeSpan timeSinceLastBackup = DateTime.Now - _taskData.Config.LastBackupTime;
                if (timeSinceLastBackup.TotalHours >= 24)
                {
                    await _dataService.BackupDataAsync();
                    _taskData.Config.LastBackupTime = DateTime.Now;
                }
                
                _hasChanges = false;
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            _autoSaveTimer?.Stop();
            _autoSaveTimer?.Dispose();
        }
    }
}