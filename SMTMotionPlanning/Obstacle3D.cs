using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTMotionPlanning
{
    class Obstacle3D : RectangularObstacle
    {
        private int height;

        public Obstacle3D(int length, int width, int height, Coordinate location) : base(length,width,location)
        {
            this.height = height;
        }

    }
}
