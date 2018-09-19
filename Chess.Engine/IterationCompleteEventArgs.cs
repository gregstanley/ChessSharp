using Chess.Engine.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chess.Engine
{
    public class IterationCompleteEventArgs : EventArgs
    {
        public int Depth { get; set; }
        public double Eval { get; set; }
        public PotentialBoard PotentialBoard { get; set; }
    }
}
