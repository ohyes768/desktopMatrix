using System;
using System.Collections.Generic;

namespace DesktopMatrix.Models
{
    /// <summary>
    /// 应用配置类
    /// </summary>
    public class AppConfig
    {
        /// <summary>
        /// 自动清理天数
        /// </summary>
        public int AutoCleanDays { get; set; } = 30;

        /// <summary>
        /// 最后备份时间
        /// </summary>
        public DateTime LastBackupTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 任务数据存储模型
    /// </summary>
    public class TaskData
    {
        /// <summary>
        /// 数据版本号
        /// </summary>
        public string Version { get; set; } = "1.0";

        /// <summary>
        /// 任务列表
        /// </summary>
        public List<TaskItem> Tasks { get; set; } = new List<TaskItem>();

        /// <summary>
        /// 应用配置
        /// </summary>
        public AppConfig Config { get; set; } = new AppConfig();
    }

    /// <summary>
    /// 数据变更类型枚举
    /// </summary>
    public enum ChangeType
    {
        Add,
        Update,
        Delete,
        Move,
        Restore
    }

    /// <summary>
    /// 数据变更事件参数
    /// </summary>
    public class DataChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 变更类型
        /// </summary>
        public ChangeType ChangeType { get; set; }

        /// <summary>
        /// 受影响的任务ID列表
        /// </summary>
        public Guid[] AffectedTaskIds { get; set; }

        public DataChangedEventArgs(ChangeType changeType, params Guid[] taskIds)
        {
            ChangeType = changeType;
            AffectedTaskIds = taskIds;
        }
    }
}