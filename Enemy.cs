using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace raka_no_f
{
    enum Position
    {
        top = 0,
        jg = 1,
        mid = 2,
        adc = 3,
        sup = 4
    }

    class Enemy
    {
        private Position position;
        private bool has_insight; // TODO: real rune names that change summ spell cd
        private int[] summonerCD;

        public Enemy(Position pos_, bool has_insight_)
        {
            position = pos_;
            has_insight = has_insight_;
        }
    }
}
