using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace TaskMatrixApp.Models
{
    public class TaskItem : INotifyPropertyChanged
    {
        private int _id;
        private string _title;
        private QuadrantType _quadrant;
        private bool _isCompleted;
        private DateTime _createTime;
        private DateTime? _completeTime;
        private List<string> _tags;
        private string _description;
        private int _priority;

        // 添加静态数据库服务引用，用于自动保存
        public static TaskMatrixApp.Services.DatabaseService _dbService;

        public int Id
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged(nameof(Id));
                }
            }
        }

        public string Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged(nameof(Title));
                }
            }
        }

        public QuadrantType Quadrant
        {
            get => _quadrant;
            set
            {
                if (_quadrant != value)
                {
                    _quadrant = value;
                    OnPropertyChanged(nameof(Quadrant));
                }
            }
        }

        public bool IsCompleted
        {
            get => _isCompleted;
            set
            {
                if (_isCompleted != value)
                {
                    // 修复日志：在修改值之前记录原始值
                    var oldValue = _isCompleted;
                    System.Diagnostics.Debug.WriteLine($"TaskItem.IsCompleted 变更: {oldValue} -> {value}, TaskId: {Id}");
                    MainWindow.LogStatic($"TaskItem.IsCompleted 变更: {oldValue} -> {value}, TaskId: {Id}, Title: '{Title}'");

                    _isCompleted = value;
                    CompleteTime = value ? DateTime.Now : null;

                    // 自动更新数据库（异步执行，避免阻塞UI）
                    if (Id > 0 && _dbService != null)
                    {
                        try
                        {
                            System.Diagnostics.Debug.WriteLine($"自动保存数据库 - TaskId: {Id}, IsCompleted: {_isCompleted}");
                            MainWindow.LogStatic($"自动保存数据库 - TaskId: {Id}, IsCompleted: {_isCompleted}");

                            // 异步执行，避免与UI线程的数据库操作冲突
                            System.Threading.Tasks.Task.Run(async () =>
                            {
                                await _dbService.UpdateTaskAsync(this);
                            });
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"自动保存失败: {ex.Message}");
                            MainWindow.LogStatic($"自动保存失败: {ex.Message}");
                        }
                    }

                    OnPropertyChanged(nameof(IsCompleted));

                    System.Diagnostics.Debug.WriteLine($"PropertyChanged 已触发: IsCompleted, 新值: {_isCompleted}");
                    MainWindow.LogStatic($"PropertyChanged 已触发: IsCompleted, 新值: {_isCompleted}");
                }
            }
        }

        public DateTime CreateTime
        {
            get => _createTime;
            set
            {
                if (_createTime != value)
                {
                    _createTime = value;
                    OnPropertyChanged(nameof(CreateTime));
                }
            }
        }

        public DateTime? CompleteTime
        {
            get => _completeTime;
            set
            {
                if (_completeTime != value)
                {
                    _completeTime = value;
                    OnPropertyChanged(nameof(CompleteTime));
                }
            }
        }

        public List<string> Tags
        {
            get => _tags;
            set
            {
                if (_tags != value)
                {
                    _tags = value;
                    OnPropertyChanged(nameof(Tags));
                }
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                if (_description != value)
                {
                    _description = value;
                    OnPropertyChanged(nameof(Description));
                }
            }
        }

        public int Priority
        {
            get => _priority;
            set
            {
                if (_priority != value)
                {
                    _priority = value;
                    OnPropertyChanged(nameof(Priority));
                }
            }
        }

        public TaskItem()
        {
            CreateTime = DateTime.Now;
            Tags = new List<string>();
            Priority = 0;
        }

        public TaskItem(string title, QuadrantType quadrant) : this()
        {
            Title = title;
            Quadrant = quadrant;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}