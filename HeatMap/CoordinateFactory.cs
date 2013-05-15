using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HeatMap.Datentypen;

namespace HeatMap
{
    class CoordinateFactory
    {
        private int precision;

        public CoordinateFactory( int precision )
        {
            this.precision = precision;
        }

        public Coordinate CreateCoordinate( double lon, double lat )
        {
            // Anhand der Präzision auf einen anderen Punkt mappen.
            if (precision > 0)
            {
                lon = Math.Round(lon, precision);
                lat = Math.Round(lat, precision);
            }

            return new Coordinate(lon, lat);
        }
    }
}
