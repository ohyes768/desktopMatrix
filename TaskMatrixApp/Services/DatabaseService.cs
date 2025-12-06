using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;
using TaskMatrixApp.Models;
using System.Diagnostics;

namespace TaskMatrixApp.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;
        private readonly string _databasePath;

        public DatabaseService()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = System.IO.Path.Combine(appDataPath, "DesktopWidget");

            if (!System.IO.Directory.Exists(appFolder))
            {
                System.IO.Directory.CreateDirectory(appFolder);
            }

            _databasePath = System.IO.Path.Combine(appFolder, "data.db");
            _connectionString = $"Data Source={_databasePath};Cache=Shared;";

            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var createTasksTable = @"
                CREATE TABLE IF NOT EXISTS Tasks (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Title TEXT NOT NULL,
                    Quadrant INTEGER NOT NULL,
                    IsCompleted BOOLEAN DEFAULT 0,
                    Priority INTEGER DEFAULT 0,
                    CreateTime DATETIME DEFAULT CURRENT_TIMESTAMP,
                    CompleteTime DATETIME NULL,
                    Tags TEXT NULL,
                    Description TEXT NULL
                );";

            using var command = new SqliteCommand(createTasksTable, connection);
            command.ExecuteNonQuery();
        }

        public async Task<List<TaskItem>> GetAllTasksAsync()
        {
            return await Task.Run(() =>
            {
                var tasks = new List<TaskItem>();

                using var connection = new SqliteConnection(_connectionString);
                connection.Open();

                var query = "SELECT * FROM Tasks ORDER BY CreateTime DESC";
                using var command = new SqliteCommand(query, connection);
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    tasks.Add(MapReaderToTaskItem(reader));
                }

                return tasks;
            });
        }

        public async Task<List<TaskItem>> GetTasksByQuadrantAsync(QuadrantType quadrant)
        {
            var tasks = new List<TaskItem>();

            await Task.Run(() =>
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();

                var query = "SELECT * FROM Tasks WHERE Quadrant = @Quadrant ORDER BY Priority DESC, CreateTime DESC";
                using var command = new SqliteCommand(query, connection);
                command.Parameters.AddWithValue("@Quadrant", (int)quadrant);
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    tasks.Add(MapReaderToTaskItem(reader));
                }
            });

            return tasks;
        }

        public async Task<int> InsertTaskAsync(TaskItem task)
        {
            // 直接在后台线程中执行，避免死锁
            return await Task.Run(() => InsertTaskSync(task));
        }

        // 同步版本
        public int InsertTaskSync(TaskItem task)
        {
            MainWindow.LogStatic($"DatabaseService.InsertTaskSync 开始 - 数据库路径: {_databasePath}");

            int retryCount = 0;
            int maxRetries = 3;

            while (retryCount < maxRetries)
            {
                try
                {
                    using var connection = new SqliteConnection(_connectionString);
                    MainWindow.LogStatic("打开数据库连接");
                    connection.Open();

                // 先插入数据
                var insertQuery = @"
                    INSERT INTO Tasks (Title, Quadrant, IsCompleted, Priority, CreateTime, CompleteTime, Tags, Description)
                    VALUES (@Title, @Quadrant, @IsCompleted, @Priority, @CreateTime, @CompleteTime, @Tags, @Description);";

                MainWindow.LogStatic("执行INSERT命令");
                using var insertCommand = new SqliteCommand(insertQuery, connection);

                // 确保所有参数都有有效值
                insertCommand.Parameters.AddWithValue("@Title", task.Title ?? "");
                insertCommand.Parameters.AddWithValue("@Quadrant", (int)task.Quadrant);
                insertCommand.Parameters.AddWithValue("@IsCompleted", task.IsCompleted);
                insertCommand.Parameters.AddWithValue("@Priority", task.Priority);
                insertCommand.Parameters.AddWithValue("@CreateTime", task.CreateTime);
                insertCommand.Parameters.AddWithValue("@CompleteTime", task.CompleteTime.HasValue ? (object)task.CompleteTime.Value : DBNull.Value);

                // Tags参数处理
                string tagsValue = null;
                if (task.Tags != null && task.Tags.Count > 0)
                {
                    tagsValue = string.Join(",", task.Tags);
                }
                insertCommand.Parameters.AddWithValue("@Tags", tagsValue != null ? (object)tagsValue : DBNull.Value);

                // Description参数处理 - 确保不为null
                string descriptionValue = task.Description;
                if (string.IsNullOrEmpty(descriptionValue))
                {
                    descriptionValue = ""; // 空字符串而不是null
                }
                insertCommand.Parameters.AddWithValue("@Description", descriptionValue);

                // 记录参数值用于调试
                MainWindow.LogStatic($"参数值检查 - Title:'{task.Title}', Quadrant:{task.Quadrant}, IsCompleted:{task.IsCompleted}, Priority:{task.Priority}, CreateTime:{task.CreateTime}, Description:'{descriptionValue}'");

                int rowsAffected = insertCommand.ExecuteNonQuery();
                MainWindow.LogStatic($"INSERT影响行数: {rowsAffected}");

                // 然后获取最后插入的ID
                var selectQuery = "SELECT last_insert_rowid();";
                MainWindow.LogStatic("获取最后插入的ID");
                using var selectCommand = new SqliteCommand(selectQuery, connection);
                var id = Convert.ToInt32(selectCommand.ExecuteScalar());
                MainWindow.LogStatic($"数据库生成ID: {id}");
                return id;
            }
                catch (SqliteException ex) when ((ex.SqliteErrorCode == 5 || ex.SqliteErrorCode == 6) && retryCount < maxRetries - 1)
                {
                    retryCount++;
                    MainWindow.LogStatic($"数据库锁定（错误代码:{ex.SqliteErrorCode}），第{retryCount}次重试...");
                    System.Threading.Thread.Sleep(200 * retryCount); // 递增延迟
                }
                catch (Exception ex)
                {
                    MainWindow.LogStatic($"DatabaseService.InsertTaskSync 异常: {ex.Message}");
                    MainWindow.LogStatic($"DatabaseService.InsertTaskSync 堆栈: {ex.StackTrace}");
                    if (ex.InnerException != null)
                    {
                        MainWindow.LogStatic($"DatabaseService.InsertTaskSync 内部异常: {ex.InnerException.Message}");
                    }
                    throw;
                }
            }

            throw new Exception($"插入任务失败，已重试{maxRetries}次");
        }

        public async Task<bool> UpdateTaskAsync(TaskItem task)
        {
            return await Task.Run(() => UpdateTaskSync(task));
        }

        // 同步版本
        public bool UpdateTaskSync(TaskItem task)
        {
            MainWindow.LogStatic($"DatabaseService.UpdateTaskSync 开始 - 任务ID: {task.Id}, 完成状态: {task.IsCompleted}");

            int retryCount = 0;
            int maxRetries = 3;

            while (retryCount < maxRetries)
            {
                try
                {
                    using var connection = new SqliteConnection(_connectionString);
                    connection.Open();

                var query = @"
                    UPDATE Tasks SET
                        Title = @Title,
                        Quadrant = @Quadrant,
                        IsCompleted = @IsCompleted,
                        Priority = @Priority,
                        CompleteTime = @CompleteTime,
                        Tags = @Tags,
                        Description = @Description
                    WHERE Id = @Id";

                using var command = new SqliteCommand(query, connection);
                command.Parameters.AddWithValue("@Title", task.Title ?? "");
                command.Parameters.AddWithValue("@Quadrant", (int)task.Quadrant);
                command.Parameters.AddWithValue("@IsCompleted", task.IsCompleted);
                command.Parameters.AddWithValue("@Priority", task.Priority);
                command.Parameters.AddWithValue("@CompleteTime", task.CompleteTime.HasValue ? (object)task.CompleteTime.Value : DBNull.Value);

                // Tags参数处理
                string tagsValue = null;
                if (task.Tags != null && task.Tags.Count > 0)
                {
                    tagsValue = string.Join(",", task.Tags);
                }
                command.Parameters.AddWithValue("@Tags", tagsValue != null ? (object)tagsValue : DBNull.Value);

                // Description参数处理 - 确保不为null
                string descriptionValue = task.Description;
                if (string.IsNullOrEmpty(descriptionValue))
                {
                    descriptionValue = ""; // 空字符串而不是null
                }
                command.Parameters.AddWithValue("@Description", descriptionValue);
                command.Parameters.AddWithValue("@Id", task.Id);

                    int rowsAffected = command.ExecuteNonQuery();
                    MainWindow.LogStatic($"UPDATE影响行数: {rowsAffected}");

                    return rowsAffected > 0;
                }
                catch (SqliteException ex) when ((ex.SqliteErrorCode == 5 || ex.SqliteErrorCode == 6) && retryCount < maxRetries - 1)
                {
                    retryCount++;
                    MainWindow.LogStatic($"数据库锁定（错误代码:{ex.SqliteErrorCode}），第{retryCount}次重试...");
                    System.Threading.Thread.Sleep(200 * retryCount); // 递增延迟
                }
                catch (Exception ex)
                {
                    MainWindow.LogStatic($"DatabaseService.UpdateTaskSync 异常: {ex.Message}");
                    MainWindow.LogStatic($"DatabaseService.UpdateTaskSync 堆栈: {ex.StackTrace}");
                    if (ex.InnerException != null)
                    {
                        MainWindow.LogStatic($"DatabaseService.UpdateTaskSync 内部异常: {ex.InnerException.Message}");
                    }
                    throw;
                }
            }

            throw new Exception($"更新任务失败，已重试{maxRetries}次");
        }

        public async Task<bool> DeleteTaskAsync(int id)
        {
            return await Task.Run(() =>
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();

                var query = "DELETE FROM Tasks WHERE Id = @Id";
                using var command = new SqliteCommand(query, connection);
                command.Parameters.AddWithValue("@Id", id);

                return command.ExecuteNonQuery() > 0;
            });
        }

        // 同步版本
        public bool DeleteTaskSync(int id)
        {
            MainWindow.LogStatic($"DatabaseService.DeleteTaskSync 开始 - ID: {id}");

            try
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();
                MainWindow.LogStatic("打开数据库连接");

                var query = "DELETE FROM Tasks WHERE Id = @Id";
                using var command = new SqliteCommand(query, connection);
                command.Parameters.AddWithValue("@Id", id);

                var result = command.ExecuteNonQuery();
                MainWindow.LogStatic($"DELETE影响行数: {result}");

                return result > 0;
            }
            catch (Exception ex)
            {
                MainWindow.LogStatic($"DeleteTaskSync 异常: {ex.Message}");
                throw;
            }
        }

        private TaskItem MapReaderToTaskItem(SqliteDataReader reader)
        {
            var task = new TaskItem
            {
                Id = Convert.ToInt32(reader["Id"]),
                Title = reader["Title"].ToString(),
                Quadrant = (QuadrantType)Convert.ToInt32(reader["Quadrant"]),
                IsCompleted = Convert.ToBoolean(reader["IsCompleted"]),
                Priority = Convert.ToInt32(reader["Priority"]),
                CreateTime = Convert.ToDateTime(reader["CreateTime"])
            };

            if (reader["CompleteTime"] != DBNull.Value)
            {
                task.CompleteTime = Convert.ToDateTime(reader["CompleteTime"]);
            }

            if (reader["Tags"] != DBNull.Value && !string.IsNullOrEmpty(reader["Tags"].ToString()))
            {
                task.Tags = new List<string>(reader["Tags"].ToString().Split(','));
            }
            else
            {
                task.Tags = new List<string>();
            }

            task.Description = reader["Description"]?.ToString();

            return task;
        }
    }
}