using System;
using System.Collections.Generic;
using System.IO;
using DesktopMatrix.Models;
namespace DesktopMatrix.Utils
{
    /// <summary>
    /// 应用常量定义
    /// </summary>
    public static class Constants
    {
        // 应用信息
        public const string AppName = "四象限任务管理器";
        public const string AppVersion = "1.0.0";
        
        // 文件路径常量
        public static readonly string AppDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "QuadrantManager");
        public static readonly string DataFolder = Path.Combine(AppDataFolder, "data");
        public static readonly string BackupFolder = Path.Combine(DataFolder, "backup");
        public static readonly string LogFolder = Path.Combine(AppDataFolder, "logs");
        public static readonly string MainDataFile = Path.Combine(DataFolder, "main.json");
        public static readonly string ErrorLogFile = Path.Combine(LogFolder, "error.log");
        
        // 象限类型描述
        public static readonly Dictionary<QuadrantType, string> QuadrantDescriptions = new Dictionary<QuadrantType, string>
        {
            { QuadrantType.Q1, "重要且紧急" },
            { QuadrantType.Q2, "重要不紧急" },
            { QuadrantType.Q3, "紧急不重要" },
            { QuadrantType.Q4, "不重要不紧急" }
        };
        
        // 错误代码
        public const int ERROR_FILE_IO = 1001;      // 文件读写失败
        public const int ERROR_DATA_VALIDATION = 1002;  // 数据校验失败
        public const int ERROR_MEMORY_OVERFLOW = 1003;  // 内存溢出
        public const int ERROR_DRAG_CONFLICT = 1004;    // 拖拽操作冲突
        
        // 配置默认值
        public const int DEFAULT_AUTO_CLEAN_DAYS = 30;  // 默认自动清理天数
        public const int AUTO_SAVE_INTERVAL = 5;        // 自动保存间隔(分钟)
        public const int MAX_BACKUP_FILES = 10;         // 最大备份文件数量
        
        // UI相关常量
        public const double TASK_CARD_MIN_HEIGHT = 80;
        public const double TASK_CARD_MAX_HEIGHT = 200;
        public const double QUADRANT_MIN_WIDTH = 300;
        public const double QUADRANT_MIN_HEIGHT = 300;
    }
}