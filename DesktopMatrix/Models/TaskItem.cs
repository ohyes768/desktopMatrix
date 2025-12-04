using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DesktopMatrix.Models
{
    /// <summary>
    /// 任务项模型类
    /// </summary>
    public class TaskItem : INotifyPropertyChanged
    {
        private Guid _id;
        private string _title;
        private QuadrantType _quadrant;
        private DateTime _createTime;
        private bool _isCompleted;
        private DateTime? _completeTime;
        private List<string> _tags;

        /// <summary>
        /// 任务唯一标识
        /// </summary>
        public Guid Id
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 任务标题
        /// </summary>
        public string Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 任务所属象限
        /// </summary>
        public QuadrantType Quadrant
        {
            get => _quadrant;
            set
            {
                if (_quadrant != value)
                {
                    _quadrant = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 任务创建时间
        /// </summary>
        public DateTime CreateTime
        {
            get => _createTime;
            set
            {
                if (_createTime != value)
                {
                    _createTime = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 任务是否完成
        /// </summary>
        public bool IsCompleted
        {
            get => _isCompleted;
            set
            {
                if (_isCompleted != value)
                {
                    _isCompleted = value;
                    if (value)
                    {
                        CompleteTime = DateTime.Now;
                    }
                    else
                    {
                        CompleteTime = null;
                    }
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 任务完成时间
        /// </summary>
        public DateTime? CompleteTime
        {
            get => _completeTime;
            set
            {
                if (_completeTime != value)
                {
                    _completeTime = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 任务标签
        /// </summary>
        public List<string> Tags
        {
            get => _tags;
            set
            {
                if (_tags != value)
                {
                    _tags = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public TaskItem()
        {
            Id = Guid.NewGuid();
            CreateTime = DateTime.Now;
            Tags = new List<string>();
        }

        /// <summary>
        /// 带参数的构造函数
        /// </summary>
        public TaskItem(string title, QuadrantType quadrant)
        {
            Id = Guid.NewGuid();
            Title = title;
            Quadrant = quadrant;
            CreateTime = DateTime.Now;
            IsCompleted = false;
            Tags = new List<string>();
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