using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTMotionPlanning
{
    abstract class Space
    {
        // World uses the traditional model used when displaying a canvas or window, placing [0,0] point int the 
        // top left corner of the screen and x and y incrementing down or right respectively.
        public int width { get; set; }
        public int length { get; set; }
        public List<Obstacle> obstacles { get; set; }
        public Agent agent { get; set; }

        public Space(int width, int length)
        {
            this.width = width;
            this.length = length;
            obstacles = new List<Obstacle>();
        }
    }
}
