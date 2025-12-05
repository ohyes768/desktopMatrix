using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows;

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
            _notifyIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application,
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