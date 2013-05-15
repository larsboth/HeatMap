using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HeatMap.Datentypen
{
    public class HeatRange
    {
        private string rangeName;
        private string color;
        private int minHeat;
        private int maxHeat;

        public int MinHeat
        {
            get { return minHeat; }
            set { minHeat = value; }
        }

        public int MaxHeat
        {
            get { return maxHeat; }
            set { maxHeat = value; }
        }

        public string RangeName
        {
            get { return rangeName; }
            set { rangeName = value; }
        }

        public string Color
        {
            get { return color; }
            set { color = value; }
        }
    }
}
