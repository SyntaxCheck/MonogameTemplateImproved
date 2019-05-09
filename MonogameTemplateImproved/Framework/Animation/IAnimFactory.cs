using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface IAnimFactory
{
    int NumberOfFrames { get; set; }
    double FrameDurationMs { get; set; }
}