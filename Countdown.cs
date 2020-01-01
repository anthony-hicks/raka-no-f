using System;

namespace raka_no_f
{
    class Countdown : System.IDisposable
    {
        public System.Windows.Forms.Label label { get; private set; }
        public Position position { get; }
        public Spell spell { get; }
        public bool done { get; private set; }

        private int m_alert_remaining;
        private int m_initial;
        private int m_remaining;
        private string m_message;
        private System.Windows.Forms.Timer m_timer;
        private System.Windows.Forms.Timer m_alert_timer;

        public Countdown(Position pos_,
                         Spell spell_,
                         System.Windows.Forms.Label label_,
                         int remaining_,
                         string message_)
        {
            label = label_;
            m_message = message_;
            m_initial = remaining_;
            m_remaining = remaining_;
            m_alert_remaining = 10;

            position = pos_;
            spell = spell_;

            m_alert_timer = new System.Windows.Forms.Timer();
            m_alert_timer.Interval = 1000;
            m_alert_timer.Tick += new System.EventHandler(this.onAlertTick);

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
            if (m_remaining < 0)
            {
                m_timer.Stop();
                m_alert_timer.Start();
            }
            else
            {
                label.Text = m_message + m_remaining;
                m_remaining--;
            }
        }

        private void onAlertTick(object sender, System.EventArgs e)
        {
            if (m_alert_remaining < 0)
            {
                done = true;
                m_alert_timer.Stop();
            }
            else
            {
                //TODO: visual alarm? Flash text possible?
                label.Text = m_message + "UP";
                m_alert_remaining--;
            }
        }

        public void Dispose()
        {
            ((IDisposable)m_timer).Dispose();
            ((IDisposable)m_alert_timer).Dispose();
            ((IDisposable)label).Dispose();
        }
    }
}
