using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace raka_no_f
{
    public class HotKeyManager
    {
        public KeyEventArgs[] spellKeys { get; set; }
        public KeyEventArgs[] positionKeys { get; set; }
        public bool hook_enabled { get; private set; }
        public KeyEventArgs clearKey { get; set; }

        private List<Keys> _hotkeys = new List<Keys>();
        private GlobalKeyboardHook _hook = new GlobalKeyboardHook();

        public HotKeyManager(KeyEventHandler keyDownHandler)
        {
            spellKeys = new KeyEventArgs[(int)Spell.noe];
            positionKeys = new KeyEventArgs[(int)Position.noe];

            _hook.KeyDown += new KeyEventHandler(keyDownHandler);
        }

        public void add(Keys key)
        {
            _hotkeys.Add(key);
            _hook.HookedKeys.Add(key);
            Console.WriteLine("Hotkey added: " + key);
        }

        public void remove(Keys key)
        {
            _hotkeys.RemoveAll(item => item == key);
            _hook.HookedKeys.RemoveAll(item => item == key);
            Console.WriteLine("Hotkey removed: " + key);
        }

        public void enableHotkeys()
        {
            _hook.HookedKeys.AddRange(_hotkeys);
            _hook.hook();
            hook_enabled = true;
        }

        public void disableHotkeys()
        {
            _hook.HookedKeys.Clear();
            _hook.unhook();
            hook_enabled = false;
        }
    }
}
