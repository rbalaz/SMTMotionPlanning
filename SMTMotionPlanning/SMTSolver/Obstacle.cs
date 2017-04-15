using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Z3;
namespace SMTMotionPlanning
{
    public abstract class Obstacle
    {
        public enum ObstacleType { Ellipse,Rectangle,Polygon,Spline}

        public ObstacleType type { get; set; }

        public Obstacle(ObstacleType type)
        {
            this.type = type;
        }
    }
}
