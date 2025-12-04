using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using DesktopMatrix.Models;
using DesktopMatrix.Utils;

namespace DesktopMatrix.ViewModels
{
    /// <summary>
    /// 任务卡片视图模型
    /// </summary>
    public class TaskViewModel : INotifyPropertyChanged
    {
        private readonly TaskItem _taskItem;

        /// <summary>
        /// 任务ID
        /// </summary>
        public Guid Id => _taskItem.Id;

        /// <summary>
        /// 任务标题
        /// </summary>
        public string Title
        {
            get => _taskItem.Title;
            set
            {
                if (_taskItem.Title != value)
                {
                    _taskItem.Title = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 任务所属象限
        /// </summary>
        public QuadrantType Quadrant => _taskItem.Quadrant;

        /// <summary>
        /// 象限显示名称
        /// </summary>
        public string QuadrantName => Quadrant.GetDisplayName();

        /// <summary>
        /// 象限背景色
        /// </summary>
        public SolidColorBrush QuadrantBrush => Quadrant.GetQuadrantBrush();

        /// <summary>
        /// 任务创建时间
        /// </summary>
        public DateTime CreateTime => _taskItem.CreateTime;

        /// <summary>
        /// 友好的创建时间显示
        /// </summary>
        public string FriendlyCreateTime => _taskItem.CreateTime.ToFriendlyTimeString();

        /// <summary>
        /// 任务是否完成
        /// </summary>
        public bool IsCompleted
        {
            get => _taskItem.IsCompleted;
            set
            {
                if (_taskItem.IsCompleted != value)
                {
                    _taskItem.IsCompleted = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CompletionStatus));
                }
            }
        }

        /// <summary>
        /// 完成状态文本
        /// </summary>
        public string CompletionStatus => IsCompleted ? "已完成" : "未完成";

        /// <summary>
        /// 删除任务命令
        /// </summary>
        public ICommand DeleteCommand { get; }

        /// <summary>
        /// 切换完成状态命令
        /// </summary>
        public ICommand ToggleCompletionCommand { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public TaskViewModel(TaskItem taskItem, ICommand deleteCommand, ICommand toggleCompletionCommand)
        {
            _taskItem = taskItem ?? throw new ArgumentNullException(nameof(taskItem));
            DeleteCommand = deleteCommand;
            ToggleCompletionCommand = toggleCompletionCommand;

            // 监听原始任务项的属性变更
            _taskItem.PropertyChanged += (s, e) =>
            {
                OnPropertyChanged(e.PropertyName);
                
                // 特殊处理某些属性
                if (e.PropertyName == nameof(TaskItem.IsCompleted))
                {
                    OnPropertyChanged(nameof(CompletionStatus));
                }
            };
        }

        #region INotifyPropertyChanged 实现

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}