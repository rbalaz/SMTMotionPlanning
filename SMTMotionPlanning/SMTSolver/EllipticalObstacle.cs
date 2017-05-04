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
