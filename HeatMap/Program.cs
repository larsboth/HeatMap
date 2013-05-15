using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using HeatMap.Datentypen;

namespace HeatMap
{
    class Program
    {
        static CoordinateFactory coordinateFactory;

        static void Main(string[] args)
        {
            coordinateFactory = new CoordinateFactory( -1 );

            HeatMap.Datentypen.HeatMap hm = new HeatMap.Datentypen.HeatMap();

            int routeIdx = 0;

            foreach (string gpxFile in Directory.EnumerateFiles("C:\\Users\\Both\\Privat\\HeatMap\\Input", "*.gpx"))
            {
                var route = ReadGPX(gpxFile, routeIdx);
                hm.AddRoute(route);

                routeIdx++;
            }
            hm.CalculateHeat();

            WriteKML( hm );
        }

        private static Route ReadGPX(string gpxFile, int routeIdx)
        {
            XDocument xml = XDocument.Load(gpxFile);
            var trkpts = xml.Root.DescendantNodes().ToList().Where(
                node =>
                node.GetType() == typeof (XElement) &&
                ((XElement) node).Name.LocalName.Equals("trkpt")).Select(node => (XElement) node);

            double lon;
            double lat;
            Route route = new Route(routeIdx);

            foreach (var trkpt in trkpts)
            {
                if (
                    double.TryParse(trkpt.Attribute("lon").Value, System.Globalization.NumberStyles.Any,
                                    CultureInfo.InvariantCulture, out lon) &&
                    double.TryParse(trkpt.Attribute("lat").Value, System.Globalization.NumberStyles.Any,
                                    CultureInfo.InvariantCulture, out lat))
                {
                    Coordinate c = coordinateFactory.CreateCoordinate(lon, lat);
                    route.AddCoordinate(c);
                }
            }
            return route;
        }

        private static void WriteKML(HeatMap.Datentypen.HeatMap hm)
        {
            XDocument doc =
              new XDocument(
                  new XElement("kml",
                      //new XAttribute("xmlns", "http://earth.google.com/kml/2.2"),
                    new XElement("Document", 
                        new XElement("name", new XText("SomeName")),
                        new XElement("description", new XCData(""))
                        )
                    )
                  );

            XNamespace xmlns = "http://earth.google.com/kml/2.2";
            doc.Root.Name = xmlns + doc.Root.Name.LocalName;


            foreach (HeatRange heatRange in hm.GetHeatRanges())
            {
                // Styles hinzufügen
                doc.Descendants("Document").First().Add(
                        new XElement("Style",
                            new XAttribute("id", heatRange.RangeName),
                            new XElement("LineStyle",
                                new XElement("color", new XText(heatRange.Color)),
                                new XElement("width", new XText("5"))

                            )
                            )
                    );                
            }


            List<Leg> unwritten = hm.Heat.Keys.ToList();
            Dictionary<Leg, Boolean> written = new Dictionary<Leg, Boolean>();

            int segmentCount = 0;

            int maxIndex = 0;

            while( unwritten.Count > 0 )
            {
                List<Leg> thisTrack = new List<Leg>();
                // Einen nehmen und damit loslegen.
                int idx = 0;
                Leg leg = unwritten[idx];

                if ( written.ContainsKey( leg ) || 
                    unwritten.Contains( new Leg( leg.End, leg.Start, leg.RouteIdx ))
                    
                    )
                {
                    unwritten.Remove( leg );
                    continue;
                }

                segmentCount++;

                int heat = hm.Heat[leg];
                string heatStyle = GetHeatString( heat, hm );

                thisTrack.Add(leg);

//                unwritten.Remove(leg);

                idx++;

                while ( 
                    idx < unwritten.Count() &&
                    !written.ContainsKey( unwritten[idx] ) &&
                    (
                        //unwritten[idx].RouteIdx == leg.RouteIdx &&
                        unwritten[idx].Start.Equals(unwritten[idx-1].End)
                    ) &&
                        hm.CompareHeat(heat, hm.Heat[unwritten[idx]])
                        )
                {
                    thisTrack.Add(unwritten[idx]);
                    idx++;
                }

                foreach (Leg l in thisTrack)
                {
                    written.Add(l, true);
                }

                //written.AddRange(thisTrack);
                unwritten.RemoveRange(0,idx);

                if (idx > maxIndex)
                    maxIndex = idx;


                // thisTrack mit heat wegschreiben.

                string stringOfCoordinates = thisTrack[0].Start.ToString() + Environment.NewLine; ;
                foreach (Leg trackLeg in thisTrack)
                {
                    stringOfCoordinates += trackLeg.End.ToString() + Environment.NewLine;
                }



                doc.Descendants("Document").First().Add(
                            new XElement("Placemark",
                                new XElement("name", new XText(string.Format("Segment {0} {1} [{2}]", segmentCount, GetHeatString(heat, hm), heat))),
                                new XElement("description", new XCData("")), // new XText("<![CDATA[]]>")),
                                new XElement("styleUrl", new XText(string.Format("#{0}", heatStyle))),
                                //new XElement("ExtendedData",
                                //    new XElement("Data", new XAttribute("name", "_SnapToRoads"), 
                                //        new XElement("value", new XText("true"))
                                //    )
                                //),
                                new XElement("LineString",
                                    new XElement("tessellate", new XText("1")),
                                    new XElement("coordinates", new XText(stringOfCoordinates))
                                    )
                                )

                    );

                //break;
            }


            doc.Save("C:\\Users\\Both\\Privat\\HeatMap\\t.kml", SaveOptions.None);

        }



        private static string GetHeatString( int heat, HeatMap.Datentypen.HeatMap hm )
        {
            HeatRange heatRange = hm.GetHeatRange( heat ); //hm.GetHeatRanges().Where(hr => hr.MinHeat <= heat && heat <= hr.MaxHeat).FirstOrDefault();


            return heatRange == null ? "unknow" : heatRange.RangeName;
        }
    }
}
