using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using TaskMatrixApp.Models;
using TaskMatrixApp.Services;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Interop;
using System.IO;

namespace TaskMatrixApp
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly TaskManager _taskManager;
        private DispatcherTimer _autoSaveTimer;
        private QuadrantType _selectedQuadrant = QuadrantType.Q1;
        private TrayService _trayService;
        private HotkeyService _hotkeyService;

        // 窗口位置绑定属性
        public WindowPosition WindowPosition
        {
            get => _windowPosition;
            set
            {
                if (_windowPosition != value)
                {
                    _windowPosition = value;
                    OnPropertyChanged(nameof(WindowPosition));
                }
            }
        }
        private WindowPosition _windowPosition;

        public MainWindow()
        {
            InitializeComponent();

            // 初始化日志
            InitializeLogger();
            LogMessage("应用程序启动");

            // 设置窗口位置到屏幕中央偏右，确保完全可见
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;

            this.Left = screenWidth - this.Width - 40; // 距离右边40px
            this.Top = 60; // 距离顶部60px

            // 初始化任务管理器
            _taskManager = new TaskManager();

            // 设置TaskItem的数据库服务引用，用于自动保存
            TaskItem._dbService = _taskManager.GetType()
                .GetField("_dbService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(_taskManager) as TaskMatrixApp.Services.DatabaseService;

            DataContext = this;

            // 初始化象限选择按钮状态
            UpdateQuadrantButtonStyles();

            // 初始化自动保存定时器
            _autoSaveTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(1) // 每分钟自动保存
            };
            _autoSaveTimer.Tick += AutoSaveTimer_Tick;
            _autoSaveTimer.Start();

            this.Loaded += MainWindow_Loaded;
        }

        private void InitializeLogger()
        {
            try
            {
                var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "DesktopWidget_log.txt");
                File.WriteAllText(logPath, $"=== DesktopWidget 日志 {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===\n");
                LogMessage($"日志文件初始化: {logPath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"日志初始化失败: {ex.Message}");
            }
        }

        private void LogMessage(string message)
        {
            try
            {
                var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "DesktopWidget_log.txt");
                var logEntry = $"[{DateTime.Now:HH:mm:ss}] {message}\n";
                File.AppendAllText(logPath, logEntry);
                System.Diagnostics.Debug.WriteLine(logEntry.Trim());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"写入日志失败: {ex.Message}, 原消息: {message}");
            }
        }

        // 静态日志方法，供其他类使用
        public static void LogStatic(string message)
        {
            try
            {
                var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "DesktopWidget_log.txt");
                var logEntry = $"[{DateTime.Now:HH:mm:ss}] {message}\n";
                File.AppendAllText(logPath, logEntry);
                System.Diagnostics.Debug.WriteLine(logEntry.Trim());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"写入静态日志失败: {ex.Message}, 原消息: {message}");
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // 获取窗口句柄用于注册热键
            var helper = new WindowInteropHelper(this);
            HwndSource source = HwndSource.FromHwnd(helper.Handle);
            source?.AddHook(HwndHook);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x312;

            if (msg == WM_HOTKEY)
            {
                _hotkeyService?.ProcessHotkeyMessage(wParam);
                handled = true;
            }

            return IntPtr.Zero;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await _taskManager.InitializeAsync();
            InitializeSystemTray();
            InitializeHotkeys();
        }

        private void SetInitialPosition()
        {
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;

            // 确保窗口完全在屏幕内可见，留出更多边距
            WindowPosition = new WindowPosition
            {
                X = Math.Max(20, screenWidth - this.Width - 40),
                Y = Math.Max(20, 60)  // Y=60 确保不会在屏幕顶部被遮挡
            };
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            // 关闭窗口时隐藏到系统托盘而不是退出
            this.Hide();
        }

        private void CleanupButton_Click(object sender, RoutedEventArgs e)
        {
            LogMessage("开始清理已完成的任务");

            var completedTasks = new List<TaskItem>();

            // 收集所有已完成的任务
            completedTasks.AddRange(_taskManager.Q1Tasks.Where(t => t.IsCompleted));
            completedTasks.AddRange(_taskManager.Q2Tasks.Where(t => t.IsCompleted));
            completedTasks.AddRange(_taskManager.Q3Tasks.Where(t => t.IsCompleted));
            completedTasks.AddRange(_taskManager.Q4Tasks.Where(t => t.IsCompleted));

            LogMessage($"找到 {completedTasks.Count} 个已完成的任务");

            if (completedTasks.Count == 0)
            {
                System.Windows.MessageBox.Show("没有已完成的任务需要清理。", "清理完成", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // 确认对话框
            var result = System.Windows.MessageBox.Show(
                $"确定要删除 {completedTasks.Count} 个已完成的任务吗？\n\n此操作无法撤销。",
                "确认清理",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                int deletedCount = 0;

                foreach (var task in completedTasks)
                {
                    try
                    {
                        LogMessage($"删除任务: ID={task.Id}, 标题='{task.Title}', 象限={task.Quadrant}");

                        // 从数据库删除
                        _taskManager.DeleteTask(task.Id);

                        // 从界面集合中删除
                        switch (task.Quadrant)
                        {
                            case QuadrantType.Q1:
                                _taskManager.Q1Tasks.Remove(task);
                                break;
                            case QuadrantType.Q2:
                                _taskManager.Q2Tasks.Remove(task);
                                break;
                            case QuadrantType.Q3:
                                _taskManager.Q3Tasks.Remove(task);
                                break;
                            case QuadrantType.Q4:
                                _taskManager.Q4Tasks.Remove(task);
                                break;
                        }

                        deletedCount++;
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"删除任务失败: ID={task.Id}, 错误={ex.Message}");
                    }
                }

                LogMessage($"清理完成，成功删除 {deletedCount} 个任务");
                System.Windows.MessageBox.Show($"成功清理了 {deletedCount} 个已完成的任务！", "清理完成", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                LogMessage("用户取消了清理操作");
            }
        }

        #region 任务管理

        // 任务集合
        public System.Collections.ObjectModel.ObservableCollection<TaskItem> Q1Tasks => _taskManager.Q1Tasks;
        public System.Collections.ObjectModel.ObservableCollection<TaskItem> Q2Tasks => _taskManager.Q2Tasks;
        public System.Collections.ObjectModel.ObservableCollection<TaskItem> Q3Tasks => _taskManager.Q3Tasks;
        public System.Collections.ObjectModel.ObservableCollection<TaskItem> Q4Tasks => _taskManager.Q4Tasks;

        // 新任务标题
        public string NewTaskTitle { get; set; } = "";

        // 添加任务命令
        private ICommand _addTaskCommand;
        public ICommand AddTaskCommand
        {
            get
            {
                if (_addTaskCommand == null)
                {
                    _addTaskCommand = new RelayCommand(async () =>
                    {
                        if (!string.IsNullOrWhiteSpace(NewTaskTitle))
                        {
                            await _taskManager.AddTaskAsync(NewTaskTitle, _selectedQuadrant);
                            NewTaskTitle = "";
                        }
                    });
                }
                return _addTaskCommand;
            }
        }

        // 切换任务完成状态命令
        private ICommand _toggleTaskCommand;
        public ICommand ToggleTaskCommand
        {
            get
            {
                if (_toggleTaskCommand == null)
                {
                    _toggleTaskCommand = new RelayCommand<int>((taskId) =>
                    {
                        LogMessage($"切换任务完成状态 - 任务ID: {taskId}");
                        try
                        {
                            bool success = _taskManager.ToggleTaskCompletionSync(taskId);
                            if (!success)
                            {
                                LogMessage($"切换任务完成状态失败 - 任务ID: {taskId}");
                            }
                        }
                        catch (Exception ex)
                        {
                            LogMessage($"切换任务完成状态异常: {ex.Message}");
                            LogMessage($"异常堆栈: {ex.StackTrace}");
                        }
                    });
                }
                return _toggleTaskCommand;
            }
        }

        #endregion

        #region 添加任务对话框

        private void AddTaskCommand_Executed(object sender, EventArgs e)
        {
            // 显示添加任务面板
            AddTaskPanel.Visibility = Visibility.Visible;
            TaskTitleTextBox.Text = "";
            TaskTitleTextBox.Focus();

            // 设置默认象限选中状态
            UpdateQuadrantButtonStyles();
        }

        #region 系统托盘和快捷键

        private void InitializeSystemTray()
        {
            _trayService = new TrayService(this);
            _trayService.ShowWindowRequested += (s, e) => this.Show();
            _trayService.ExitRequested += (s, e) => this.Close();
        }

        private void InitializeHotkeys()
        {
            var windowInteropHelper = new WindowInteropHelper(this);
            var handle = windowInteropHelper.Handle;

            _hotkeyService = new HotkeyService(handle);

            // 注册快捷键：Ctrl+Shift+D 显示/隐藏窗口
            _hotkeyService.RegisterHotkey(System.Windows.Forms.Keys.D, ctrl: true, shift: true, callback: () =>
            {
                if (this.Visibility == Visibility.Visible)
                {
                    this.Hide();
                }
                else
                {
                    this.Show();
                    this.Activate();
                }
            });

            // 注册快捷键：Ctrl+Shift+A 添加任务
            _hotkeyService.RegisterHotkey(System.Windows.Forms.Keys.A, ctrl: true, shift: true, callback: () =>
            {
                if (this.Visibility != Visibility.Visible)
                {
                    this.Show();
                    this.Activate();
                }
                AddTaskCommand_Executed(null, EventArgs.Empty);
            });
        }

        #endregion

        private void SelectQuadrantButton_Click(object sender, RoutedEventArgs e)
        {
            var clickedButton = sender as System.Windows.Controls.Button;

            LogMessage($"点击了按钮: {clickedButton.Name}, 内容: {clickedButton.Content}");

            // 根据按钮名称直接判断象限，而不是依赖文本内容
            _selectedQuadrant = clickedButton.Name switch
            {
                "Q1Button" => QuadrantType.Q1,
                "Q2Button" => QuadrantType.Q2,
                "Q3Button" => QuadrantType.Q3,
                "Q4Button" => QuadrantType.Q4,
                _ => QuadrantType.Q1
            };

            LogMessage($"选中象限更新为: {_selectedQuadrant}");

            // 更新按钮样式状态
            UpdateQuadrantButtonStyles();
        }

        private void UpdateQuadrantButtonStyles()
        {
            LogMessage($"更新按钮样式，当前选中象限: {_selectedQuadrant}");

            // 重置所有按钮为默认状态
            ResetButtonStyle(Q1Button);
            ResetButtonStyle(Q2Button);
            ResetButtonStyle(Q3Button);
            ResetButtonStyle(Q4Button);

            // 设置选中的按钮为高亮状态
            switch (_selectedQuadrant)
            {
                case QuadrantType.Q1:
                    LogMessage("高亮 Q1 按钮");
                    HighlightButton(Q1Button);
                    break;
                case QuadrantType.Q2:
                    LogMessage("高亮 Q2 按钮");
                    HighlightButton(Q2Button);
                    break;
                case QuadrantType.Q3:
                    LogMessage("高亮 Q3 按钮");
                    HighlightButton(Q3Button);
                    break;
                case QuadrantType.Q4:
                    LogMessage("高亮 Q4 按钮");
                    HighlightButton(Q4Button);
                    break;
            }
        }

        private void ResetButtonStyle(System.Windows.Controls.Button button)
        {
            // 获取对应的 Border
            Border parentBorder = null;
            switch (button.Name)
            {
                case "Q1Button":
                    parentBorder = Q1Border;
                    break;
                case "Q2Button":
                    parentBorder = Q2Border;
                    break;
                case "Q3Button":
                    parentBorder = Q3Border;
                    break;
                case "Q4Button":
                    parentBorder = Q4Border;
                    break;
            }

            if (parentBorder != null)
            {
                // 恢复原始背景色
                switch (button.Name)
                {
                    case "Q1Button":
                        parentBorder.Background = FindResource("Q1ButtonBrush") as SolidColorBrush;
                        break;
                    case "Q2Button":
                        parentBorder.Background = FindResource("Q2ButtonBrush") as SolidColorBrush;
                        break;
                    case "Q3Button":
                        parentBorder.Background = FindResource("Q3ButtonBrush") as SolidColorBrush;
                        break;
                    case "Q4Button":
                        parentBorder.Background = FindResource("Q4ButtonBrush") as SolidColorBrush;
                        break;
                }

                // 恢复未选中状态的边框
                parentBorder.BorderBrush = FindResource("UnselectedBorderBrush") as SolidColorBrush;
                parentBorder.BorderThickness = new Thickness(2);

                // 清除效果和变换
                parentBorder.Effect = null;
                parentBorder.RenderTransform = null;
            }

            // 恢复按钮样式
            button.FontWeight = FontWeights.Medium;
            button.Effect = null;
            button.RenderTransform = null;
        }

        private void HighlightButton(System.Windows.Controls.Button button)
        {
            // 获取对应的 Border
            Border parentBorder = null;
            switch (button.Name)
            {
                case "Q1Button":
                    parentBorder = Q1Border;
                    break;
                case "Q2Button":
                    parentBorder = Q2Border;
                    break;
                case "Q3Button":
                    parentBorder = Q3Border;
                    break;
                case "Q4Button":
                    parentBorder = Q4Border;
                    break;
            }

            if (parentBorder != null)
            {
                // 设置高亮背景色
                switch (button.Name)
                {
                    case "Q1Button":
                        parentBorder.Background = FindResource("Q1HighlightBrush") as SolidColorBrush;
                        break;
                    case "Q2Button":
                        parentBorder.Background = FindResource("Q2HighlightBrush") as SolidColorBrush;
                        break;
                    case "Q3Button":
                        parentBorder.Background = FindResource("Q3HighlightBrush") as SolidColorBrush;
                        break;
                    case "Q4Button":
                        parentBorder.Background = FindResource("Q4HighlightBrush") as SolidColorBrush;
                        break;
                }

                // 设置选中状态的白色边框
                parentBorder.BorderBrush = FindResource("SelectedBorderBrush") as SolidColorBrush;
                parentBorder.BorderThickness = new Thickness(3);

                // 添加强烈的阴影效果
                parentBorder.Effect = new DropShadowEffect
                {
                    Color = System.Windows.Media.Color.FromRgb(0, 0, 0),
                    BlurRadius = 15,
                    ShadowDepth = 3,
                    Opacity = 0.6
                };

                // 添加明显的缩放效果
                parentBorder.RenderTransform = new ScaleTransform(1.15, 1.15);
                parentBorder.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);

                // 添加按钮内部的发光效果
                button.Effect = new DropShadowEffect
                {
                    Color = System.Windows.Media.Color.FromRgb(255, 255, 255),
                    BlurRadius = 8,
                    ShadowDepth = 0,
                    Opacity = 0.6
                };
            }

            // 加粗按钮文字
            button.FontWeight = FontWeights.Bold;
        }

        private async void ConfirmAddTask_Click(object sender, RoutedEventArgs e)
        {
            LogMessage("开始添加任务");
            try
            {
                string taskTitle = TaskTitleTextBox.Text?.Trim();
                LogMessage($"任务标题: '{taskTitle}', 选中象限: {_selectedQuadrant}");

                if (!string.IsNullOrWhiteSpace(taskTitle))
                {
                    LogMessage("调用 TaskManager.AddTaskSync");

                    // 先关闭对话框，提升用户体验
                    AddTaskPanel.Visibility = Visibility.Collapsed;
                    TaskTitleTextBox.Text = "";

                    // 异步执行添加操作
                    var task = await Task.Run(() => _taskManager.AddTaskSync(taskTitle, _selectedQuadrant));

                    if (task != null)
                    {
                        LogMessage($"任务添加成功！标题: {task.Title}, ID: {task.Id}, 象限: {task.Quadrant}");
                    }
                    else
                    {
                        LogMessage("任务添加失败：TaskManager 返回 null");
                        System.Windows.MessageBox.Show("任务添加失败", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    LogMessage("任务标题为空，不添加任务");
                    AddTaskPanel.Visibility = Visibility.Collapsed;
                    TaskTitleTextBox.Text = "";
                }
            }
            catch (Exception ex)
            {
                LogMessage($"添加任务时发生异常：{ex.Message}");
                LogMessage($"异常类型: {ex.GetType().Name}");
                LogMessage($"堆栈跟踪：{ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    LogMessage($"内部异常：{ex.InnerException.Message}");
                }
                System.Windows.MessageBox.Show($"添加任务时发生错误：{ex.Message}\n\n详细日志请查看桌面的 DesktopWidget_log.txt", "错误", MessageBoxButton.OK, MessageBoxImage.Error);

                // 确保对话框关闭
                AddTaskPanel.Visibility = Visibility.Collapsed;
                TaskTitleTextBox.Text = "";
            }
        }

        private void CancelAddTask_Click(object sender, RoutedEventArgs e)
        {
            AddTaskPanel.Visibility = Visibility.Collapsed;
        }

        
    
        #endregion

        #region 窗口位置管理

        protected override void OnLocationChanged(EventArgs e)
        {
            base.OnLocationChanged(e);
            _windowPosition = new WindowPosition { X = Left, Y = Top };
            OnPropertyChanged(nameof(WindowPosition));
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            // 保存窗口位置
            SaveWindowPosition();

            // 停止自动保存定时器
            _autoSaveTimer.Stop();

            // 释放系统托盘和快捷键资源
            _trayService?.Dispose();
            _hotkeyService?.Dispose();

            base.OnClosing(e);
        }

        private void SaveWindowPosition()
        {
            // 这里可以实现保存窗口位置到配置文件
            // 暂时先使用简单的实现
        }

        #endregion

        #region 自动保存

        private async void AutoSaveTimer_Tick(object sender, EventArgs e)
        {
            // 触发自动保存逻辑
            // 可以在这里实现数据备份等操作
            await Task.CompletedTask;
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    public class WindowPosition
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

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

        public void Execute(object parameter) => _execute();

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            if (parameter is T typedParameter)
            {
                return _canExecute == null || _canExecute(typedParameter);
            }
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is T typedParameter)
            {
                _execute(typedParameter);
            }
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}