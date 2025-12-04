using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using DesktopMatrix.Models;
using DesktopMatrix.Services;

namespace DesktopMatrix.ViewModels
{
    /// <summary>
    /// 主窗口视图模型
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ITaskManager _taskManager;
        private string _newTaskTitle;
        private QuadrantType _selectedQuadrant;

        /// <summary>
        /// 第一象限任务集合
        /// </summary>
        public ObservableCollection<TaskItem> Q1Tasks => _taskManager.Q1Tasks;

        /// <summary>
        /// 第二象限任务集合
        /// </summary>
        public ObservableCollection<TaskItem> Q2Tasks => _taskManager.Q2Tasks;

        /// <summary>
        /// 第三象限任务集合
        /// </summary>
        public ObservableCollection<TaskItem> Q3Tasks => _taskManager.Q3Tasks;

        /// <summary>
        /// 第四象限任务集合
        /// </summary>
        public ObservableCollection<TaskItem> Q4Tasks => _taskManager.Q4Tasks;

        /// <summary>
        /// 新任务标题
        /// </summary>
        public string NewTaskTitle
        {
            get => _newTaskTitle;
            set
            {
                if (_newTaskTitle != value)
                {
                    _newTaskTitle = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 当前选中的象限
        /// </summary>
        public QuadrantType SelectedQuadrant
        {
            get => _selectedQuadrant;
            set
            {
                if (_selectedQuadrant != value)
                {
                    _selectedQuadrant = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 添加任务命令
        /// </summary>
        public ICommand AddTaskCommand { get; private set; }

        /// <summary>
        /// 删除任务命令
        /// </summary>
        public ICommand DeleteTaskCommand { get; private set; }

        /// <summary>
        /// 移动任务命令
        /// </summary>
        public ICommand MoveTaskCommand { get; private set; }

        /// <summary>
        /// 切换任务完成状态命令
        /// </summary>
        public ICommand ToggleCompletionCommand { get; private set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public MainViewModel(ITaskManager taskManager)
        {
            _taskManager = taskManager;
            SelectedQuadrant = QuadrantType.Q1;

            // 初始化命令
            AddTaskCommand = new RelayCommand(AddTask, CanAddTask);
            DeleteTaskCommand = new RelayCommand<Guid>(DeleteTask);
            MoveTaskCommand = new RelayCommand<Tuple<Guid, QuadrantType>>(MoveTask);
            ToggleCompletionCommand = new RelayCommand<Guid>(ToggleTaskCompletion);

            // 初始化任务管理器
            InitializeAsync().Wait();

            // 添加测试数据
            _taskManager.AddTask("完成紧急报告", QuadrantType.Q1);
            _taskManager.AddTask("处理客户投诉", QuadrantType.Q1);
            _taskManager.AddTask("制定年度计划", QuadrantType.Q2);
            _taskManager.AddTask("学习新技术", QuadrantType.Q2);
            _taskManager.AddTask("回复邮件", QuadrantType.Q3);
            _taskManager.AddTask("日常会议", QuadrantType.Q3);
            _taskManager.AddTask("整理文档", QuadrantType.Q4);
            _taskManager.AddTask("查看新闻", QuadrantType.Q4);
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private async Task InitializeAsync()
        {
            await _taskManager.InitializeAsync();
        }

        /// <summary>
        /// 添加任务
        /// </summary>
        private void AddTask()
        {
            if (CanAddTask())
            {
                _taskManager.AddTask(NewTaskTitle, SelectedQuadrant);
                NewTaskTitle = string.Empty;
            }
        }

        /// <summary>
        /// 判断是否可以添加任务
        /// </summary>
        private bool CanAddTask()
        {
            return !string.IsNullOrWhiteSpace(NewTaskTitle);
        }

        /// <summary>
        /// 删除任务
        /// </summary>
        private void DeleteTask(Guid taskId)
        {
            _taskManager.DeleteTask(taskId);
        }

        /// <summary>
        /// 移动任务
        /// </summary>
        private void MoveTask(Tuple<Guid, QuadrantType> parameters)
        {
            if (parameters != null)
            {
                _taskManager.MoveTask(parameters.Item1, parameters.Item2);
            }
        }

        /// <summary>
        /// 切换任务完成状态
        /// </summary>
        private void ToggleTaskCompletion(Guid taskId)
        {
            var task = _taskManager.Q1Tasks.FirstOrDefault(t => t.Id == taskId) ??
                       _taskManager.Q2Tasks.FirstOrDefault(t => t.Id == taskId) ??
                       _taskManager.Q3Tasks.FirstOrDefault(t => t.Id == taskId) ??
                       _taskManager.Q4Tasks.FirstOrDefault(t => t.Id == taskId);

            if (task != null)
            {
                task.IsCompleted = !task.IsCompleted;
                _taskManager.SaveChangesAsync();
            }
        }

        #region INotifyPropertyChanged 实现

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    /// <summary>
    /// 命令实现类
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute();
        }

        public void Execute(object parameter)
        {
            _execute();
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }

    /// <summary>
    /// 带参数的命令实现类
    /// </summary>
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Predicate<T> _canExecute;

        public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute((T)parameter);
        }

        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}