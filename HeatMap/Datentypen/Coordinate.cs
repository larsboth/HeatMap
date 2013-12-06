using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace HeatMap.Datentypen
{
    public class Coordinate//: Tuple<double, double>
    {
        private double longitude;
        private double latitude;

        public double Latitude
        {
            get { return latitude; }
            set { latitude = value; }
        }

        public double Longitude
        {
            get { return longitude; }
            set { longitude = value; }
        }

        public Coordinate( double lon, double lat )
            //:base(lon, lat)
        {
            Longitude = lon;
            latitude = lat;
        }

        public override bool Equals(System.Object obj)
        {
            Coordinate comparee = (Coordinate)obj;

            return this.Longitude == comparee.Longitude && this.latitude == comparee.latitude;
        }



        public override int GetHashCode()
        {
            //return longitude.GetHashCode() + latitude.GetHashCode();
            //return base.GetHashCode();
            return latitude.GetHashCode() ^ Longitude.GetHashCode();

        }

        public double CalculateDistance( Coordinate comparee )
        {
            return
                Math.Sqrt(
                    Math.Pow(Math.Abs(latitude) - Math.Abs(comparee.latitude),2) +
                    Math.Pow(Math.Abs(Longitude) - Math.Abs(comparee.Longitude),2)
                );
        }


        public bool IsInRectangle( Coordinate a, Coordinate b )
        {
            Coordinate upper = a.Longitude <= b.Longitude ? a : b;
            Coordinate left = a.Latitude <= b.Latitude ? a : b;
            Coordinate lower = a.Longitude >= b.Longitude ? a : b;
            Coordinate right = a.Latitude >= b.Latitude ? a : b;


            return
                upper.Longitude <= Longitude &&
                Longitude <= lower.Longitude &&
                left.Latitude <= Latitude &&
                Latitude <= right.Latitude;

            //return
            //    (
            //        (a.Longitude < Longitude && b.Longitude >= Longitude) ||
            //        (a.Longitude > Longitude && b.Longitude <= Longitude)
            //    ) &&
            //    (
            //        (a.Latitude < Latitude && b.Latitude >= Latitude) ||
            //        (a.Latitude > Latitude && b.Latitude <= Latitude)
            //    );
        }

        public override string ToString()
        {
            return Longitude.ToString(CultureInfo.GetCultureInfo("en-GB")) + "," + latitude.ToString(CultureInfo.GetCultureInfo("en-GB"));
        }
    }
}
