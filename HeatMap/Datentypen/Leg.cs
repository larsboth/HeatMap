using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HeatMap.Datentypen
{
    public class Leg
    {
        private Coordinate start;
        private Coordinate end;
        private int routeIdx;

        public Coordinate Start
        {
            get { return start; }
            set { start = value; }
        }

        public Coordinate End
        {
            get { return end; }
            set { end = value; }
        }

        public int RouteIdx
        {
            get { return routeIdx; }
            set { routeIdx = value; }
        }

        public Leg( Coordinate start, Coordinate end, int routeIdx )
        {
            this.start = start;
            this.end = end;
            this.RouteIdx = routeIdx;
        }


        public override bool Equals( System.Object obj )
        {
            return 
                obj != null &&
                this.start.Equals(((Leg)obj).start) &&
                this.end.Equals(((Leg)obj).end);
        }

        public override int GetHashCode()
        {
            return start.GetHashCode() ^ end.GetHashCode();
        }
    }
}
