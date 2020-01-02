using System.Collections.Generic;

namespace raka_no_f
{
    enum Position
    {
        top = 0,
        jg = 1,
        mid = 2,
        adc = 3,
        sup = 4,
        noe
    }

    // NOTE: We depend on casting integers to Enum, so we can't have multiple enums w/ same value
    public enum Spell
    {
        flash = 0,
        teleport = 1,
        heal = 2,
        ignite = 3,
        exhaust = 4,
        noe
    }

    class Enemy
    {
        private Position position;
        private bool has_insight;
        public Dictionary<Spell, int> cd { get; private set; }

        public Enemy(Position pos_, bool has_insight_)
        {
            position = pos_;
            has_insight = has_insight_;

            Cooldowns defaults = new Cooldowns();
            cd = new Dictionary<Spell, int>();

            for (Spell spell = Spell.flash; spell < Spell.noe; spell++)
            {
                cd[spell] = defaults[spell];
            }
        }
    }
}
