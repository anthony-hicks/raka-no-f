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

namespace raka_no_f_winforms
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int modifiers, int key);

        [DllImport("user32.dll")]
        private static extern bool UnRegisterHotKey(IntPtr hWnd, int id);

        private System.Windows.Forms.ContextMenu contextMenu1;
        private System.Windows.Forms.MenuItem menuItem1;
        private int m_ad_key_id;
        private int m_top_key_id;

        public Form1()
        {
            InitializeComponent();
            this.contextMenu1 = new System.Windows.Forms.ContextMenu();
            this.menuItem1 = new System.Windows.Forms.MenuItem();

            this.contextMenu1.MenuItems.AddRange(
                new System.Windows.Forms.MenuItem[] { this.menuItem1 }
            );

            this.menuItem1.Index = 0;
            this.menuItem1.Text = "exit stage left";
            this.menuItem1.Click += new System.EventHandler(this.menuItem1_Click);

            // TODO: figure out best way to move out handler signup
            // setup_notifyicon() ?
            this.Resize += new System.EventHandler(this.Form1_Resize);
            this.notifyIcon1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseDoubleClick);
            this.notifyIcon1.ContextMenu = this.contextMenu1;
            notifyIcon1.Text = "notifyIcon1 text here";
            notifyIcon1.Visible = false;

            HotKeyManager hotkeys = new HotKeyManager(this.Handle);

            m_ad_key_id = hotkeys.RegisterGlobal(Keys.Q, KeyModifiers.Alt | KeyModifiers.Shift, "Alt+Shift+Q");
            m_top_key_id = hotkeys.RegisterGlobal(Keys.S, KeyModifiers.Alt | KeyModifiers.Shift, "Alt+Shift+S");
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == HotKeyManager.WM_HOTKEY)
            {
                // TODO: Input 
                //       1. Flash --> AD
                //       OR 
                //       2. AD --> Flash
                if (m_ad_key_id == (int)m.WParam)
                {
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

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                Hide();
                notifyIcon1.Visible = true;
            }
            Console.WriteLine("resize event fired");
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
        }

        private void menuItem1_Click(object sender, EventArgs e)
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
    }
}
