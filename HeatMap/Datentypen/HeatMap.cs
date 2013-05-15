using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HeatMap.Datentypen
{
    public class HeatMap
    {
        private List<Leg> allLegs = new List<Leg>();
        private Dictionary<Leg, int> heat = new Dictionary<Leg, int>();
        private Dictionary<int, int> heatMapping = new Dictionary<int, int>();

        private CoordinatesTreeNode coordinatesTreeRoot = null;

        private double stickyDistance = 0.0004;
        private int minHeat;
        private int maxHeat;

        private List<HeatRange> ranges;

        public HeatMap()
        {

        }

        public Dictionary<Leg, int> Heat
        {
            get { return heat; }
            set { heat = value; }
        }

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

        public void AddRoute(Route route)
        {
            route.CreateLegs();
            foreach (Leg leg in route.Legs)
            {
                AddLeg(leg);
            }
        }

        public void AddLeg(Leg leg)
        {

            if ( AddToNodesTree(leg, leg.End) )
            {
                AddToNodesTree(leg, leg.Start);
                allLegs.Add(leg);
            }
        }


        public bool AddToNodesTree( Leg leg, Coordinate coordinate )
        {
            if ( coordinatesTreeRoot == null )
            {
                coordinatesTreeRoot = new CoordinatesTreeNode( stickyDistance, 
                        new Coordinate( coordinate.Longitude - stickyDistance*3, coordinate.Latitude - stickyDistance*3 ), 
                        new Coordinate( coordinate.Longitude + stickyDistance*3, coordinate.Latitude + stickyDistance*3 )
                    );
            }

            CoordinatesTreeNode nodeToAddTo = null;
            Coordinate coordinateCopy = 
                new Coordinate(
                        coordinate.Longitude,
                        coordinate.Latitude
                    );

            int rootNodesAdded = 0;

            while (nodeToAddTo == null)
            {
                nodeToAddTo = coordinatesTreeRoot.FindNode(coordinateCopy);

                if ( nodeToAddTo == null )
                {
                    if (coordinatesTreeRoot.IsInRange(coordinateCopy))
                        return false;

                    coordinatesTreeRoot = coordinatesTreeRoot.CreateParent();
                    rootNodesAdded++;
                } else
                {
                    return nodeToAddTo.AddLeg(leg, coordinate);
                }
            }
            return false;
        }

        public bool CompareHeat(int heatA, int heatB)
        {
            return 
                heatMapping.ContainsKey(heatA) && heatMapping.ContainsKey(heatB) &&
                heatMapping[heatA] == heatMapping[heatB];
        }

        public HeatRange GetHeatRange( int heat )
        {
            return ranges[heatMapping[heat]];
        }

        public List<HeatRange> GetHeatRanges()
        {
            if (ranges != null)
                return ranges;

            ranges = new List<HeatRange>();

            //ranges.Add(new HeatRange() { RangeName = "arctic", Color = "64143C00" });
            //ranges.Add(new HeatRange() { RangeName = "freezing", Color = "64147800" });
            ranges.Add(new HeatRange() { RangeName = "cold", Color = "6414B400" });
            //ranges.Add(new HeatRange() { RangeName = "cooler", Color = "6414F000" });
            //ranges.Add(new HeatRange() { RangeName = "cool 5", Color = "6414F082" });
            //ranges.Add(new HeatRange() { RangeName = "cool 4", Color = "6414F0AA" });
            //ranges.Add(new HeatRange() { RangeName = "cool 3", Color = "6414F0D2" });
            ranges.Add(new HeatRange() { RangeName = "cool 2", Color = "6414F0E6" });
            //ranges.Add(new HeatRange() { RangeName = "cool 1", Color = "6414F0FA" });
            //ranges.Add(new HeatRange() { RangeName = "medium", Color = "6414B4D2" });
            ranges.Add(new HeatRange() { RangeName = "warm 1", Color = "6414B4FF" });
            //ranges.Add(new HeatRange() { RangeName = "warm 2", Color = "641478E6" });
            //ranges.Add(new HeatRange() { RangeName = "warm 3", Color = "641478FF" });
            //ranges.Add(new HeatRange() { RangeName = "warmer", Color = "64143CFF" });
            ranges.Add(new HeatRange() { RangeName = "hot", Color = "641400FF" });
            //ranges.Add(new HeatRange() { RangeName = "hotter", Color = "647846F0" });
            //ranges.Add(new HeatRange() { RangeName = "hottest", Color = "647800F0" });
            //ranges.Add(new HeatRange() { RangeName = "boiling", Color = "647800B4" });
            ranges.Add(new HeatRange() { RangeName = "steam", Color = "64780078" });

            for (int idx = 0; idx < ranges.Count; idx++)
            {
                ranges[idx].MinHeat = this.MinHeat + (int)Math.Floor((this.MaxHeat - this.MinHeat) / (double)ranges.Count * (double)idx);
                ranges[idx].MaxHeat = this.MinHeat + (int)Math.Floor((this.MaxHeat - this.MinHeat) / (double)ranges.Count * ((double)idx + 1));

            }

            int rangeID = 0;

            foreach ( HeatRange range in ranges )  {
                for (int heatIdx = range.MinHeat; heatIdx <= range.MaxHeat; heatIdx++)
                {
                    heatMapping[heatIdx] = rangeID;
                }
                rangeID++;
            }

            if ( !heatMapping.ContainsKey(maxHeat) )
                heatMapping[maxHeat] = rangeID - 1;

            return ranges;
        }

        public void CalculateHeat()
        {
            Heat = new Dictionary<Leg, int>();

            foreach (Leg leg in allLegs)
            {
                List<Leg> legClusterStart = coordinatesTreeRoot.GetLegs( leg.Start );
                List<Leg> legClusterEnd = coordinatesTreeRoot.GetLegs(leg.End);

                //Heat[leg] = legClusterStart.Select(l => l.RouteIdx).Distinct().Count();
                Heat[leg] = legClusterStart.Count() + legClusterEnd.Count();
            }

            if (Heat.Values.Count > 0)
            {
                MinHeat = Heat.Values.Min();
                MaxHeat = Heat.Values.Max();
            }

        }
    }
}
