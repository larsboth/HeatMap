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

        private int minHeat;
        private int maxHeat;

        private List<HeatRange> ranges;
        private List<HeatRange> selectedRanges;

        public double StickyDistance { get; set; }

        public HeatMap()
        {
            StickyDistance = 0.0004;
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
                coordinatesTreeRoot = new CoordinatesTreeNode( StickyDistance, 
                        new Coordinate( coordinate.Longitude - StickyDistance*3, coordinate.Latitude - StickyDistance*3 ), 
                        new Coordinate( coordinate.Longitude + StickyDistance*3, coordinate.Latitude + StickyDistance*3 )
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
            return selectedRanges[heatMapping[heat]];
        }

        public List<HeatRange> GetHeatRanges()
        {
            if (selectedRanges != null)
                return selectedRanges;

            string opaqueness = "FF";

            ranges = new List<HeatRange>();

            ranges.Add(new HeatRange() { RangeName = "hot1", Color = opaqueness + "14F014" });
            ranges.Add(new HeatRange() { RangeName = "hot2", Color = opaqueness + "14F03C" });
            ranges.Add(new HeatRange() { RangeName = "hot3", Color = opaqueness + "14F0A0" });
            ranges.Add(new HeatRange() { RangeName = "hot4", Color = opaqueness + "14F0DC" });
            ranges.Add(new HeatRange() { RangeName = "hot5", Color = opaqueness + "14F0FF" });
            ranges.Add(new HeatRange() { RangeName = "hot6", Color = opaqueness + "14E7FF" });
            ranges.Add(new HeatRange() { RangeName = "hot7", Color = opaqueness + "14D5FF" });
            ranges.Add(new HeatRange() { RangeName = "hot8", Color = opaqueness + "14C3FF" });
            ranges.Add(new HeatRange() { RangeName = "hot9", Color = opaqueness + "14A8FF" });
            ranges.Add(new HeatRange() { RangeName = "hot10", Color = opaqueness + "1485FF" });
            ranges.Add(new HeatRange() { RangeName = "hot11", Color = opaqueness + "1461FF" });
            ranges.Add(new HeatRange() { RangeName = "hot12", Color = opaqueness + "143EFF" });
            ranges.Add(new HeatRange() { RangeName = "hot13", Color = opaqueness + "1400FF" });

            selectedRanges = ranges; 


            for (int idx = 0; idx < selectedRanges.Count; idx++)
            {
                selectedRanges[idx].MinHeat = this.MinHeat + (int)Math.Floor((this.MaxHeat - this.MinHeat) / (double)selectedRanges.Count * (double)idx);
                selectedRanges[idx].MaxHeat = this.MinHeat + (int)Math.Floor((this.MaxHeat - this.MinHeat) / (double)selectedRanges.Count * ((double)idx + 1));
            }

            int rangeID = 0;

            foreach (HeatRange range in selectedRanges)
            {
                for (int heatIdx = range.MinHeat; heatIdx <= range.MaxHeat; heatIdx++)
                {
                    heatMapping[heatIdx] = rangeID;
                }
                rangeID++;
            }

            if ( !heatMapping.ContainsKey(maxHeat) )
                heatMapping[maxHeat] = rangeID - 1;

            return selectedRanges;
        }

        public void CalculateHeat()
        {
            Heat = new Dictionary<Leg, int>();
            selectedRanges = null;

            foreach (Leg leg in allLegs)
            {
                List<Leg> legClusterStart = coordinatesTreeRoot.GetLegs( leg.Start );
                List<Leg> legClusterEnd = coordinatesTreeRoot.GetLegs(leg.End);
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
