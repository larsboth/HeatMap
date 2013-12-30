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

        [Option('t', "stickyness", Required = false, HelpText = "Max distance for two points to be treated as equal.")]
        public float Stickyness { get; set; }

    }
    
}
