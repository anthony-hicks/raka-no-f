﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

//TODO: standardize m_ members and param_
//TODO: add customization of hotkeys via trayIcon context menu
//TODO: no window icon when not in tray
//TODO: have overlay + tray when league is running (in game)
//      only have tray when league isn't running

namespace raka_no_f
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int modifiers, int key);

        [DllImport("user32.dll")]
        private static extern bool UnRegisterHotKey(IntPtr hWnd, int id);

        private System.Windows.Forms.ContextMenu contextMenu;
        private System.Windows.Forms.MenuItem menuItemExit;

        private bool[] selected;
        private Enemy[] enemies;
        private List<Countdown> countdowns;

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
                enemies[(int)pos] = new Enemy(pos, false);
            }

            hotkeys = new Dictionary<string, int[]>
            {
                [nameof(Position)] = new int[(int)Position.noe],
                [nameof(Spell)] = new int[(int)Spell.noe]
            };

            hotkeyManager = new HotKeyManager(this.Handle);
            assignDefaultHotkeys();

            // TODO: get sums from RiotAPI to get more accurate CDs?
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == HotKeyManager.WM_HOTKEY)
            {
                int pressed;
                int hotkey_id = (int)m.WParam;

                // If a summoner spell hotkey was pressed
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
            base.WndProc(ref m);
        }

        private void assignDefaultHotkeys()
        {
            hotkeys[nameof(Position)][(int)Position.top] = hotkeyManager.RegisterGlobal(Keys.NumPad7, KeyModifiers.None, "NumPad7");
            hotkeys[nameof(Position)][(int)Position.jg] = hotkeyManager.RegisterGlobal(Keys.NumPad4, KeyModifiers.None, "NumPad4");
            hotkeys[nameof(Position)][(int)Position.mid] = hotkeyManager.RegisterGlobal(Keys.NumPad1, KeyModifiers.None, "NumPad1");
            hotkeys[nameof(Position)][(int)Position.adc] = hotkeyManager.RegisterGlobal(Keys.NumPad0, KeyModifiers.None, "NumPad0");
            hotkeys[nameof(Position)][(int)Position.sup] = hotkeyManager.RegisterGlobal(Keys.Decimal, KeyModifiers.None, "Decimal");

            hotkeys[nameof(Spell)][(int)Spell.flash] = hotkeyManager.RegisterGlobal(Keys.Add, KeyModifiers.None, "Add");
            hotkeys[nameof(Spell)][(int)Spell.ignite] = hotkeyManager.RegisterGlobal(Keys.NumPad9, KeyModifiers.None, "NumPad9");
            hotkeys[nameof(Spell)][(int)Spell.teleport] = hotkeyManager.RegisterGlobal(Keys.Enter, KeyModifiers.None, "Enter");
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

            contextMenu.MenuItems.AddRange(
                new System.Windows.Forms.MenuItem[] { menuItemExit }
            );

            menuItemExit.Index = 0;
            menuItemExit.Text = "Exit";

            notifyIcon.ContextMenu = contextMenu;
            notifyIcon.Text = "notifyIcon text here";
            notifyIcon.Visible = false;
        }

        private void assignEventHandlers()
        {
            // Main form
            this.Resize += new System.EventHandler(this.Form1_Resize);

            // Tray icon
            menuItemExit.Click += new System.EventHandler(menuItemExit_Click);
            notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(notifyIcon1_MouseDoubleClick);
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
