namespace SMTMotionPlanning
{
    class Circle
    {
        public int x { get; private set; }
        public int y { get; private set; }
        public int radius { get; private set; }
        public Circle(int x, int y, int radius)
        {
            this.x = x;
            this.y = y;
            this.radius = radius;
        }
    }
}
