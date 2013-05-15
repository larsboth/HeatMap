using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace HeatMap.Datentypen
{
    /// <summary>
    /// Basisklasse zum Aufbauen eines Nonaren Baums. Jeder Knoten stellt ein Quadrat auf der Karte dar und beinhaltet 9 Kinder, 
    /// die das aktuelle Quadrat in gleichgroße Teile unterteilt.
    /// </summary>
    class CoordinatesTreeNode
    {
        private List<Leg> legs = new List<Leg>();
        private CoordinatesTreeNode[,] children = new CoordinatesTreeNode[3,3];
        private CoordinatesTreeNode parent = null;
        private double segmentWidth;
        private Coordinate center = null;

        private static long nodeCount = 0;

        private Coordinate upperLeft = null;
        private Coordinate lowerRight = null;
        private Coordinate snapToCoordinate = null;

        public CoordinatesTreeNode(double segmentWidth, Coordinate upperLeft, Coordinate lowerRight, CoordinatesTreeNode center = null, CoordinatesTreeNode parent = null)
        {
            nodeCount++;

            this.segmentWidth = segmentWidth;
            this.upperLeft = upperLeft;
            this.lowerRight = lowerRight;
            this.parent = parent;

            children[1, 1] = center;
        }

        /// <summary>
        /// Ermittelt die Breite des aktuellen Knoten.
        /// </summary>
        /// <returns>Breite des aktuellen Knoten.</returns>
        public double ThisNodeWidth()
        {
            return Math.Round(Math.Abs(upperLeft.Longitude - lowerRight.Longitude), 14);
        }

        /// <summary>
        /// Ermittelt die Höhe des aktuellen Knoten.
        /// </summary>
        /// <returns>Höhe des aktuellen Knoten.</returns>
        public double ThisNodeHeight()
        {
            return Math.Round(Math.Abs(upperLeft.Latitude - lowerRight.Latitude), 14);
        }

        /// <summary>
        /// Prüft, ob die Koordinate innerhalb des durch den Knoten abgedeckten Quadrats liegt.
        /// </summary>
        /// <param name="coordinate">Zu prüfende Koordinate</param>
        /// <returns>Wahrheitswert, der anzeigt ob die Koordinate im Quadrat liegt.</returns>
        public bool IsInRange( Coordinate coordinate)
        {
            return coordinate.IsInRectangle(upperLeft, lowerRight);
        }

        
        /// <summary>
        /// Erstellt zum aktuellen Knoten einen passenden Elternknoten. Dieser hat die dreifache Kantenlänge und der Unterknoten im
        /// Zentrum ist der aktuelle Knoten.
        /// </summary>
        /// <returns>Den neu erstellten Elterknoten.</returns>
        public CoordinatesTreeNode CreateParent()
        {
            CoordinatesTreeNode myParent = new CoordinatesTreeNode( 
                segmentWidth,
                new Coordinate(upperLeft.Longitude - ThisNodeWidth() * 1, upperLeft.Latitude - ThisNodeWidth() * 1),
                new Coordinate(lowerRight.Longitude + ThisNodeWidth() * 1, lowerRight.Latitude + ThisNodeWidth() * 1),
                this
                );

            parent = myParent;

            return myParent;
        }


        /// <summary>
        /// Gibt die Liste der Legs zurück die innerhalb des Knoten, in dem die übergebene Koordinate liegt, gespeichert sind.
        /// </summary>
        /// <param name="coordinate">Koordinate über die der korrekte Knoten gefunden werden soll.</param>
        /// <returns>Liste von Legs.</returns>
        public List<Leg> GetLegs( Coordinate coordinate )
        {
            CoordinatesTreeNode ctn = FindNode(coordinate);
            if (ctn != null)
                return ctn.legs;

            // Wenn kein Knoten gefunden wurde, eine leer Liste zurückgeben.
            return new List<Leg>();
        }

        public Coordinate GetCenterCoordinate()
        {
            if ( center == null )
                center = new Coordinate(
                    upperLeft.Longitude + ThisNodeWidth() / 2,
                    upperLeft.Latitude + ThisNodeWidth() / 2
                );
            
            return center;
        }


        /// <summary>
        /// Findet den Blattknoten, in dem diese Koordinate erfasst ist. Gegebenenfalls werden hierfür neue
        /// Knoten erstellt. Die Tiefe des Baums wird automatisch erhöht, solange bis die minimale Kantenbreite
        /// unterschritten bzw. erreicht wird.
        /// </summary>
        /// <param name="coordinate">Suchkoordinate für den Knoten.</param>
        /// <returns>Liefert Knoten zurück, in dem die übergebene Koordinate liegt. Der Rückgabewert kann null sein, 
        /// falls die Koordinate in keinem (Unter-)knoten das aktuellen Knotens ist. Dies kann dazu interpretiert werden,
        /// einen neuen Oberknoten zu erstellen.</returns>
        public CoordinatesTreeNode FindNode( Coordinate coordinate )
        {

            //Console.WriteLine( "{0}, {1}", ThisNodeWidth(), segmentWidth );

            // Schon der richtige Knoten? Ist der Fall, wenn die minimale Kantenbreite unterschritten wird.
            // Sollte die Koordinate nicht innerhalb des Quadrats des aktuellen Knotens liegen, wird null
            // zurückgegeben werden.
            if ( ThisNodeWidth() <= segmentWidth )
            {
                if (
                    upperLeft.Latitude < coordinate.Latitude &&
                    upperLeft.Longitude < coordinate.Longitude &&
                    lowerRight.Latitude >= coordinate.Latitude &&
                    lowerRight.Longitude >= coordinate.Longitude
                    )
                {
                    return this;
                } else
                {
                    return null;
                }
            }


            // Falls nein, einen Unterknoten finden. Die Auswahl des Unterknoten findet über die Errechnung der
            // Arrayposition in einem zweidimensionalen Array statt (0, 1, 2), jeweils für die Nord/Süd und West/Ost-Ausrichtung.
            int northSouth = 0;
            int eastWest = 0;

            if (upperLeft.Latitude + (ThisNodeWidth() / 3) > coordinate.Latitude)
            {
                northSouth = 0;
            } else
                if (
                    upperLeft.Latitude + (ThisNodeWidth() / 3) <= coordinate.Latitude && 
                    upperLeft.Latitude + (ThisNodeWidth() / 3)* 2 > coordinate.Latitude
                    )
            {
                northSouth = 1;
            }
            else
            {
                northSouth = 2;
            }

            if (upperLeft.Longitude + (ThisNodeWidth() / 3) > coordinate.Longitude)
            {
                eastWest = 0;
            } else
                if (
                    upperLeft.Longitude + (ThisNodeWidth() / 3) <= coordinate.Longitude &&
                    upperLeft.Longitude + (ThisNodeWidth() / 3) * 2 > coordinate.Longitude
                    )
            {
                eastWest = 1;
            }
            else
            {
                eastWest = 2;
            }

            // Falls der selektierte Unterknoten nicht schon existiert, muss er neu erzeugt werden.
            if (children[eastWest, northSouth] == null)
            {
                children[eastWest, northSouth] = new CoordinatesTreeNode(segmentWidth,
                        new Coordinate(
                            upperLeft.Longitude + (ThisNodeWidth() / 3) * (eastWest),
                            upperLeft.Latitude + (ThisNodeWidth() / 3) * (northSouth)),
                        new Coordinate(
                            lowerRight.Longitude + (ThisNodeWidth() / 3) * (eastWest-2),
                            lowerRight.Latitude + (ThisNodeWidth() / 3) * (northSouth-2)),
                            null,
                            this
                    );
            }

            // Ermittlung des Unterknoten und Rückgabe.
            CoordinatesTreeNode returnValue = children[eastWest, northSouth].FindNode(coordinate);
            return returnValue;
        }

        /// <summary>
        /// Fügt dem aktuellen Knoten ein Leg hinzu. Die Koordinaten des Legs werden entsprechend der hinterlegten
        /// snapToCoordinate angepasst. Diese wiederum wird definiert durch das erste hinzugefügte Leg.
        /// </summary>
        /// <param name="leg"></param>
        /// <param name="coordinate"></param>
        /// <returns></returns>
        public bool AddLeg(Leg leg, Coordinate coordinate)
        {
            if (snapToCoordinate == null)
            {
                snapToCoordinate = new Coordinate(  
                        coordinate.Longitude,
                        coordinate.Latitude
                    );
            }

            coordinate.Longitude = snapToCoordinate.Longitude;
            coordinate.Latitude = snapToCoordinate.Latitude;

            // Legs ohne Länge werden nicht zugefügt.
            if (!leg.End.Equals(leg.Start))
            {
                legs.Add(leg);
                return true;
            }
            return false;
        }
    }
}
