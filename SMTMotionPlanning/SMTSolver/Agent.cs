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
