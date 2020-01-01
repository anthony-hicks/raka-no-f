using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

//TODO: standardize m_ members and param_
// TODO: separate the in-game form, hotkey settings form, and tray icon into separate files
//       and use main/program to instantiate.

namespace raka_no_f
{
    public partial class Form1 : Form
    {
        private HotKeyForm hotkeyForm;
        private System.Windows.Forms.ContextMenu contextMenu;
        private System.Windows.Forms.MenuItem menuItemExit;
        private System.Windows.Forms.MenuItem menuItemHotkeys;

        private HotKeyManager hkManager;
        private bool[] selected;
        private Enemy[] enemies;
        private List<Countdown> countdowns;

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

            hkManager = new HotKeyManager(this.hook_KeyDown);
            hotkeyForm = new HotKeyForm(hkManager);

            populateDefaultHotkeyOptions();
            hkManager.disableHotkeys();

            // Checks if League of Legends is running
            Timer ingameChecker = new Timer();
            ingameChecker.Interval = 2000;
            ingameChecker.Tick += new System.EventHandler(this.checkIfInGame_Tick);
            ingameChecker.Start();

            // Checks if any countdown is complete, and removes if so.
            Timer countdownMonitor = new Timer();
            countdownMonitor.Interval = 5000;
            countdownMonitor.Tick += new System.EventHandler(this.monitorCountdowns_Tick);
            countdownMonitor.Start();
        }

        private bool keyEventArgsEquals(KeyEventArgs l, KeyEventArgs r)
        {
            return ((l.KeyCode == r.KeyCode) && (l.Modifiers == r.Modifiers));
        }

