using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using DesktopMatrix.Models;

namespace DesktopMatrix.Services
{
    /// <summary>
    /// 数据服务实现类
    /// </summary>
    public class DataService : IDataService
    {
        private readonly IEncryptionService _encryptionService;
        private readonly string _appDataFolder;
        private readonly string _dataFolder;
        private readonly string _backupFolder;
        private readonly string _logFolder;
        private readonly string _mainDataFile;
        private readonly string _errorLogFile;

        public event EventHandler<DataChangedEventArgs> DataChanged;

        public DataService(IEncryptionService encryptionService)
        {
            _encryptionService = encryptionService;
            
            // 设置应用数据目录
            _appDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "QuadrantManager");
            
            _dataFolder = Path.Combine(_appDataFolder, "data");
            _backupFolder = Path.Combine(_dataFolder, "backup");
            _logFolder = Path.Combine(_appDataFolder, "logs");
            _mainDataFile = Path.Combine(_dataFolder, "main.json");
            _errorLogFile = Path.Combine(_logFolder, "error.log");

            // 确保目录存在
            EnsureDirectoriesExist();
        }

        /// <summary>
        /// 确保所需目录存在
        /// </summary>
        private void EnsureDirectoriesExist()
        {
            Directory.CreateDirectory(_appDataFolder);
            Directory.CreateDirectory(_dataFolder);
            Directory.CreateDirectory(_backupFolder);
            Directory.CreateDirectory(_logFolder);
        }

        /// <summary>
        /// 保存数据
        /// </summary>
        public async Task SaveDataAsync(TaskData data)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                string jsonData = JsonSerializer.Serialize(data, options);
                string encryptedData = _encryptionService.Encrypt(jsonData);

                // 使用临时文件防止写入过程中的崩溃导致数据损坏
                string tempFile = _mainDataFile + ".tmp";
                await File.WriteAllTextAsync(tempFile, encryptedData, Encoding.UTF8);
                
                // 如果存在旧文件，先删除
                if (File.Exists(_mainDataFile))
                {
                    File.Delete(_mainDataFile);
                }
                
                // 重命名临时文件
                File.Move(tempFile, _mainDataFile);
                
                // 更新备份时间
                data.Config.LastBackupTime = DateTime.Now;
                
