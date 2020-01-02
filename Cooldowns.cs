using System.Collections.Generic;

namespace raka_no_f
{
    public class Cooldowns
    {
        private Dictionary<Spell, int> _defaults;

        public static readonly int _flash = 300;
        public static readonly int _teleport = 360;
        public static readonly int _ignite = 180;
        public static readonly int _exhaust = 210;
        public static readonly int _heal = 240;

        public Cooldowns()
        {
            _defaults = new Dictionary<Spell, int>()
            {
                { Spell.flash, _flash },
                { Spell.teleport, _teleport },
                { Spell.ignite, _ignite },
                { Spell.exhaust, _exhaust },
                { Spell.heal, _heal }
            };
        }

        public int this[Spell i]
        {
            get { return this._defaults[i]; }
        }
    }
}
