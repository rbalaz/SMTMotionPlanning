namespace SMTMotionPlanning
{
    using System;
    using System.Drawing;
    using Accord.MachineLearning;
    using Accord.Math;
    using Accord.Imaging;
    using Accord;

    public class RansacHomographyEstimator
    {
        private RANSAC<MatrixH> ransac;
        private int[] inliers;

        private PointF[] pointSet1;
        private PointF[] pointSet2;

        // Gets the RANSAC estimator used.
        public RANSAC<MatrixH> Ransac
        {
            get { return ransac; }
        }

        // Gets the final set of inliers detected by RANSAC.
        public int[] Inliers
        {
            get { return inliers; }
        }

        // Creates a new RANSAC homography estimator.
        public RansacHomographyEstimator(double threshold, double probability)
        {
            // Create a new RANSAC with the selected threshold
            ransac = new RANSAC<MatrixH>(4, threshold, probability);

            // Set RANSAC functions
            ransac.Fitting = homography;
            ransac.Distances = distance;
        }

        // Matches two sets of points using RANSAC.
        public MatrixH Estimate(Accord.Point[] points1, Accord.Point[] points2)
        {
            // Initial argument checkings
            if (points1.Length != points2.Length)
                throw new ArgumentException("The number of points should be equal.");

            if (points1.Length < 4)
                throw new ArgumentException("At least four points are required to fit an homography");

            PointF[] p1 = new PointF[points1.Length];
            PointF[] p2 = new PointF[points2.Length];
            for (int i = 0; i < points1.Length; i++)
            {
                p1[i] = new PointF(points1[i].X, points1[i].Y);
                p2[i] = new PointF(points2[i].X, points2[i].Y);
            }

            return Estimate(p1, p2);
        }

        // Matches two sets of points using RANSAC.
        public MatrixH Estimate(IntPoint[] points1, IntPoint[] points2)
        {
            // Initial argument checkings
            if (points1.Length != points2.Length)
                throw new ArgumentException("The number of points should be equal.");

            if (points1.Length < 4)
                throw new ArgumentException("At least four points are required to fit an homography");

            PointF[] p1 = new PointF[points1.Length];
            PointF[] p2 = new PointF[points2.Length];
            for (int i = 0; i < points1.Length; i++)
            {
                p1[i] = new PointF(points1[i].X, points1[i].Y);
                p2[i] = new PointF(points2[i].X, points2[i].Y);
            }

            return Estimate(p1, p2);
        }

        // Matches two sets of points using RANSAC.
        public MatrixH Estimate(PointF[] points1, PointF[] points2)
        {
            // Initial argument checkings
            if (points1.Length != points2.Length)
                throw new ArgumentException("The number of points should be equal.");

            if (points1.Length < 4)
                throw new ArgumentException("At least four points are required to fit an homography");


            // Normalize each set of points so that the origin is
            //  at centroid and mean distance from origin is sqrt(2).
            MatrixH T1, T2;
            pointSet1 = Accord.Imaging.Tools.Normalize(points1, out T1);
            pointSet2 = Accord.Imaging.Tools.Normalize(points2, out T2);


            // Compute RANSAC and find the inlier points
            MatrixH H = ransac.Compute(points1.Length, out inliers);

            if (inliers == null || inliers.Length < 4)
                return null;


            // Compute the final homography considering all inliers
            H = homography(inliers);

            // Denormalise
            H = T2.Inverse() * (H * T1);

            return H;
        }

        // Estimates a homography with the given points.
        private MatrixH homography(int[] points)
        {
            // Retrieve the original points
            PointF[] x1 = pointSet1.Get(points);
            PointF[] x2 = pointSet2.Get(points);

            // Compute the homography
            return Accord.Imaging.Tools.Homography(x1, x2);
        }

        // Compute inliers using the Symmetric Transfer Error,
        private int[] distance(MatrixH H, double t)
        {
            int n = pointSet1.Length;

            // Compute the projections (both directions)
            PointF[] p1 = H.TransformPoints(pointSet1);
            PointF[] p2 = H.Inverse().TransformPoints(pointSet2);

            // Compute the distances
            double[] d2 = new double[n];
            for (int i = 0; i < n; i++)
            {
                // Compute the distance as
                float ax = pointSet1[i].X - p2[i].X;
                float ay = pointSet1[i].Y - p2[i].Y;
                float bx = pointSet2[i].X - p1[i].X;
                float by = pointSet2[i].Y - p1[i].Y;
                d2[i] = (ax * ax) + (ay * ay) + (bx * bx) + (by * by);
            }

            // Find and return the inliers
            return Matrix.Find(d2, z => z < t);
        }
    }
}
