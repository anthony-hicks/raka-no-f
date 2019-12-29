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

        public Form1()
        {
            InitializeComponent();

            initializeTrayIcon();
            assignEventHandlers();

            HotKeyManager hotkeys = new HotKeyManager(this.Handle);

            m_ad_key_id = hotkeys.RegisterGlobal(Keys.Q, KeyModifiers.Alt | KeyModifiers.Shift, "Alt+Shift+Q");
            m_top_key_id = hotkeys.RegisterGlobal(Keys.S, KeyModifiers.Alt | KeyModifiers.Shift, "Alt+Shift+S");
            m_flash_key_id = hotkeys.RegisterGlobal(Keys.F, KeyModifiers.Ctrl | KeyModifiers.Alt, "Ctrl+Alt+F");

            // TODO: get sums from RiotAPI to get more accurate CDs?
            foreach (Position pos in Enum.GetValues(typeof(Position)))
            {
                enemies[(int)pos] = new Enemy(pos, false);
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == HotKeyManager.WM_HOTKEY)
            {
                if (m_flash_key_id == (int)m.WParam)
                {
                    if (m_ad_selected)
                    {
                        // Start a timer that updates the text in box w/ below
                        // put string in box --> "AD Flash: {}".format(timer.now())
                        // TODO: should create a class that knows how much time is remaining.
                        //       I would hate to have all these ad_flash_timer_remaining, top_ignite_time_remaining, ...
                        Timer timer = new Timer();
                        timer.Interval = 1000;
                        timer.Tick += new EventHandler(timer_Tick);
                        timer.Start();
                        m_ad_selected = false;

                        // TODO: the countdown needs to know about label1 in order to update the text
                        //       on its own.
                        Countdown countdown = new Countdown(Position.adc, Summoner.Flash, enemies[Position.adc].cd["flash"]);
                    }
                    // TODO: Check if any(m_ad_selected, m_top_selected, ...)
                    //       and update the box with whichever one is true.
                    //       TODO: Should put in a container for iteration.
                }
                // TODO: If we handle the 2nd hotkey (summ), we shouldn't process anything else.
                if (m_ad_key_id == (int)m.WParam)
                {
                    m_ad_selected = true;
                    // TODO: timeout for bools?
                    Console.WriteLine("ad key has been hit");
                }
                else if (m_top_key_id == (int)m.WParam)
                {
                    Console.WriteLine("top key has been hit");
                }
                else
                {
                    Console.WriteLine("unknown hotkey pressed");
                }
            }
            base.WndProc(ref m);
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            label1.Text = "AD Flash: " + remaining;
            remaining--;
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

        private void label1_Click(object sender, EventArgs e)
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
    }
}
