using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace raka_no_f
{
    [Flags]
    public enum KeyModifiers
    {
        None = 0x0000,
        Alt = 0x0001,
        Ctrl = 0x0002,
        Shift = 0x0004,
        Win = 0x0008
    }

    class HotKeyManager
    {
        public const int WM_HOTKEY = 0x0312;

        private int m_lastId;
        private List<int> m_registered;
        private IntPtr m_handle;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int modifiers, int key);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public HotKeyManager(IntPtr handle)
        {
            m_lastId = 1;
            m_handle = handle;
            m_registered = new List<int>();
        }

        public int RegisterGlobal(Keys key, KeyModifiers modifiers, string msg)
        {
            // Arbitrary id uses count of the registered hotkeys.
            int id = m_lastId;

            if (!RegisterHotKey(m_handle, id, (int)modifiers, key.GetHashCode()))
            {
                MessageBox.Show("Error registering hotkey " + key.ToString());
                System.Windows.Forms.Application.Exit();
            }
            else
            {
                m_registered.Add(id);
                Console.WriteLine("Registered hotkey #" + id.ToString() + ": " + msg);
                m_lastId++;
            }

            return id;
        }

        ~HotKeyManager()
        {
            foreach (int id in m_registered)
            {
                UnregisterHotKey(m_handle, id);
            }
        }
    }
}
