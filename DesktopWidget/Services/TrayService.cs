using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows;
using System.IO;

namespace DesktopWidget.Services
{
    public class TrayService : IDisposable
    {
        private NotifyIcon _notifyIcon;
        private readonly MainWindow _mainWindow;

        public event EventHandler ShowWindowRequested;
        public event EventHandler ExitRequested;

        public TrayService(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            InitializeTrayIcon();
        }

        private void InitializeTrayIcon()
        {
            // Try to load custom icon, fallback to system icon if not available
            Icon customIcon = null;
            try
            {
                // Try multiple possible paths for the icon file
                string[] possiblePaths = {
                    // First try the specialized tray icon
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tray_icon.ico"),
                    // Absolute path to project root
                    Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "tray_icon.ico")),
                    Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "quadrant_icon.ico")),
                    // Relative paths from executable location
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "quadrant_icon.ico"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "quadrant_icon.ico"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "quadrant_icon.ico"),
                    // Embedded resource approach
                    "pack://application:,,,/tray_icon.ico",
                    "pack://application:,,,/quadrant_icon.ico"
                };

                Console.WriteLine($"Current base directory: {AppDomain.CurrentDomain.BaseDirectory}");

                foreach (string iconPath in possiblePaths)
                {
                    if (iconPath.StartsWith("pack://"))
                    {
                        // Try to load from embedded resource
                        try
                        {
                            var stream = System.Windows.Application.GetResourceStream(new Uri(iconPath))?.Stream;
                            if (stream != null)
                            {
                                customIcon = new Icon(stream);
                                Console.WriteLine($"Successfully loaded embedded icon");
                                break;
                            }
                        }
                        catch
                        {
                            Console.WriteLine($"Failed to load embedded resource: {iconPath}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Checking icon path: {iconPath}, Exists: {File.Exists(iconPath)}");
                        if (File.Exists(iconPath))
                        {
                            // Create icon with proper size for system tray (16x16 or 32x32)
                            using (var originalIcon = new Icon(iconPath))
                            {
                                // Create a new icon with the desired size
                                customIcon = new Icon(iconPath, new System.Drawing.Size(16, 16));
                                Console.WriteLine($"Successfully loaded icon from: {iconPath}, Size: {originalIcon.Width}x{originalIcon.Height}");
                                break;
                            }
                        }
                    }
                }

                if (customIcon != null)
                {
                    Console.WriteLine("Custom icon loaded successfully!");
                }
                else
                {
                    Console.WriteLine("Failed to load custom icon, using system icon");
                }
            }
            catch (Exception ex)
            {
                // Log the error if needed, or silently fallback to system icon
                System.Diagnostics.Debug.WriteLine($"Failed to load custom icon: {ex.Message}");
                Console.WriteLine($"Failed to load custom icon: {ex.Message}");
            }

            _notifyIcon = new NotifyIcon
            {
                Icon = customIcon ?? SystemIcons.Application,
                Text = "Desktop Widget - 四象限任务管理",
                Visible = true
            };

            // 创建上下文菜单
            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("显示窗口", null, OnShowWindow);
            contextMenu.Items.Add("-");
            contextMenu.Items.Add("退出", null, OnExit);

            _notifyIcon.ContextMenuStrip = contextMenu;
            _notifyIcon.DoubleClick += OnNotifyIconDoubleClick;
        }

        private void OnShowWindow(object sender, EventArgs e)
        {
            ShowWindowRequested?.Invoke(this, EventArgs.Empty);
        }

        private void OnExit(object sender, EventArgs e)
        {
            ExitRequested?.Invoke(this, EventArgs.Empty);
        }

        private void OnNotifyIconDoubleClick(object sender, EventArgs e)
        {
            ShowWindowRequested?.Invoke(this, EventArgs.Empty);
        }

        public void ShowBalloonTip(string title, string text, int timeout = 3000)
        {
            _notifyIcon?.ShowBalloonTip(timeout, title, text, ToolTipIcon.Info);
        }

        public void UpdateIcon(Icon icon)
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.Icon = icon;
            }
        }

        public void Dispose()
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
                _notifyIcon = null;
            }
        }
    }
}