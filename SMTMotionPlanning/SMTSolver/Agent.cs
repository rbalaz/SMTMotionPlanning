﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTMotionPlanning
{
    class Agent
    {
        public Coordinate currentLocation { get; private set; }
        private int width;
        private int length;

        public Agent(Coordinate startLocation, int width, int length)
        {
            this.length = length;
            currentLocation = startLocation;
            this.width = width;
        }
    }
}