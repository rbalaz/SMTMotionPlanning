using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTMotionPlanning
{
    class Initializer
    {
        private string type;
        private int dimensions;
        public Initializer(string type, int dimensions)
        {
            this.type = type;
            this.dimensions = dimensions;
        }

        public Space initialiseWorld()
        {
            if (dimensions == 2)
                return initialize2DWorld();
            else
                return initialise3DWorld();
        }

        private Space2D initialize2DWorld()
        {
            if (type.Equals("basic"))
            {
                Space2D world = new Space2D(640, 480);
                RectangularObstacle front = new RectangularObstacle(40, 30, new Coordinate(new int[] { 40, 50 }));
                RectangularObstacle middle = new RectangularObstacle(70, 60, new Coordinate(new int[] { 240, 90 }));
                RectangularObstacle back = new RectangularObstacle(50, 50, new Coordinate(new int[] { 500, 120 }));
                world.obstacles.Add(front);
                world.obstacles.Add(middle);
                world.obstacles.Add(back);
                Agent agent = new Agent(new Coordinate(new int[] { 10, 10 }),10 ,10);
                world.agent = agent;

                return world;                       
            }
            return null;
        }

        private Space3D initialise3DWorld()
        {
            throw new NotImplementedException();
        }
    }
}
