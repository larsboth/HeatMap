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
        private int smoothingRatio;

        //private double StickyDistance = 0.0004; //0.0004;
        private int minHeat;
        private int maxHeat;

        private List<HeatRange> ranges;
        private List<HeatRange> selectedRanges;

        public double StickyDistance { get; set; }
        public int HeatSteps { get; set; }
        public int SmoothingRatio
        {
            set
            {
                smoothingRatio = value;
                Route.SmoothingRatio = smoothingRatio;
            }
        }


        public HeatMap()
        {
            StickyDistance = 0.0004;
            HeatSteps = 3;
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

            ranges = new List<HeatRange>();

            ranges.Add(new HeatRange() { RangeName = "arctic", Color = "50780014" });
            ranges.Add(new HeatRange() { RangeName = "freezing", Color = "50F00A14" });
            ranges.Add(new HeatRange() { RangeName = "cold", Color = "50F03C14" });
            ranges.Add(new HeatRange() { RangeName = "cooler", Color = "5078A03C" });
            ranges.Add(new HeatRange() { RangeName = "cool 5", Color = "50147814" });
            ranges.Add(new HeatRange() { RangeName = "cool 4", Color = "5014B400" });
            ranges.Add(new HeatRange() { RangeName = "cool 3", Color = "5014F000" });
            ranges.Add(new HeatRange() { RangeName = "cool 2", Color = "5014F0A0" });
            ranges.Add(new HeatRange() { RangeName = "cool 1", Color = "5014E7FF" });
            ranges.Add(new HeatRange() { RangeName = "medium", Color = "5014BAFF" });
            ranges.Add(new HeatRange() { RangeName = "warm 1", Color = "50147CFF" });
            ranges.Add(new HeatRange() { RangeName = "warm 2", Color = "50146AFF" });
            ranges.Add(new HeatRange() { RangeName = "warm 3", Color = "50143EFF" });
            ranges.Add(new HeatRange() { RangeName = "warmer", Color = "50141AFF" });
            ranges.Add(new HeatRange() { RangeName = "hot", Color = "501400E6" });

            // Depending on number of choosen Heat steps, remove a few.
            selectedRanges = new List<HeatRange>();
            selectedRanges.Add(ranges.First());
            selectedRanges.Add(ranges.Last());

            float step = (ranges.Count()) / ((float)HeatSteps - 2);

            for (float stepLoop = 1; stepLoop < ranges.Count(); stepLoop += step)
            {
                if (!selectedRanges.Contains(ranges[(int)stepLoop])) {
                    selectedRanges.Add(ranges[(int)stepLoop]);
                }
            }


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
