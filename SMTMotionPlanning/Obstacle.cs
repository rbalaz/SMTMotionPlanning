using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTMotionPlanning
{
    public class Obstacle
    {
        // All obstacles are presumed to be rectangular
        // Location of the obstacle is defined with the top-leftmost corner of the obstacle
        private Coordinate location;
        private int length;
        private int width;

        public Obstacle(int length, int width, Coordinate location)
        {
            this.length = length;
            this.width = width;
            this.location = location;
        }

        public int getWidth()
        {
            return width;
        }

        public int getLength()
        {
            return length;
        }

        public Coordinate getLocation()
        {
            return location;
        }
    }
}
