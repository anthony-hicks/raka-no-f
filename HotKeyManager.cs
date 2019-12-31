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

        private int lastId;
        private List<int> registered;
        private IntPtr handle;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int modifiers, int key);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public HotKeyManager(IntPtr handle_)
        {
            lastId = 1;
            handle = handle_;
            registered = new List<int>();
        }

        public int RegisterGlobal(Keys key, KeyModifiers modifiers, string msg)
        {
            // Arbitrary id uses count of the registered hotkeys.
            int id = lastId;

            if (!RegisterHotKey(handle, id, (int)modifiers, key.GetHashCode()))
            {
                MessageBox.Show("Error registering hotkey " + key.ToString());
                System.Windows.Forms.Application.Exit();
            }
            else
            {
                registered.Add(id);
                Console.WriteLine("Registered hotkey #" + id.ToString() + ": " + msg);
                lastId++;
            }

            return id;
        }

        public void UnregisterGlobal(int hotkey_id)
        {
            if (!UnregisterHotKey(handle, hotkey_id))
            {
                MessageBox.Show("Error unregistering hotkey " + hotkey_id);
                System.Windows.Forms.Application.Exit();
            }
            else
            {
                registered.Remove(hotkey_id);
            }
        }

        public void UnregisterAll()
        {
            foreach (int id in registered)
            {
                UnregisterGlobal(id);
            }
        }

        ~HotKeyManager()
        {
            this.UnregisterAll();
        }
    }
}
