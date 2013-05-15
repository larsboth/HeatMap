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


        public Route(int routeIdx)
        {
         this.RouteIdx = routeIdx;   
        }

        public List<Leg> Legs
        {
            get
            {
                //CreateLegs();
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

            // Direkt wegsmoothbar?
            if (coordinates.Count > 2)
            {
                Coordinate lineStart = coordinates[coordinates.Count - 3];  //x1
                Coordinate lineEnd = coordinates[coordinates.Count - 1];    //x2
                Coordinate point = coordinates[coordinates.Count - 2];     //x

                //                (x - x1) / (x2 - x1) = (y - y1) / (y2 - y1) = (z - z1) / (z2 - z1)
                double valueA = (point.Latitude - lineStart.Latitude) / (lineEnd.Latitude - lineStart.Latitude);
                double valueB = (point.Longitude - lineStart.Longitude) / (lineEnd.Longitude - lineStart.Longitude);

                if (
                        Math.Abs(valueA - valueB) < 0.000000000000005 && //0.000000000005 &&
                        point.IsInRectangle( lineEnd, lineStart )
                    )
                {
                    coordinates.Remove(point);
                }
            }

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
                else
                {
                    
                }
                
            }

        }

    }
}
