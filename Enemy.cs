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

    // TODO: place enums in appropriate file
    // NOTE: We depend on casting integers to Enum, so we can't have multiple enums w/ same value
    enum Spell
    {
        flash = 0,
        ignite = 1,
        exhaust = 2,
        teleport = 3,
        noe
    }

    class Enemy
    {
        private Position position;
        private bool has_insight; // TODO: real rune names that change summ spell cd
        public Dictionary<Spell, uint> cd { get; } // Could also use indexing using enu, sparse array

        public Enemy(Position pos_, bool has_insight_)
        {
            position = pos_;
            has_insight = has_insight_;

            cd = new Dictionary<Spell, uint>();
            cd[Spell.flash] = 300; //TODO: defaults?
            cd[Spell.ignite] = 5;
            cd[Spell.exhaust] = 180;
            cd[Spell.teleport] = 360;
        }
    }
}
