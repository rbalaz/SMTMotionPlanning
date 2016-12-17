using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Z3;

namespace SMTMotionPlanning
{
    class EllipticalObstacle : Obstacle
    {
        public Coordinate location { get; private set; }
        public int width { get; private set; }
        public int length { get; private set; }

        public EllipticalObstacle(Coordinate location, int width, int length) : base(ObstacleType.Ellipse)
        {
            this.location = location;
            this.width = width;
            this.length = length;
        }
    }
}
