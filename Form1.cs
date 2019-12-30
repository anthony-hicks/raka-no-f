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

            enemies = new Enemy[(int)Position.noe];

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
                if (m_flash_key_id == (int)m.WParam)
                {
                    if (m_ad_selected)
                    {
                        m_ad_selected = false;
                        // TODO: Create a new label here if there's not one already present for the given pos + summ combo
                        //       could hash on the combo
                        //       or check all existing countdowns positions and flashes (not bad since this only occurs when creating a new countdown)
                        Countdown countdown = new Countdown(
                            Position.adc, 
                            Spell.flash, 
                            this.label1, 
                            enemies[(int)Position.adc].cd["flash"]
                        );
                    }
                }
                // TODO: If we handle the 2nd hotkey (summ), we shouldn't process anything else.
                if (m_ad_key_id == (int)m.WParam)
                {
                    m_ad_selected = true;
                    // TODO: timeout for bools?
                    Console.WriteLine("adc selected");
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