        private void hook_KeyDown(object sender, KeyEventArgs e)
        {
            // We don't want to process the hotkeys if we're changing hotkeys
            if (!hotkeyForm.Visible)
            {
                int pressed;

                // If a summoner spell hotkey was pressed
                if (hkManager.spellKeys.Any(item => keyEventArgsEquals(item, e)))
                {
                    pressed = Array.FindIndex(hkManager.spellKeys, item => keyEventArgsEquals(item, e));
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
                else if (hkManager.positionKeys.Any(item => keyEventArgsEquals(item, e)))
                {
                    pressed = Array.FindIndex(hkManager.positionKeys, item => keyEventArgsEquals(item, e));
                    Console.WriteLine((Position)pressed + " pressed.");

                    Array.Clear(selected, 0, selected.Length);
                    selected[pressed] = true;
                }
                else if (keyEventArgsEquals(hkManager.clearKey, e))
                {
                    removeAllCountdowns();
                }
                else
                {
                    Console.WriteLine("Hotkey ID not recognized: " + e.KeyData);
                }
                e.Handled = true;
            }
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
            hkManager.disableHotkeys();
            removeAllCountdowns();
        }

        private void removeAllCountdowns()
        {
            for (int i = countdowns.Count - 1; countdowns.Count > 0 && i >= 0; i--)
            {
                removeCountdownAt(i);
            }
        }
        private void removeCountdownAt(int i)
        {
            // Remove the label from the flow layout panel
            this.flowLayoutPanel1.Controls.Remove(countdowns[i].label);

            // Release resources of label
            countdowns[i].label.Dispose();

            // Remove the countdown
            countdowns.RemoveAt(i);
        }

        private void monitorCountdowns_Tick(object sender, EventArgs e)
        {
            if (countdowns.Count > 0)
            {
                for (int i = countdowns.Count - 1; i >= 0; i--)
                {
                    if (countdowns[i].done)
                    {
                        removeCountdownAt(i);
                    }
                }
            }
        }

        private void checkIfInGame_Tick(object sender, EventArgs e)
        {
            bool game_running = isLeagueRunning();

            if (game_running && !hkManager.hook_enabled)
            {
                hkManager.enableHotkeys();
            }
            else if (!game_running && hkManager.hook_enabled)
            {
                post();
            }
        }

        private void processCountdown(Position position_, Spell spell_)
        {
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
                string labelName = position_.ToString() + " " + spell_.ToString();
                string labelText = labelName + ": ";

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

        private void populateDefaultHotkeyOptions()
        {
            hkManager.add(Keys.End);
            hkManager.clearKey = new KeyEventArgs(Keys.End);
            hotkeyForm.hkDisplayControls["clear"].hkControl.Hotkey = Keys.End;
            hotkeyForm.hkDisplayControls["clear"].hkControl.HotkeyModifiers = Keys.None;

            hkManager.add(Keys.NumPad7);
            hkManager.positionKeys[(int)Position.top] = new KeyEventArgs(Keys.NumPad7);

            hotkeyForm.hkDisplayControls[Position.top.ToString()].hkControl.Hotkey = Keys.NumPad7;
            hotkeyForm.hkDisplayControls[Position.top.ToString()].hkControl.HotkeyModifiers = Keys.None;

            hkManager.add(Keys.NumPad4);
            hkManager.positionKeys[(int)Position.jg] = new KeyEventArgs(Keys.NumPad4);
            hotkeyForm.hkDisplayControls[Position.jg.ToString()].hkControl.Hotkey = Keys.NumPad4;
            hotkeyForm.hkDisplayControls[Position.jg.ToString()].hkControl.HotkeyModifiers = Keys.None;

            hkManager.add(Keys.NumPad1);
            hkManager.positionKeys[(int)Position.mid] = new KeyEventArgs(Keys.NumPad1);
            hotkeyForm.hkDisplayControls[Position.mid.ToString()].hkControl.Hotkey = Keys.NumPad1;
            hotkeyForm.hkDisplayControls[Position.mid.ToString()].hkControl.HotkeyModifiers = Keys.None;

            hkManager.add(Keys.NumPad0);
            hkManager.positionKeys[(int)Position.adc] = new KeyEventArgs(Keys.NumPad0);
            hotkeyForm.hkDisplayControls[Position.adc.ToString()].hkControl.Hotkey = Keys.NumPad0;
            hotkeyForm.hkDisplayControls[Position.adc.ToString()].hkControl.HotkeyModifiers = Keys.None;

            hkManager.add(Keys.Decimal);
            hkManager.positionKeys[(int)Position.sup] = new KeyEventArgs(Keys.Decimal);
            hotkeyForm.hkDisplayControls[Position.sup.ToString()].hkControl.Hotkey = Keys.Decimal;
            hotkeyForm.hkDisplayControls[Position.sup.ToString()].hkControl.HotkeyModifiers = Keys.None;

            hkManager.add(Keys.Add);
            hkManager.spellKeys[(int)Spell.flash] = new KeyEventArgs(Keys.Add);
            hotkeyForm.hkDisplayControls[Spell.flash.ToString()].hkControl.Hotkey = Keys.Add;
            hotkeyForm.hkDisplayControls[Spell.flash.ToString()].hkControl.HotkeyModifiers = Keys.None;

            hkManager.add(Keys.NumPad9);
            hkManager.spellKeys[(int)Spell.ignite] = new KeyEventArgs(Keys.NumPad9);
            hotkeyForm.hkDisplayControls[Spell.ignite.ToString()].hkControl.Hotkey = Keys.NumPad9;
            hotkeyForm.hkDisplayControls[Spell.ignite.ToString()].hkControl.HotkeyModifiers = Keys.None;

            hkManager.add(Keys.U);
            hkManager.spellKeys[(int)Spell.exhaust] = new KeyEventArgs(Keys.U | Keys.Control);
            hotkeyForm.hkDisplayControls[Spell.exhaust.ToString()].hkControl.Hotkey = Keys.U;
            hotkeyForm.hkDisplayControls[Spell.exhaust.ToString()].hkControl.HotkeyModifiers = Keys.Control;

            hkManager.add(Keys.Enter);
            hkManager.spellKeys[(int)Spell.teleport] = new KeyEventArgs(Keys.Enter);
            hotkeyForm.hkDisplayControls[Spell.teleport.ToString()].hkControl.Hotkey = Keys.Enter;
            hotkeyForm.hkDisplayControls[Spell.teleport.ToString()].hkControl.HotkeyModifiers = Keys.None;

            hotkeyForm.changed.Clear();
        }
    }
}
