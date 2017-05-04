namespace SMTMotionPlanning
{
    public class RectangularObstacle : Obstacle
    {
        public Coordinate location { get; private set; }
        public RealCoordinate realLocation { get; set; }
        public int length { get; private set; }
        public int width { get; private set; }

        public RectangularObstacle(int length, int width, Coordinate location) : base(ObstacleType.Rectangle)
        {
            this.length = length;
            this.width = width;
            this.location = location;
        }

        public RectangularObstacle(int length, int width, RealCoordinate location) : base(ObstacleType.Rectangle)
        {
            this.length = length;
            this.width = width;
            realLocation = location;
        }
    }
}
