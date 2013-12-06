using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;
using CommandLine.Text;

namespace HeatMap
{
    class Options
    {
        [Option('i', "input", Required = true, HelpText = "Input directory to read.")]
        public string InputDir { get; set; }

        [Option('o', "output", Required = true, HelpText = "Output file to write.")]
        public string OutputFile { get; set; }

        [Option('h', "heatsteps", Required = false, HelpText = "Number of different colors used to create heatmap.")]
        public int HeatSteps { get; set; }

        [Option('s', "smoothing", Required = false, HelpText = "Factor between line length and point distance to line to be pruned.")]
        public int SmoothingRatio { get; set; }

        [Option('t', "stickyness", Required = false, HelpText = "Max distance for two points to be treated as equal.")]
        public float Stickyness { get; set; }

    }
    
}
