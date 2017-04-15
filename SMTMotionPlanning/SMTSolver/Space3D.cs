using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTMotionPlanning
{
    class Space3D : Space
    {
        private int height { get; set; }

        public Space3D(int width, int length, int height) : base(width,length)
        {
            this.height = height;
        }

    }
}
