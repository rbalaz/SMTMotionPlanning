using System.Collections.Generic;

namespace SMTMotionPlanning
{
    public class Space
    {
        // World uses the traditional model used when displaying a canvas or window, placing [0,0] point int the 
        // top left corner of the screen and x and y incrementing down or right respectively.
        public int width { get; set; }
        public int length { get; set; }
        public List<Obstacle> obstacles { get; set; }

        public Space(int width, int length)
        {
            this.width = width;
            this.length = length;
            obstacles = new List<Obstacle>();
        }
    }
}
