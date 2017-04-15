using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Z3;

namespace SMTMotionPlanning
{
    class PolygonalObstacle : Obstacle
    {
        public List<Coordinate> points { get; set; }

        public PolygonalObstacle(List<Coordinate> points) : base(ObstacleType.Polygon)
        {
            this.points = points;
        }

        public List<LineSegment> getLines()
        {
            List<LineSegment> lines = new List<LineSegment>();
            for (int i = 0; i < points.Count - 1; i++)
            {
                LineSegment segment;
                if (points[i + 1].x != points[i].x)
                {
                    double k = (double)(points[i + 1].y - points[i].y) / (double)(points[i + 1].x - points[i].x);
                    double q = points[i].y - k * points[i].x;
                    segment = new LineSegment(k, q, points[i], points[i + 1]);
                    segment.isVertical = false;
                }
                else
                {
                    segment = new LineSegment(points[i].x, points[i], points[i + 1]);
                    segment.isVertical = true;
                }
                lines.Add(segment);
            }

            return lines;
        }
    }
}
