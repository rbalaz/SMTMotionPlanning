using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTMotionPlanning
{
    public class Coordinate
    {
        public int x { get; private set; }
        public int y { get; private set; }
        public int z { get; private set; }
        private bool is3DWorld;
        public bool Is3DWorld { get; }

        public Coordinate(int[] coordinates)
        {
            x = coordinates[0];
            y = coordinates[1];
            if (coordinates.Length == 3)
                z = coordinates[2];

            is3DWorld = (coordinates.Length == 3);
        }

        public Coordinate(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public Coordinate(int x, int y, int z):this(x, y)
        {
            this.z = z;
        }

        public string toString()
        {
            return "x: " + x + "   y:" + y;
        }

        public static int getXDistanceBetweenCoordinates(Coordinate c1, Coordinate c2)
        {
            return Math.Abs(c1.x - c2.x);
        }

        public static int getYDistanceBetweenCoordinates(Coordinate c1, Coordinate c2)
        {
            return Math.Abs(c1.y - c2.y);
        }
    }
}
