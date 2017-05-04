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
