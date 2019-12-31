using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;

//TODO: standardize m_ members and param_
//TODO: add customization of hotkeys via trayIcon context menu
//TODO: no window icon when not in tray
//TODO: have overlay + tray when league is running (in game)
//      only have tray when league isn't running
// TODO: separate the in-game form, hotkey settings form, and tray icon into separate files
//       and use main/program to instantiate.
//TODO: We want the hotkey form to process the hotkeys when it's open, can we do that
//      without unregistering hotkeys?

namespace raka_no_f
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int modifiers, int key);

        [DllImport("user32.dll")]
        private static extern bool UnRegisterHotKey(IntPtr hWnd, int id);

        private GlobalKeyboardHook hook = new GlobalKeyboardHook();

        private HotKeyForm hotkeyForm;
        private System.Windows.Forms.ContextMenu contextMenu;
        private System.Windows.Forms.MenuItem menuItemExit;
        private System.Windows.Forms.MenuItem menuItemHotkeys;

        private bool[] selected;
        private Enemy[] enemies;
        private List<Countdown> countdowns;

        private bool hotkeys_registered;
        private HotKeyManager hotkeyManager;
        private Dictionary<string, int[]> hotkeys;

        public Form1()
        {
            InitializeComponent();

            initializeTrayIcon();
            assignEventHandlers();

            selected = new bool[(int)Position.noe];
            enemies = new Enemy[(int)Position.noe];
            countdowns = new List<Countdown>();

            for (Position pos = Position.top; pos < Position.noe; ++pos)
            {
                enemies[(int)pos] = new Enemy(pos, false); // We assume no mods to summ spell CDs for now.
                // TODO: get sums from RiotAPI to get more accurate CDs?
            }

            hotkeys = new Dictionary<string, int[]>
            {
                [nameof(Position)] = new int[(int)Position.noe],
                [nameof(Spell)] = new int[(int)Spell.noe]
            };

            hotkeyManager = new HotKeyManager(this.Handle);
            hotkeyForm = new HotKeyForm(hotkeys);
            //assignDefaultHotkeys();

            Timer ingameChecker = new Timer();
            ingameChecker.Interval = 20000; // 20s
            ingameChecker.Tick += new System.EventHandler(this.checkIfInGame_Tick);
            ingameChecker.Start();

            hook.HookedKeys.Add(Keys.A);
            hook.HookedKeys.Add(Keys.B);

            hook.KeyDown += new KeyEventHandler(hook_KeyDown);
            hook.KeyUp += new KeyEventHandler(hook_KeyUp);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == HotKeyManager.WM_HOTKEY)
            {
                int pressed;
                int hotkey_id = (int)m.WParam;

                // If a summoner spell hotkey was pressed
                // TODO: use contains instead?
                if (hotkeys[nameof(Spell)].Any(item => item == hotkey_id))
                {
                    pressed = Array.FindIndex(hotkeys[nameof(Spell)], item => item == hotkey_id);
                    Console.WriteLine((Spell)pressed + " pressed.");

                    // If a position is selected, create/update a countdown
                    if (selected.Any(item => item == true))
                    {
                        int position = Array.FindIndex(selected, item => item == true);

                        selected[position] = false;
                        processCountdown((Position)position, (Spell)pressed);
                    }
                }
                // If a position hotkey was pressed, clear any selected positions, and select the associated position.
                else if (hotkeys[nameof(Position)].Any(item => item == hotkey_id))
                {
                    pressed = Array.FindIndex(hotkeys[nameof(Position)], item => item == hotkey_id);
                    Console.WriteLine((Position)pressed + " pressed.");

                    Array.Clear(selected, 0, selected.Length);
                    selected[pressed] = true;
                }
                else
                {
                    Console.WriteLine("Hotkey ID not recognized.");
                }
            }

            //TODO: put this on its own timer?
            for (int i = countdowns.Count - 1; countdowns.Count > 0 && i >= 0; i--)
            {
                if (countdowns[i].done)
                {
                    // Remove the label from the flow layout panel
                    this.flowLayoutPanel1.Controls.Remove(countdowns[i].label);

                    // Release resources of label
                    countdowns[i].label.Dispose();

                    // Remove the countdown
                    countdowns.RemoveAt(i);
                }
            }
            base.WndProc(ref m);
        }

        private void assignDefaultHotkeys()
        {
            // Add a hook for the keycode of the hotkey
            //hotkeys[nameof(Position)][(int)Position.top] = k;
            // 
            //hotkeys[nameof(Position)][(int)Position.top] = hotkeyManager.RegisterGlobal(Keys.NumPad7, KeyModifiers.None, "NumPad7");
            hotkeyForm.hotkeyControls[Position.top.ToString()].Hotkey = Keys.NumPad7;
            hotkeyForm.hotkeyControls[Position.top.ToString()].HotkeyModifiers = Keys.None;

            hotkeys[nameof(Position)][(int)Position.jg] = hotkeyManager.RegisterGlobal(Keys.NumPad4, KeyModifiers.None, "NumPad4");
            hotkeyForm.hotkeyControls[Position.jg.ToString()].Hotkey = Keys.NumPad4;
            hotkeyForm.hotkeyControls[Position.jg.ToString()].HotkeyModifiers = Keys.None;

            hotkeys[nameof(Position)][(int)Position.mid] = hotkeyManager.RegisterGlobal(Keys.NumPad1, KeyModifiers.None, "NumPad1");
            hotkeyForm.hotkeyControls[Position.mid.ToString()].Hotkey = Keys.NumPad1;
            hotkeyForm.hotkeyControls[Position.mid.ToString()].HotkeyModifiers = Keys.None;

            hotkeys[nameof(Position)][(int)Position.adc] = hotkeyManager.RegisterGlobal(Keys.NumPad0, KeyModifiers.None, "NumPad0");
            hotkeyForm.hotkeyControls[Position.adc.ToString()].Hotkey = Keys.NumPad0;
            hotkeyForm.hotkeyControls[Position.adc.ToString()].HotkeyModifiers = Keys.None;

            hotkeys[nameof(Position)][(int)Position.sup] = hotkeyManager.RegisterGlobal(Keys.Decimal, KeyModifiers.None, "Decimal");
            hotkeyForm.hotkeyControls[Position.sup.ToString()].Hotkey = Keys.Decimal;
            hotkeyForm.hotkeyControls[Position.sup.ToString()].HotkeyModifiers = Keys.None;

            hotkeys[nameof(Spell)][(int)Spell.flash] = hotkeyManager.RegisterGlobal(Keys.Add, KeyModifiers.None, "Add");
            hotkeyForm.hotkeyControls[Spell.flash.ToString()].Hotkey = Keys.Add;
            hotkeyForm.hotkeyControls[Spell.flash.ToString()].HotkeyModifiers = Keys.None;

            hotkeys[nameof(Spell)][(int)Spell.ignite] = hotkeyManager.RegisterGlobal(Keys.NumPad9, KeyModifiers.None, "NumPad9");
            hotkeyForm.hotkeyControls[Spell.ignite.ToString()].Hotkey = Keys.NumPad9;
            hotkeyForm.hotkeyControls[Spell.ignite.ToString()].HotkeyModifiers = Keys.None;

            hotkeys[nameof(Spell)][(int)Spell.teleport] = hotkeyManager.RegisterGlobal(Keys.Enter, KeyModifiers.None, "Enter");
            hotkeyForm.hotkeyControls[Spell.teleport.ToString()].Hotkey = Keys.Enter;
            hotkeyForm.hotkeyControls[Spell.teleport.ToString()].HotkeyModifiers = Keys.None;
        }

        private void hook_KeyDown(object sender, KeyEventArgs e)
        {
            Console.WriteLine("hook KeyDown " + e.KeyCode.ToString());
            if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
            {
                Console.WriteLine("(keydown) Control is active.");
            }
            e.Handled = true;
        }

        private void hook_KeyUp(object sender, KeyEventArgs e)
        {
            // TODO: put all wndproc key stuff in here
            Console.WriteLine("hook KeyUp " + e.KeyCode.ToString());
            // modifier = KeyData | KeyCode
            if (e.KeyCode == Keys.A && (Control.ModifierKeys & Keys.Control) == Keys.Control)
            {
                Console.WriteLine("CTRL+A");
                //Console.WriteLine("(keyup) Control is active.");
            }

            KeyEventArgs k = new KeyEventArgs(Keys.NumPad7 | Keys.Control);
            Console.WriteLine("k.Control = " + k.Control);
            Console.WriteLine("k.key = " + k.KeyCode);
            Console.WriteLine("k.KeyData = " + k.KeyData);
            Console.WriteLine("k.Modifiers = " + k.Modifiers);
            e.Handled = true;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                Hide();
                notifyIcon.Visible = true;
            }
            Console.WriteLine("resize event fired");
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon.Visible = false;
        }

        private void menuItemHotkeys_Click(object sender, EventArgs e)
        {
            hotkeyForm.Show();
            hotkeyForm.WindowState = FormWindowState.Normal;
            hotkeyForm.Activate();
        }

        private void menuItemExit_Click(object sender, EventArgs e)
        {
            // Close the form, which closes the application.
            this.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void initializeTrayIcon()
        {
            contextMenu = new System.Windows.Forms.ContextMenu();
            menuItemExit = new System.Windows.Forms.MenuItem();
            menuItemHotkeys = new System.Windows.Forms.MenuItem();

            contextMenu.MenuItems.AddRange(
                new System.Windows.Forms.MenuItem[] { 
                    menuItemExit,
                    menuItemHotkeys
                }
            );

            menuItemHotkeys.Index = 0;
            menuItemHotkeys.Text = "Hotkeys";

            menuItemExit.Index = 1;
            menuItemExit.Text = "Exit";

            notifyIcon.ContextMenu = contextMenu;
            notifyIcon.Text = "raka-no-f";
            notifyIcon.Visible = true;
        }

        private void assignEventHandlers()
        {
            // Main form
            this.Resize += new System.EventHandler(this.Form1_Resize);

            // Tray icon
            menuItemHotkeys.Click += new System.EventHandler(menuItemHotkeys_Click);
            menuItemExit.Click += new System.EventHandler(menuItemExit_Click);
        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private System.Windows.Forms.Label createDefaultLabel(string name)
        {
            System.Windows.Forms.Label label = new System.Windows.Forms.Label();
            label.AutoSize = true;
            label.BackColor = System.Drawing.Color.Transparent;
            label.Font = new System.Drawing.Font("Franklin Gothic Medium", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            label.ForeColor = System.Drawing.Color.White;
            label.Name = name;

            return label;
        }

        private void checkIfInGame_Tick(object sender, EventArgs e)
        {
            Process[] processes = Process.GetProcessesByName("League of Legends");
            bool game_running = (processes.Length != 0);
            Console.WriteLine("game is running? " + game_running);

            if (game_running && !hotkeys_registered)
            {
                this.assignDefaultHotkeys();
                //hook.hook();
                hotkeys_registered = true;
            }
            else if (!game_running && hotkeys_registered)
            {
                hotkeyManager.UnregisterAll();
                //hook.unhook(); TODO
                hotkeys_registered = false;
            }
        }

        private void processCountdown(Position position_, Spell spell_)
        {
            string labelName = position_.ToString() + " " + spell_.ToString();
            string labelText = labelName + ": ";
            Console.WriteLine("Creating/Updating " + labelName);

            bool exists = false;
            foreach (Countdown c in countdowns)
            {
                if (c.position == position_ && c.spell == spell_)
                {
                    Console.WriteLine("Existing countdown found. Resetting.");
                    c.reset();
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                Console.WriteLine("Creating new countdown for " + labelName);
                System.Windows.Forms.Label label = createDefaultLabel(labelName);
                this.flowLayoutPanel1.Controls.Add(label);
                countdowns.Add(new Countdown(
                    position_,
                    spell_,
                    label,
                    enemies[(int)position_].cd[spell_],
                    labelText
                ));
            }
        }
    }
}
