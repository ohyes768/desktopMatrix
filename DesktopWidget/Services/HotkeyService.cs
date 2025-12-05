using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace DesktopWidget.Services
{
    public class HotkeyService : IDisposable
    {
        private readonly Dictionary<int, Action> _hotkeys = new Dictionary<int, Action>();
        private readonly IntPtr _windowHandle;
        private int _currentId = 0;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const uint MOD_ALT = 0x1;
        private const uint MOD_CONTROL = 0x2;
        private const uint MOD_SHIFT = 0x4;
        private const uint MOD_WIN = 0x8;
        private const uint WM_HOTKEY = 0x312;

        public HotkeyService(IntPtr windowHandle)
        {
            _windowHandle = windowHandle;
        }

        public bool RegisterHotkey(Keys key, bool ctrl = false, bool alt = false, bool shift = false, bool win = false, Action callback = null)
        {
            if (callback == null) return false;

            uint modifiers = 0;
            if (ctrl) modifiers |= MOD_CONTROL;
            if (alt) modifiers |= MOD_ALT;
            if (shift) modifiers |= MOD_SHIFT;
            if (win) modifiers |= MOD_WIN;

            int id = ++_currentId;
            bool success = RegisterHotKey(_windowHandle, id, modifiers, (uint)key);

            if (success)
            {
                _hotkeys[id] = callback;
            }

            return success;
        }

        public bool UnregisterHotkey(int id)
        {
            if (_hotkeys.ContainsKey(id))
            {
                bool success = UnregisterHotKey(_windowHandle, id);
                if (success)
                {
                    _hotkeys.Remove(id);
                }
                return success;
            }
            return false;
        }

        public void ProcessHotkeyMessage(IntPtr wParam)
        {
            int id = wParam.ToInt32();
            if (_hotkeys.ContainsKey(id))
            {
                _hotkeys[id]?.Invoke();
            }
        }

        public void Dispose()
        {
            foreach (var id in new List<int>(_hotkeys.Keys))
            {
                UnregisterHotkey(id);
            }
            _hotkeys.Clear();
        }
    }
}