using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTMotionPlanning
{
    abstract class Space
    {
        class AgentAlreadyInitialisedException : Exception { }
        // World uses the traditional model used when displaying a canvas or window, placing [0,0] point int the 
        // top left corner of the screen and x and y incrementing down or right respectively.
        private int width;
        private int length;
        private List<Obstacle> obstacles;
        private Agent agent;

        public Space(int width, int length)
        {
            this.width = width;
            this.length = length;
            obstacles = new List<Obstacle>();
        }
        public void addObstacle(Obstacle obstacle)
        {
            obstacles.Add(obstacle);
        }
        public void removeObstacle(Obstacle obstacle)
        {
            obstacles.Remove(obstacle);
        }
        public void addAgent(Agent agent)
        {
            this.agent = agent;
        }

        public List<Obstacle> getObstacles()
        {
            return obstacles;
        }

        public int getWidth()
        {
            return width;
        }

        public int getLength()
        {
            return length;
        }

        public Agent getAgent()
        {
            return agent;
        }
    }
}
