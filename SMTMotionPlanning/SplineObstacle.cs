using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTMotionPlanning
{
    class SplineObstacle : Obstacle
    {
        public List<Coordinate> points { get; set; }
        public SplineObstacle(List<Coordinate> points) : base(ObstacleType.Spline)
        {
            this.points = points;
        }

        public static double[] Polyfit(double[] x, double[] y, int degree)
        {
            // Vandermonde matrix
            var v = new DenseMatrix(x.Length, degree + 1);
            for (int i = 0; i < v.RowCount; i++)
                for (int j = 0; j <= degree; j++) v[i, j] = Math.Pow(x[i], j);
            var yv = new DenseVector(y).ToColumnMatrix();
            var qr = v.QR();
            // Math.Net doesn't have an "economy" QR, so:
            // cut R short to square upper triangle, then recompute Q
            var r = qr.R.SubMatrix(0, degree + 1, 0, degree + 1);
            var q = v.Multiply(r.Inverse());
            var p = r.Inverse().Multiply(q.TransposeThisAndMultiply(yv));
            return p.Column(0).ToArray();
        }

        public List<RectangularObstacle> transformObstacle(int obstaclePassDistance)
        {
            List<RectangularObstacle> obstacles = new List<RectangularObstacle>();
            if (points.Count > 4)
            {
                int currentCount = 0;
                List<SplineObstacle> splitObstacles = new List<SplineObstacle>();
                while (currentCount + 2 < points.Count)
                {
                    List<Coordinate> splitPoints = points.Skip(currentCount).Take(3).ToList();
                    splitObstacles.Add(new SplineObstacle(splitPoints));
                    currentCount += 2;
                }
                List<Coordinate> lastSplitPoints = points.Skip(currentCount).ToList();
                if (lastSplitPoints.Count > 1)
                {
                    splitObstacles.Add(new SplineObstacle(lastSplitPoints));
                }
                foreach (SplineObstacle spline in splitObstacles)
                    obstacles = obstacles.Concat(handleSplineObstacle(spline, obstaclePassDistance)).ToList();
            }
            else
                obstacles = obstacles.Concat(handleSplineObstacle(this, obstaclePassDistance)).ToList();

            return obstacles;
        }

        public List<RectangularObstacle> handleSplineObstacle(SplineObstacle obstacle, int obstaclePassDistance)
        {
            double[] x = obstacle.points.Select(item => (double)item.x).ToArray();
            double[] y = obstacle.points.Select(item => (double)item.y).ToArray();

            Polynom poly = new Polynom(Polyfit(x, y, obstacle.points.Count - 1).Reverse().ToArray());
            RealCoordinate current = new RealCoordinate(obstacle.points[0].x, obstacle.points[0].y - obstaclePassDistance);
            RealCoordinate end = new RealCoordinate(obstacle.points[obstacle.points.Count - 1].x,
                obstacle.points[obstacle.points.Count - 1].y);
            List<RectangularObstacle> obstacles = new List<RectangularObstacle>();
            RectangularObstacle firstObstacle = new RectangularObstacle(2 * obstaclePassDistance, obstaclePassDistance,
                new RealCoordinate(current.x - obstaclePassDistance, current.y - obstaclePassDistance));
            obstacles.Add(firstObstacle);
            do
            {
                RectangularObstacle rect = new RectangularObstacle(2 * obstaclePassDistance, 2,
                    new RealCoordinate(current.x, current.y));
                obstacles.Add(rect);
                if (current.x + 2 <= end.x)
                {
                    current.x += 2;
                    current.y = poly.getPolynomValue(current.x) - obstaclePassDistance;
                }
                else
                {
                    current.x = end.x;
                    current.y = end.y;
                }
            } while (RealCoordinate.getDistanceBetweenCoordinates(current, end) != 0);
            RectangularObstacle lastObstacle = new RectangularObstacle(2 * obstaclePassDistance, obstaclePassDistance,
                new RealCoordinate(current.x, current.y));
            obstacles.Add(lastObstacle);

            return obstacles;
        }
    }
}
