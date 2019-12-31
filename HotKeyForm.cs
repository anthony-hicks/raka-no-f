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
        private HashSet<exscape.HotkeyControl> changed = new HashSet<exscape.HotkeyControl>();

        private Dictionary<string, KeyEventArgs[]> hotkeys;
        public Dictionary<string, exscape.HotkeyControl> hotkeyControls;

        public HotKeyForm(Dictionary<string, KeyEventArgs[]> hotkeys_)
        {
            InitializeComponent();
            this.button1.Click += new EventHandler(this.okButton_Click);
            this.AcceptButton = this.button1;
            hotkeys = hotkeys_;

            hotkeyControls = new Dictionary<string, exscape.HotkeyControl>();
            this.Resize += new System.EventHandler(this.onResize);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.onClose);

            int posx = 50;
            int sumx = 260;
            int posy = 25;
            int sumy = 25;

            for (Position pos = Position.top; pos < Position.noe; pos++)
            {
                Label label = Form1.createDefaultLabel(pos.ToString());
                label.Text = pos.ToString();
                label.Location = new Point(posx - 40, posy + 30);
                label.ForeColor = System.Drawing.Color.Black;
                this.Controls.Add(label);

                exscape.HotkeyControl hkcontrol = createDefaultHotkeyControl(pos.ToString());
                hkcontrol.TextChanged += new EventHandler(hotkeyControl_TextChanged);
                hkcontrol.Location = new Point(posx, posy += 30);

                this.hotkeyControls[pos.ToString()] = hkcontrol;
                this.Controls.Add(hkcontrol);
            }

            for (Spell spell = Spell.flash; spell < Spell.noe; spell++)
            {
                Label label = Form1.createDefaultLabel(spell.ToString());
                label.Text = spell.ToString();
                label.Location = new Point(sumx - 55, sumy + 30);
                label.ForeColor = Color.Black;
                this.Controls.Add(label);

                exscape.HotkeyControl hkcontrol = createDefaultHotkeyControl(spell.ToString());
                hkcontrol.TextChanged += new EventHandler(hotkeyControl_TextChanged);
                hkcontrol.Location = new Point(sumx, sumy += 30);

                this.hotkeyControls[spell.ToString()] = hkcontrol;
                this.Controls.Add(hkcontrol);
            }

            this.Hide();
        }

        private exscape.HotkeyControl createDefaultHotkeyControl(string name_)
        {
            exscape.HotkeyControl hkcontrol = new exscape.HotkeyControl();

            hkcontrol.Hotkey = Keys.None;
            hkcontrol.HotkeyModifiers = Keys.None;
            hkcontrol.Name = name_;
            hkcontrol.Size = new Size(125, 20);
            hkcontrol.Text = "";

            return hkcontrol;
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

        private void okButton_Click(object sender, EventArgs e)
        {
            foreach (exscape.HotkeyControl control in this.changed)
            {
                Console.WriteLine("control was modified: " + control.Hotkey);
                // Kinda spaghetti
                // Need to move stuff around so I'm not exposing random pieces of each class/form
                //Form1.hook.HookedKeys.Remove(control.Hotkey);
                //this.hotkeys[]
                /* 1. Remove the old hotkey from the hook
                 * 2. set hotkeys[][] = new KeyEventArgs(new hotkey)
                 * 3. control.Hotkey = ? // Do we even need to do this? Set to text?
                 */
            }

            this.changed.Clear();
            this.Close();
        }

        private void hotkeyControl_TextChanged(object sender, EventArgs e)
        {
            this.changed.Add((exscape.HotkeyControl)sender);
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }

        private void hotkeyControl1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
