using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTMotionPlanning
{
    public class Coordinate
    {
        class DimensionNotSupportedException : Exception { }

        private int[] coordinates;
        private bool is3DWorld;
        public bool Is3DWorld { get; }

        public Coordinate(int[] coordinates)
        {
            this.coordinates = coordinates;
            is3DWorld = (this.coordinates.Length == 3);
        }

        public Coordinate(int x, int y)
        {
            this.coordinates = new int[] { x, y };
        }

        public int getX()
        {
            return coordinates[0];
        }

        public int getY()
        {
            return coordinates[1];
        }

        public int getZ()
        {
            if (coordinates.Length < 3)
                throw new DimensionNotSupportedException();
            else
                return coordinates[2];
        }

        public string toString()
        {
            return "x: " + coordinates[0] + "   y:" + coordinates[1];
        }

        public static int getXDistanceBetweenCoordinates(Coordinate c1, Coordinate c2)
        {
            return Math.Abs(c1.getX() - c2.getX());
        }

        public static int getYDistanceBetweenCoordinates(Coordinate c1, Coordinate c2)
        {
            return Math.Abs(c1.getY() - c2.getY());
        }
    }
}
