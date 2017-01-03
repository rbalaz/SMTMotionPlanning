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

        public List<RectangularObstacle> transformObstacle(int obstaclePassDistance)
        {
            List<LineSegment> lines = getLines();
            List<RectangularObstacle> obstacles = new List<RectangularObstacle>();
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].isHorizontal)
                {
                    int leftX = (lines[i].start.x < lines[i].end.x ? lines[i].start.x : lines[i].end.x) - obstaclePassDistance;
                    int leftY = lines[i].start.y - obstaclePassDistance;
                    RectangularObstacle rectangle = new RectangularObstacle(2 * obstaclePassDistance,
                        Coordinate.getXDistanceBetweenCoordinates(lines[i].start, lines[i].end) + 2 * obstaclePassDistance, 
                        new Coordinate(leftX, leftY));
                    obstacles.Add(rectangle);
                }
                else if (lines[i].isVertical)
                {
                    int leftY = (lines[i].start.y < lines[i].end.y ? lines[i].start.y : lines[i].end.y) - obstaclePassDistance;
                    int leftX = lines[i].start.x - obstaclePassDistance;
                    RectangularObstacle rectangle = new RectangularObstacle(Coordinate.getYDistanceBetweenCoordinates(lines[i].start, lines[i].end) 
                        + 2 * obstaclePassDistance, 2 * obstaclePassDistance, new Coordinate(leftX, leftY));
                    obstacles.Add(rectangle);
                }
                else
                {
                    List<RectangularObstacle> segmentCases = encaseLineSegment(lines[i], obstaclePassDistance);
                    obstacles = obstacles.Concat(segmentCases).ToList();
                }
            }
            return obstacles;
        }

        private List<RectangularObstacle> encaseLineSegment(LineSegment segment, int obstaclePassDistance)
        {
            int xDifference = Math.Abs(segment.start.x - segment.end.x);
            int yDifference = Math.Abs(segment.start.y - segment.end.y);
            if (xDifference >= yDifference)
                return encaseLineSegmentByX(segment, obstaclePassDistance);
            else
                return encaseLineSegmentByY(segment, obstaclePassDistance);
        }

        private List<RectangularObstacle> encaseLineSegmentByX(LineSegment segment, int obstaclePassDistance)
        {
            double x = segment.start.x < segment.end.x ? segment.start.x : segment.end.x;
            double y = segment.start.x < segment.end.x ? segment.start.y : segment.end.y;
            double otherSideX = x == segment.start.x ? segment.end.x : segment.start.x;
            double otherSideY = y == segment.start.y ? segment.end.y : segment.start.y;
            RealCoordinate current = new RealCoordinate(x, y - obstaclePassDistance);
            RealCoordinate realEnd = new RealCoordinate(otherSideX, otherSideY);
            List<RectangularObstacle> obstacles = new List<RectangularObstacle>();
            RectangularObstacle firstObstacle = new RectangularObstacle(2 * obstaclePassDistance, obstaclePassDistance,
                new RealCoordinate(x - obstaclePassDistance, y - obstaclePassDistance));
            obstacles.Add(firstObstacle);
            do
            {
                RectangularObstacle obstacle = new RectangularObstacle(2 * obstaclePassDistance, 2,
                    new RealCoordinate(current.x, current.y));
                obstacles.Add(obstacle);
                if (current.x + 2 <= otherSideX)
                {
                    current.x += 2;
                    current.y = segment.k * current.x + segment.q - obstaclePassDistance;
                }
                else
                {
                    current.x = otherSideX;
                    current.y = otherSideY;
                }
            } while (RealCoordinate.getDistanceBetweenCoordinates(current, realEnd) != 0);
            RectangularObstacle lastObstacle = new RectangularObstacle(2 * obstaclePassDistance, obstaclePassDistance,
                new RealCoordinate(current.x, current.y));
            obstacles.Add(lastObstacle);

            return obstacles;
        }

        private List<RectangularObstacle> encaseLineSegmentByY(LineSegment segment, int obstaclePassDistance)
        {
            double x = segment.start.x < segment.end.x ? segment.start.x : segment.end.x;
            double y = segment.start.x < segment.end.x ? segment.start.y : segment.end.y;
            double otherSideX = x == segment.start.x ? segment.end.x : segment.start.x;
            double otherSideY = y == segment.start.y ? segment.end.y : segment.start.y;
            RealCoordinate current = new RealCoordinate(x - obstaclePassDistance, y);
            RealCoordinate realEnd = new RealCoordinate(otherSideX, otherSideY);
            List<RectangularObstacle> obstacles = new List<RectangularObstacle>();
            RectangularObstacle firstObstacle = new RectangularObstacle(obstaclePassDistance, 2 * obstaclePassDistance,
                new RealCoordinate(x - obstaclePassDistance, y - obstaclePassDistance));
            obstacles.Add(firstObstacle);
            do
            {
                RectangularObstacle obstacle = new RectangularObstacle(2, 2 * obstaclePassDistance,
                    new RealCoordinate(current.x, current.y));
                obstacles.Add(obstacle);
                if (current.y + 2 <= otherSideY)
                {
                    current.y += 2;
                    current.x = (current.y - segment.q) / segment.k - obstaclePassDistance;
                }
                else
                {
                    current.x = otherSideX;
                    current.y = otherSideY;
                }
            } while (RealCoordinate.getDistanceBetweenCoordinates(current, realEnd) != 0);
            RectangularObstacle lastObstacle = new RectangularObstacle(obstaclePassDistance, 2 * obstaclePassDistance,
                new RealCoordinate(current.x, current.y));
            obstacles.Add(lastObstacle);

            return obstacles;
        }
    }
}
