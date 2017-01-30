using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTMotionPlanning
{
    class RealLineSegment
    {
        public double k { get; private set; }
        public double q { get; private set; }
        public RealCoordinate start { get; private set; }
        public RealCoordinate end { get; private set; }

        public RealLineSegment(double k, double q, RealCoordinate start, RealCoordinate end)
        {
            this.k = k;
            this.q = q;
            this.start = start;
            this.end = end;
        }

    }
}