                // 触发数据变更事件
                OnDataChanged(new DataChangedEventArgs(ChangeType.Update));
            }
            catch (Exception ex)
            {
                await LogErrorAsync("保存数据失败", ex);
                throw;
            }
        }

        /// <summary>
        /// 加载数据
        /// </summary>
        public async Task<TaskData> LoadDataAsync()
        {
            try
            {
                // 如果主数据文件不存在，返回新的数据对象
                if (!File.Exists(_mainDataFile))
                {
                    return new TaskData();
                }

                string encryptedData = await File.ReadAllTextAsync(_mainDataFile, Encoding.UTF8);
                string jsonData = _encryptionService.Decrypt(encryptedData);

                var data = JsonSerializer.Deserialize<TaskData>(jsonData);
                
                // 数据校验
                if (data == null || data.Tasks == null)
                {
                    await LogErrorAsync("数据校验失败", new Exception("数据格式无效"));
                    return new TaskData();
                }

                return data;
            }
            catch (Exception ex)
            {
                await LogErrorAsync("加载数据失败", ex);
                
                // 尝试从备份恢复
                return await TryRecoverFromBackupAsync() ?? new TaskData();
            }
        }

        /// <summary>
        /// 备份数据
        /// </summary>
        public async Task BackupDataAsync()
        {
            try
            {
                if (!File.Exists(_mainDataFile))
                {
                    return;
                }

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmm");
                string backupFile = Path.Combine(_backupFolder, $"{timestamp}.bak");
                
                // 复制主数据文件到备份
                File.Copy(_mainDataFile, backupFile, true);
                
                // 清理过期备份
                await CleanOldBackupsAsync();
            }
            catch (Exception ex)
            {
                await LogErrorAsync("备份数据失败", ex);
            }
        }

        /// <summary>
        /// 清理过期备份
        /// </summary>
        private async Task CleanOldBackupsAsync()
        {
            try
            {
                var backupFiles = Directory.GetFiles(_backupFolder, "*.bak");
                
                // 保留最近10个备份
                if (backupFiles.Length <= 10)
                {
                    return;
                }

                // 按创建时间排序
                Array.Sort(backupFiles, (a, b) =>
                {
                    return File.GetCreationTime(b).CompareTo(File.GetCreationTime(a));
                });

                // 删除旧备份
                for (int i = 10; i < backupFiles.Length; i++)
                {
                    File.Delete(backupFiles[i]);
                }
            }
            catch (Exception ex)
            {
                await LogErrorAsync("清理备份失败", ex);
            }
        }

        /// <summary>
        /// 尝试从备份恢复
        /// </summary>
        private async Task<TaskData> TryRecoverFromBackupAsync()
        {
            try
            {
                var backupFiles = Directory.GetFiles(_backupFolder, "*.bak");
                if (backupFiles.Length == 0)
                {
                    return null;
                }

                // 按创建时间排序，获取最新备份
                Array.Sort(backupFiles, (a, b) =>
                {
                    return File.GetCreationTime(b).CompareTo(File.GetCreationTime(a));
                });

                string latestBackup = backupFiles[0];
                string encryptedData = await File.ReadAllTextAsync(latestBackup, Encoding.UTF8);
                string jsonData = _encryptionService.Decrypt(encryptedData);

                var data = JsonSerializer.Deserialize<TaskData>(jsonData);
                
                // 恢复成功，保存到主数据文件
                await File.WriteAllTextAsync(_mainDataFile, encryptedData, Encoding.UTF8);
                
                await LogErrorAsync("已从备份恢复数据", null);
                
                return data;
            }
            catch (Exception ex)
            {
                await LogErrorAsync("从备份恢复失败", ex);
                return null;
            }
        }

        /// <summary>
        /// 记录错误日志
        /// </summary>
        private async Task LogErrorAsync(string message, Exception ex)
        {
            try
            {
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
                if (ex != null)
                {
                    logEntry += $"\r\n{ex.Message}\r\n{ex.StackTrace}\r\n";
                }
                logEntry += "\r\n";

                await File.AppendAllTextAsync(_errorLogFile, logEntry, Encoding.UTF8);
            }
            catch
            {
                // 日志记录失败，忽略异常
            }
        }

        /// <summary>
        /// 触发数据变更事件
        /// </summary>
        protected virtual void OnDataChanged(DataChangedEventArgs e)
        {
            DataChanged?.Invoke(this, e);
        }

        /// <summary>
        /// 从指定备份文件恢复数据
        /// </summary>
        public async Task<bool> RestoreFromBackupAsync(string backupFile)
        {
            try
            {
                string backupPath = Path.Combine(_backupFolder, backupFile);
                if (!File.Exists(backupPath))
                {
                    await LogErrorAsync($"备份文件不存在: {backupFile}", null);
                    return false;
                }

                string encryptedData = await File.ReadAllTextAsync(backupPath, Encoding.UTF8);
                string jsonData = _encryptionService.Decrypt(encryptedData);

                // 验证数据格式
                var data = JsonSerializer.Deserialize<TaskData>(jsonData);
                if (data == null)
                {
                    await LogErrorAsync($"备份文件格式无效: {backupFile}", null);
                    return false;
                }

                // 恢复到主数据文件
                await File.WriteAllTextAsync(_mainDataFile, encryptedData, Encoding.UTF8);
                await LogErrorAsync($"已从备份文件恢复: {backupFile}", null);

                OnDataChanged(new DataChangedEventArgs(ChangeType.Restore));
                return true;
            }
            catch (Exception ex)
            {
                await LogErrorAsync($"从备份恢复失败: {backupFile}", ex);
                return false;
            }
        }
    }
}