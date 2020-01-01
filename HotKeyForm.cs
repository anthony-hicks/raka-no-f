using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace raka_no_f
{
    public partial class HotKeyForm : Form
    {
        public HashSet<exscape.HotkeyControl> changed = new HashSet<exscape.HotkeyControl>();

        private HotKeyManager hkManager;
        private Dictionary<string, KeyEventArgs[]> hotkeys;
        public Dictionary<string, HotkeyDisplayControl> hkDisplayControls = new Dictionary<string, HotkeyDisplayControl>();

        public HotKeyForm(HotKeyManager hkManager_)
        {
            InitializeComponent();
            hkManager = hkManager_;
            this.button1.Click += new EventHandler(this.okButton_Click);
            this.AcceptButton = this.button1;

            this.Resize += new System.EventHandler(this.onResize);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.onClose);

            int posx = 50;
            int sumx = 260;
            int posy = 25;
            int sumy = 25;

            for (Position pos = Position.top; pos < Position.noe; pos++)
            {
                HotkeyDisplayControl hkDisplayControl = new HotkeyDisplayControl();

                Label label = Form1.createDefaultLabel(pos.ToString());
                label.Text = pos.ToString();
                label.Location = new Point(posx - 40, posy + 30);
                label.ForeColor = Color.Black;

                exscape.HotkeyControl hkControl = createDefaultHotkeyControl(pos.ToString());
                hkControl.TextChanged += new EventHandler(hotkeyControl_TextChanged);
                hkControl.Location = new Point(posx, posy += 30);

                hkDisplayControl.label = label;
                hkDisplayControl.hkControl = hkControl;
                hkDisplayControls[pos.ToString()] = hkDisplayControl;

                this.Controls.Add(label);
                this.Controls.Add(hkControl);
            }

            for (Spell spell = Spell.flash; spell < Spell.noe; spell++)
            {
                HotkeyDisplayControl hkDisplayControl = new HotkeyDisplayControl();

                Label label = Form1.createDefaultLabel(spell.ToString());
                label.Text = spell.ToString();
                label.Location = new Point(sumx - 55, sumy + 30);
                label.ForeColor = Color.Black;

                exscape.HotkeyControl hkControl = createDefaultHotkeyControl(spell.ToString());
                hkControl.TextChanged += new EventHandler(hotkeyControl_TextChanged);
                hkControl.Location = new Point(sumx, sumy += 30);

                hkDisplayControl.label = label;
                hkDisplayControl.hkControl = hkControl;
                hkDisplayControls[spell.ToString()] = hkDisplayControl;

                this.Controls.Add(label);
                this.Controls.Add(hkControl);
            }

            HotkeyDisplayControl clearControl = new HotkeyDisplayControl();
            Label clearLabel = Form1.createDefaultLabel("clear");
            clearLabel.Text = "clear";
            clearLabel.Location = new Point(sumx - 55, sumy + 30);
            clearLabel.ForeColor = Color.Black;

            exscape.HotkeyControl clearHkControl = createDefaultHotkeyControl("clear");
            clearHkControl.TextChanged += new EventHandler(hotkeyControl_TextChanged);
            clearHkControl.Location = new Point(sumx, sumy += 30);

            clearControl.label = clearLabel;
            clearControl.hkControl = clearHkControl;
            hkDisplayControls["clear"] = clearControl;

            this.Controls.Add(clearLabel);
            this.Controls.Add(clearHkControl);

            this.Hide();
        }

        private exscape.HotkeyControl createDefaultHotkeyControl(string name_)
        {
            exscape.HotkeyControl hkControl = new exscape.HotkeyControl();

            hkControl.Hotkey = Keys.None;
            hkControl.HotkeyModifiers = Keys.None;
            hkControl.Name = name_;
            hkControl.Size = new Size(125, 20);
            hkControl.Text = "";

            return hkControl;
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
            foreach (HotkeyDisplayControl control in this.hkDisplayControls.Values)
            {
                if (changed.Contains(control.hkControl))
                {
                    Spell spell;
                    Position position;
                    Keys oldKey;
                    Keys newKey = control.hkControl.Hotkey;
                    Keys newModifiers = control.hkControl.HotkeyModifiers;

                    if (Enum.TryParse<Spell>(control.label.Text, out spell))
                    {
                        oldKey = hkManager.spellKeys[(int)spell].KeyData;
                        hkManager.remove(oldKey);
                        hkManager.spellKeys[(int)spell] = new KeyEventArgs(newKey | newModifiers);
                    }
                    else if (Enum.TryParse<Position>(control.label.Text, out position))
                    {
                        oldKey = hkManager.positionKeys[(int)position].KeyData;
                        hkManager.remove(oldKey);
                        hkManager.positionKeys[(int)position] = new KeyEventArgs(newKey | newModifiers);
                    }
                    else
                    {
                        Console.WriteLine("Could not parse label.Text into an enum.");
                    }

                    hkManager.add(newKey);
                    changed.Remove(control.hkControl);
                }
            }
            this.Close();
        }

        private void hotkeyControl_TextChanged(object sender, EventArgs e)
        {
            changed.Add((exscape.HotkeyControl)sender);
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }

        private void hotkeyControl1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
