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
//TODO: add a clearall hotkey
//TODO: Synchronize timer ticks?

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

        private bool hook_enabled;
        private Dictionary<string, KeyEventArgs[]> hotkeys;

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

            hotkeys = new Dictionary<string, KeyEventArgs[]>
            {
                [nameof(Position)] = new KeyEventArgs[(int)Position.noe],
                [nameof(Spell)] = new KeyEventArgs[(int)Spell.noe]
            };

            hotkeyForm = new HotKeyForm(hotkeys);
            populateDefaultHotkeyOptions();

            Timer ingameChecker = new Timer();
            ingameChecker.Interval = 5000; // Every 5s
            ingameChecker.Tick += new System.EventHandler(this.checkIfInGame_Tick);
            ingameChecker.Start();

            Timer countdownMonitor = new Timer();
            countdownMonitor.Interval = 5000; // Every 5s
            countdownMonitor.Tick += new System.EventHandler(this.monitorCountdowns_Tick);
            countdownMonitor.Start();

            hook.KeyDown += new KeyEventHandler(hook_KeyDown);
        }

        private bool keyEventArgsEquals(KeyEventArgs l, KeyEventArgs r)
        {
            return ((l.KeyCode == r.KeyCode) && (l.Modifiers == r.Modifiers));
        }

        private void hook_KeyDown(object sender, KeyEventArgs e)
        {
            int pressed;

            // If a summoner spell hotkey was pressed
            if (hotkeys[nameof(Spell)].Any(item => keyEventArgsEquals(item, e)))
            {
                pressed = Array.FindIndex(hotkeys[nameof(Spell)], item => keyEventArgsEquals(item, e));
                Console.WriteLine((Spell)pressed + " pressed.");

                // If a position is selected
                if (selected.Contains(true))
                {
                    int position = Array.FindIndex(selected, item => item == true);

                    selected[position] = false;
                    processCountdown((Position)position, (Spell)pressed);
                }
            }
            // If a position hotkey was pressed, clear any selected positions, and select the associated position.
            else if (hotkeys[nameof(Position)].Any(item => keyEventArgsEquals(item, e)))
            {
                pressed = Array.FindIndex(hotkeys[nameof(Position)], item => keyEventArgsEquals(item, e));
                Console.WriteLine((Position)pressed + " pressed.");

                Array.Clear(selected, 0, selected.Length);
                selected[pressed] = true;
            }
            else
            {
                Console.WriteLine("Hotkey ID not recognized.");
            }
            e.Handled = true;
        }

        private void menuItemHotkeys_Click(object sender, EventArgs e)
        {
            hotkeyForm.Show();
            hotkeyForm.WindowState = FormWindowState.Normal;
            hotkeyForm.Activate();
        }

        private void menuItemExit_Click(object sender, EventArgs e)
        {
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

            // Tray icon
            menuItemHotkeys.Click += new System.EventHandler(menuItemHotkeys_Click);
            menuItemExit.Click += new System.EventHandler(menuItemExit_Click);
        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {
        }

        public static System.Windows.Forms.Label createDefaultLabel(string name)
        {
            System.Windows.Forms.Label label = new System.Windows.Forms.Label();
            label.AutoSize = true;
            label.BackColor = System.Drawing.Color.Transparent;
            label.Font = new System.Drawing.Font("Franklin Gothic Medium", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            label.ForeColor = System.Drawing.Color.White;
            label.Name = name;

            return label;
        }

        private bool isLeagueRunning()
        {
            return (Process.GetProcessesByName("League of Legends").Length != 0);
        }

        private void post()
        {
            this.Hide();

            hook.HookedKeys.Clear();
            hook.unhook();
            hook_enabled = false;

            for (int i = countdowns.Count - 1; countdowns.Count > 0 && i >= 0; i--)
            {
                // Remove the label from the flow layout panel
                this.flowLayoutPanel1.Controls.Remove(countdowns[i].label);

                // Release resources of label
                countdowns[i].label.Dispose();

                // Remove the countdown
                countdowns.RemoveAt(i);
            }
        }

        private void monitorCountdowns_Tick(object sender, EventArgs e)
        {
            if (countdowns.Count > 0)
            {
                for (int i = countdowns.Count - 1; i >= 0; i--)
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
            }
        }

        private void checkIfInGame_Tick(object sender, EventArgs e)
        {
            bool game_running = isLeagueRunning();

            if (game_running && !hook_enabled)
            {
                this.enableDefaultHotkeys();
                hook.hook();
                hook_enabled = true;
            }
            else if (!game_running && hook_enabled)
            {
                post();
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

        private void enableDefaultHotkeys()
        {
            hook.HookedKeys.Add(Keys.NumPad7);
            hook.HookedKeys.Add(Keys.NumPad4);
            hook.HookedKeys.Add(Keys.NumPad1);
            hook.HookedKeys.Add(Keys.NumPad0);
            hook.HookedKeys.Add(Keys.Decimal);
            hook.HookedKeys.Add(Keys.Add);
            hook.HookedKeys.Add(Keys.NumPad9);
            hook.HookedKeys.Add(Keys.U);
            hook.HookedKeys.Add(Keys.Enter);
        }

        private void populateDefaultHotkeyOptions()
        {
            hotkeys[nameof(Position)][(int)Position.top] = new KeyEventArgs(Keys.NumPad7);
            hotkeyForm.hotkeyControls[Position.top.ToString()].Hotkey = Keys.NumPad7;
            hotkeyForm.hotkeyControls[Position.top.ToString()].HotkeyModifiers = Keys.None;

            hotkeys[nameof(Position)][(int)Position.jg] = new KeyEventArgs(Keys.NumPad4);
            hotkeyForm.hotkeyControls[Position.jg.ToString()].Hotkey = Keys.NumPad4;
            hotkeyForm.hotkeyControls[Position.jg.ToString()].HotkeyModifiers = Keys.None;

            hotkeys[nameof(Position)][(int)Position.mid] = new KeyEventArgs(Keys.NumPad1);
            hotkeyForm.hotkeyControls[Position.mid.ToString()].Hotkey = Keys.NumPad1;
            hotkeyForm.hotkeyControls[Position.mid.ToString()].HotkeyModifiers = Keys.None;

            hotkeys[nameof(Position)][(int)Position.adc] = new KeyEventArgs(Keys.NumPad0);
            hotkeyForm.hotkeyControls[Position.adc.ToString()].Hotkey = Keys.NumPad0;
            hotkeyForm.hotkeyControls[Position.adc.ToString()].HotkeyModifiers = Keys.None;

            hotkeys[nameof(Position)][(int)Position.sup] = new KeyEventArgs(Keys.Decimal);
            hotkeyForm.hotkeyControls[Position.sup.ToString()].Hotkey = Keys.Decimal;
            hotkeyForm.hotkeyControls[Position.sup.ToString()].HotkeyModifiers = Keys.None;

            hotkeys[nameof(Spell)][(int)Spell.flash] = new KeyEventArgs(Keys.Add);
            hotkeyForm.hotkeyControls[Spell.flash.ToString()].Hotkey = Keys.Add;
            hotkeyForm.hotkeyControls[Spell.flash.ToString()].HotkeyModifiers = Keys.None;

            hotkeys[nameof(Spell)][(int)Spell.ignite] = new KeyEventArgs(Keys.NumPad9);
            hotkeyForm.hotkeyControls[Spell.ignite.ToString()].Hotkey = Keys.NumPad9;
            hotkeyForm.hotkeyControls[Spell.ignite.ToString()].HotkeyModifiers = Keys.None;

            hotkeys[nameof(Spell)][(int)Spell.exhaust] = new KeyEventArgs(Keys.U | Keys.Control);
            hotkeyForm.hotkeyControls[Spell.exhaust.ToString()].Hotkey = Keys.U;
            hotkeyForm.hotkeyControls[Spell.exhaust.ToString()].HotkeyModifiers = Keys.Control;

            hotkeys[nameof(Spell)][(int)Spell.teleport] = new KeyEventArgs(Keys.Enter);
            hotkeyForm.hotkeyControls[Spell.teleport.ToString()].Hotkey = Keys.Enter;
            hotkeyForm.hotkeyControls[Spell.teleport.ToString()].HotkeyModifiers = Keys.None;
        }
    }
}
