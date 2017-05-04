namespace SMTMotionPlanning
{
    class LineSegment
    {
        public double k { get; set; }
        public double q { get; set; }
        public Coordinate start { get; set; }
        public Coordinate end { get; set; }
        public bool isVertical { get; set; }
        public bool isHorizontal { get; set; }
        public int xval { get; set; }

        public LineSegment(double k, double q, Coordinate start, Coordinate end)
        {
            this.k = k;
            this.q = q;
            this.start = start;
            this.end = end;
            if (k == 0)
                isHorizontal = true;
            else
                isHorizontal = false;
        }

        public LineSegment(int xval, Coordinate start, Coordinate end)
        {
            this.xval = xval;
            this.start = start;
            this.end = end;
            isHorizontal = false;
        }
    }
}
