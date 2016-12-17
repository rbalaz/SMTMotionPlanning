using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTMotionPlanning
{
    public class RealCoordinate
    {
        public double x { get; set; }
        public double y { get; set; }

        public RealCoordinate(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public static double getDistanceBetweenCoordinates(RealCoordinate c1, RealCoordinate c2)
        {
            return Math.Sqrt(Math.Pow(c1.x - c2.x, 2) + Math.Pow(c1.y - c2.y, 2));
        }
    }
}
