using System.Drawing;
using System.Drawing.Imaging;
using Accord.Imaging;
using Accord.Imaging.Filters;
using Accord.Math;
using Accord;

namespace SMTMotionPlanning
{
    class ImageStitching
    {
        // Images we are going to stitch together
        private Bitmap img1;
        private Bitmap img2;

        // Fields to store our interest points in the two images
        private IntPoint[] harrisPoints1;
        private IntPoint[] harrisPoints2;

        // Fields to store our correlated points
        private IntPoint[] correlationPoints1;
        private IntPoint[] correlationPoints2;

        // The homography matrix estimated by RANSAC
        private MatrixH homography;

        public ImageStitching(Bitmap first, Bitmap second)
        {
            img1 = first;
            img2 = second;
        }

        private Bitmap harris()
        {
            // Step 1: Detect feature points using Harris Corners Detector
            HarrisCornersDetector harris = new HarrisCornersDetector(0.04f, 1000f);
            harrisPoints1 = harris.ProcessImage(img1).ToArray();
            harrisPoints2 = harris.ProcessImage(img2).ToArray();

            // Show the marked points in the original images
            Bitmap img1mark = new PointsMarker(harrisPoints1).Apply(img1);
            Bitmap img2mark = new PointsMarker(harrisPoints2).Apply(img2);

            // Concatenate the two images together in a single image (just to show on screen)
            Concatenate concatenate = new Concatenate(img1mark);

            return concatenate.Apply(img2mark);
        }

        private Bitmap correlation()
        {
            // Step 2: Match feature points using a correlation measure
            CorrelationMatching matcher = new CorrelationMatching(9, img1, img2);
            IntPoint[][] matches = matcher.Match(harrisPoints1, harrisPoints2);
         
            // Get the two sets of points
            correlationPoints1 = matches[0];
            correlationPoints2 = matches[1];

            // Concatenate the two images in a single image (just to show on screen)
            Concatenate concat = new Concatenate(img1);
            Bitmap img3 = concat.Apply(img2);

            // Show the marked correlations in the concatenated image
            PairsMarker pairs = new PairsMarker(
                correlationPoints1, // Add image1's width to the X points
                                    // to show the markings correctly
                correlationPoints2.Apply(p => new IntPoint(p.X + img1.Width, p.Y)));

            return pairs.Apply(img3);
        }

        private Bitmap ransac()
        {
            // Step 3: Create the homography matrix using a robust estimator
            RansacHomographyEstimator ransac = new RansacHomographyEstimator(0.001, 0.99);
            homography = ransac.Estimate(correlationPoints1, correlationPoints2);

            // Plot RANSAC results against correlation results
            IntPoint[] inliers1 = correlationPoints1.Get(ransac.Inliers);
            IntPoint[] inliers2 = correlationPoints2.Get(ransac.Inliers);

            // Concatenate the two images in a single image (just to show on screen)
            Concatenate concat = new Concatenate(img1);
            Bitmap img3 = concat.Apply(img2);

            // Show the marked correlations in the concatenated image
            PairsMarker pairs = new PairsMarker(
                inliers1, // Add image1's width to the X points to show the markings correctly
                inliers2.Apply(p => new IntPoint(p.X + img1.Width, p.Y)));

            return pairs.Apply(img3);
        }

        private Bitmap blend()
        {
            // Step 4: Project and blend the second image using the homography
            Blend blend = new Blend(homography, img1);
            return blend.Apply(img2);
        }

        public void doItAll()
        {
            // Do it all
            Bitmap afterHarris = harris();
            Bitmap afterCorrelation = correlation();
            Bitmap afterRansac = ransac();
            Bitmap afterBlend = blend();

            afterHarris.Save("afterHarris.png", ImageFormat.Png);
            afterCorrelation.Save("afterCorrelation.png", ImageFormat.Png);
            afterRansac.Save("afterRansac.png", ImageFormat.Png);
            afterBlend.Save("afterBlend.png", ImageFormat.Png);
        }
    }
}
