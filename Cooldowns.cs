using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace raka_no_f
{
    public class Cooldowns
    {
        private Dictionary<Spell, int> _defaults;

        public static readonly int _flash = 300;
        public static readonly int _teleport = 360;
        public static readonly int _ignite = 180;
        public static readonly int _exhaust = 180;

        public Cooldowns()
        {
            _defaults = new Dictionary<Spell, int>()
            {
                { Spell.flash, _flash },
                { Spell.teleport, _teleport },
                { Spell.ignite, _ignite },
                { Spell.exhaust, _exhaust }
            };
        }

        public int this[Spell i]
        {
            get { return this._defaults[i]; }
        }
    }
}
