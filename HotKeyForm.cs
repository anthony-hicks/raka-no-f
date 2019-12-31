using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace raka_no_f
{
    public partial class HotKeyForm : Form
    {
        private Dictionary<string, int[]> ids;
        public Dictionary<string, exscape.HotkeyControl> hotkeyControls;

        public HotKeyForm(Dictionary<string, int[]> hotkeys)
        {
            InitializeComponent();

            ids = hotkeys;

            hotkeyControls = new Dictionary<string, exscape.HotkeyControl>();
            this.Resize += new System.EventHandler(this.onResize);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.onClose);

            int posx = 100;
            int sumx = 300;
            int posy = 175;
            int sumy = 175;

            for (Position pos = Position.top; pos < Position.noe; pos++)
            {

                this.hotkeyControls[pos.ToString()] = new exscape.HotkeyControl();
                
                this.hotkeyControls[pos.ToString()].Hotkey = Keys.None;
                this.hotkeyControls[pos.ToString()].HotkeyModifiers = Keys.None;
                this.hotkeyControls[pos.ToString()].Location = new System.Drawing.Point(posx, posy += 30);
                this.hotkeyControls[pos.ToString()].Name = pos.ToString();
                this.hotkeyControls[pos.ToString()].Size = new System.Drawing.Size(100, 20);
                //this.hotkeyControls[pos.ToString()].TabIndex = 4;
                this.hotkeyControls[pos.ToString()].Text = "";
                //this.hotkeyControls[pos.ToString()].TextChanged += new System.E
                this.Controls.Add(this.hotkeyControls[pos.ToString()]);
            }

            for (Spell spell = Spell.flash; spell < Spell.noe; spell++)
            {
                this.hotkeyControls[spell.ToString()] = new exscape.HotkeyControl();
            }

            this.Hide();
        }

        private void onResize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
            }
        }

        private void onClose(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }

        private void hotkeyControl1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
