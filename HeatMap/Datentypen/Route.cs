using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HeatMap.Datentypen
{
    public class Route
    {
        private List<Coordinate> coordinates = new List<Coordinate>();
        private List<Leg> legs = new List<Leg>();
        private int routeIdx;

        public static double Stickyness { get; set; }

        public Route(int routeIdx)
        {
            this.RouteIdx = routeIdx;
        }

        public List<Leg> Legs
        {
            get
            {
                return legs;
            }
            set { legs = value; }
        }

        public int RouteIdx
        {
            get { return routeIdx; }
            set { routeIdx = value; }
        }

        public void AddCoordinate( Coordinate c )
        {
            coordinates.Add(c);

            if (coordinates.Count > 2)
            {
                Coordinate lineStart = coordinates[coordinates.Count - 3];  //x1
                Coordinate lineEnd = coordinates[coordinates.Count - 1];    //x2
                Coordinate point = coordinates[coordinates.Count - 2];     //x

                //                (x - x1) / (x2 - x1) = (y - y1) / (y2 - y1) = (z - z1) / (z2 - z1)
                double valueA = (point.Latitude - lineStart.Latitude) / (lineEnd.Latitude - lineStart.Latitude);
                double valueB = (point.Longitude - lineStart.Longitude) / (lineEnd.Longitude - lineStart.Longitude);

                 if (
                        DistanceFromPointToLine(point, lineStart, lineEnd) == 0
                    )
                {
                    coordinates.Remove(point);
                }
            }

        }

        public static double DistanceFromPointToLine(Coordinate point, Coordinate l1, Coordinate l2)
        {
            // given a line based on two points, and a point away from the line,
            // find the perpendicular distance from the point to the line.
            // see http://mathworld.wolfram.com/Point-LineDistance2-Dimensional.html
            // for explanation and defination.
            return Math.Abs((l2.Latitude - l1.Latitude) * (l1.Longitude - point.Longitude) - (l1.Latitude - point.Latitude) * (l2.Longitude - l1.Longitude)) /
                    Math.Sqrt(Math.Pow(l2.Latitude - l1.Latitude, 2) + Math.Pow(l2.Longitude - l1.Longitude, 2));
        }

        public void CreateLegs()
        {
            Coordinate start;
            Coordinate end;

            legs = new List<Leg>();
            start = coordinates[0];

            foreach ( Coordinate current in coordinates )
            {
                if (current == start)
                    continue;

                if (start.CalculateDistance(current) != 0)
                {
                    end = current;
                    legs.Add( new Leg(start, end, routeIdx) );
                    start = end;
                } 
            }
        }
    }
}
