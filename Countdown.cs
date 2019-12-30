using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace raka_no_f
{
    class Countdown
    {
        private System.Windows.Forms.Label m_label;
        public Position position { get; }
        public Spell spell { get; }

        private uint m_initial;
        private uint m_remaining;
        private string m_message;
        private System.Windows.Forms.Timer m_timer;

        public Countdown(Position pos_, 
                         Spell spell_, 
                         System.Windows.Forms.Label label_, 
                         uint remaining_,
                         string message_)
        {
            m_label = label_;
            m_message = message_;
            m_initial = remaining_;
            m_remaining = remaining_;
            position = pos_;
            spell = spell_;

            m_timer = new System.Windows.Forms.Timer();
            m_timer.Interval = 1000;
            m_timer.Tick += new System.EventHandler(this.onTick);
            m_timer.Start();
        }

        public void reset()
        {
            m_timer.Stop();
            m_remaining = m_initial;
            m_timer.Start();
        }

        private void onTick(object sender, System.EventArgs e)
        {
            if (m_remaining == 0)
            {
                m_timer.Stop();
                // TODO: some sort of visual alarm that the ability is now off cd? or getting close?
            }
            m_label.Text = m_message + m_remaining;
            m_remaining--;
        }
    }
}
