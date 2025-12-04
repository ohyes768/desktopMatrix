using System.Threading.Tasks;
using DesktopMatrix.Models;

namespace DesktopMatrix.Services
{
    /// <summary>
    /// 数据服务接口
    /// </summary>
    public interface IDataService
    {
        /// <summary>
        /// 加载任务数据
        /// </summary>
        Task<TaskData> LoadDataAsync();

        /// <summary>
        /// 保存任务数据
        /// </summary>
        Task SaveDataAsync(TaskData data);

        /// <summary>
        /// 备份数据
        /// </summary>
        Task BackupDataAsync();

        /// <summary>
        /// 恢复数据
        /// </summary>
        Task<bool> RestoreFromBackupAsync(string backupFile);
    }
}