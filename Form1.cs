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
        private int m_ad_key_id;
        private int m_top_key_id;
        private int m_flash_key_id;

        private bool m_top_selected;
        private bool m_jg_selected;
        private bool m_mid_selected;
        private bool m_ad_selected;
        private bool m_sup_selected;

        private Enemy[] enemies;
        private List<Countdown> countdowns;

        public Form1()
        {
            InitializeComponent();

            initializeTrayIcon();
            assignEventHandlers();

            HotKeyManager hotkeys = new HotKeyManager(this.Handle);

            m_ad_key_id = hotkeys.RegisterGlobal(Keys.Q, KeyModifiers.Alt | KeyModifiers.Shift, "Alt+Shift+Q");
            m_top_key_id = hotkeys.RegisterGlobal(Keys.S, KeyModifiers.Alt | KeyModifiers.Shift, "Alt+Shift+S");
            m_flash_key_id = hotkeys.RegisterGlobal(Keys.F, KeyModifiers.Ctrl | KeyModifiers.Alt, "Ctrl+Alt+F");

            enemies = new Enemy[(int)Position.noe];
            countdowns = new List<Countdown>();

            for (Position pos = Position.top; pos < Position.noe; ++pos)
            {
                enemies[(int)pos] = new Enemy(pos, false);
            }
            // TODO: get sums from RiotAPI to get more accurate CDs?
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == HotKeyManager.WM_HOTKEY)
            {
                /* TODO: 
                 * selected: {
                 *      Position.top: false,
                 *      Position.adc: true
                 * }
                 *      spell = spell_ids.find((int)m.Wparam))
                 *      if(found):
                 *          position = selected.find(=> x == true)
                 *          if (position):
                 *              position = false;
                 *              processCountdown(whichever_was_true, whichever_spell_was_pressed, "{} {}: ".format(which.ToString(), spell.ToString()))
                 * 
                 * 
                 * 
                 * 
                 * 
                 */
                if (m_flash_key_id == (int)m.WParam)
                {
                    Console.WriteLine("flash selected");
                    if (m_ad_selected)
                    {
                        m_ad_selected = false;
                        this.processCountdown(Position.adc, Spell.flash);
                    }
                    else if (m_top_selected)
                    {
                        m_top_selected = false;
                        this.processCountdown(Position.top, Spell.flash);
                    }
                }
                else if (m_ad_key_id == (int)m.WParam)
                {
                    m_ad_selected = true;
                    m_top_selected = false;
                    // TODO: timeout for bools?
                    /* foreach (c in countdowns):
                     *      if c.done:
                     *          this.Controls.Remove(c.label)
                     * we could also just do this.label.Dispose() in Countdown, when it is ready. Depends on what should know.
                     */
                    Console.WriteLine("adc selected");
                }
                else if (m_top_key_id == (int)m.WParam)
                {
                    m_top_selected = true;
                    m_ad_selected = false; //TODO: we don't want to allow "AD -> TOP -> FLASH" to register a flash for AD
                                           // If any position hotkey has been pressed, set all bools to false
                    Console.WriteLine("top selected");
                }
                else
                {
                    Console.WriteLine("unknown hotkey pressed");
                }
            }
            base.WndProc(ref m);
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
