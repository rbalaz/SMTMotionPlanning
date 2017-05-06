using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Math.Geometry;

namespace SMTMotionPlanning
{
    class Segmentation
    {
        private int[] redBounds;
        private int[] greenBounds;
        private int[] blueBounds;
        private int minimumCornerDistance;
        private int originalImageHeight;
        private int originalImageWidth;

        public Segmentation(int[] redBounds, int[] greenBounds, int[] blueBounds, int minimumCornerDistance,
            int originalImageHeight, int originalImageWidth)
        {
            this.redBounds = redBounds;
            this.greenBounds = greenBounds;
            this.blueBounds = blueBounds;
            this.minimumCornerDistance = minimumCornerDistance;
            this.originalImageHeight = originalImageHeight;
            this.originalImageWidth = originalImageWidth;
        }

        public void ProcessImage(Bitmap bitmap)
        {
            // lock image
            BitmapData bitmapData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadWrite, bitmap.PixelFormat);

            // step 1 - turn background to black
            ColorFiltering colorFilter = new ColorFiltering();
            // coin.jpg had these boundaries: red <0,64>, green <0,64>, blue <0,64>
            colorFilter.Red = new IntRange(redBounds[0], redBounds[1]);
            colorFilter.Green = new IntRange(greenBounds[0], greenBounds[1]);
            colorFilter.Blue = new IntRange(blueBounds[0], blueBounds[1]);
            colorFilter.FillOutsideRange = false;
            colorFilter.ApplyInPlace(bitmapData);

            // step 2 - locating objects
            BlobCounter blobCounter = new BlobCounter();
            blobCounter.FilterBlobs = true;
            blobCounter.MinHeight = 10;
            blobCounter.MinWidth = 10;
            blobCounter.ProcessImage(bitmapData);
            Blob[] blobs = blobCounter.GetObjectsInformation();
            bitmap.UnlockBits(bitmapData);

            // step 3 - check objects' type and highlight
            SimpleShapeChecker shapeChecker = new SimpleShapeChecker();
            Graphics g = Graphics.FromImage(bitmap);
            Pen circlePen = new Pen(Color.Yellow, 2); // circles
            Pen strangeShapePen = new Pen(Color.Red, 2);       // quadrilateral with unknown sub-type
            Pen rectanglePen = new Pen(Color.Brown, 2);   // quadrilateral with known sub-type
            Pen trianglePen = new Pen(Color.Blue, 2);     // triangle

            // step 3.5 - prepare file to save obstacles
            string path = @"C:\Users\Robert\Documents\Visual Studio 2015\Projects\SMTMotionPlanning\SMTMotionPlanning\obstacleFiles\";
            FileStream stream = new FileStream(path + "obstacles.txt", FileMode.Create, FileAccess.Write);
            StreamWriter writer = new StreamWriter(stream);
            for (int i = 0, n = blobs.Length; i < n; i++)
            {
                List<IntPoint> edgePoints = blobCounter.GetBlobsEdgePoints(blobs[i]);
                edgePoints = eliminateDuplicates(edgePoints);
                AForge.Point center;
                List<IntPoint> corners;
                float radius;
                if (shapeChecker.IsCircle(edgePoints, out center, out radius))
                {
                    g.DrawEllipse(circlePen, center.X - radius, center.Y - radius, 2 * radius, 2 * radius);
                    writer.WriteLine("e " + (int)(center.X - radius) + " " + (int)(center.Y - radius)
                        + " " + (int)(radius) + " " + (int)(radius));
                }
                else if (shapeChecker.IsConvexPolygon(edgePoints, out corners))
                {
                    PolygonSubType polygonType = shapeChecker.CheckPolygonSubType(corners);
                    Pen pen;
                    if (polygonType == PolygonSubType.Rectangle || polygonType == PolygonSubType.Square)
                    {
                        pen = rectanglePen;
                        writer.WriteLine("r " + corners[0].X + " " + corners[0].Y + " " +
                            Math.Abs(corners[0].X - corners[1].X) + " " + Math.Abs(corners[0].Y - corners[2].Y));
                        g.DrawPolygon(pen, ToPointsArray(corners));
                    }
                    else
                    {
                        pen = corners.Count == 3 ? trianglePen : strangeShapePen;
                        g.DrawPolygon(pen, ToPointsArray(corners));
                        writer.Write("p");
                        foreach (IntPoint corner in corners)
                            writer.Write(" " + corner.X + " " + corner.Y);
                        writer.WriteLine();
                    }
                }
                else
                {
                    corners = orderPolygonCorners(findPolygonCorners(edgePoints), edgePoints);
                    g.DrawPolygon(strangeShapePen, ToPointsArray(corners));
                    writer.Write("p");
                    foreach (IntPoint corner in corners)
                        writer.Write(" " + corner.X + " " + corner.Y);
                    writer.WriteLine();
                }
            }

            circlePen.Dispose();
            strangeShapePen.Dispose();
            trianglePen.Dispose();
            rectanglePen.Dispose();
            writer.Close();
            stream.Close();
            g.Dispose();
            transformCoordinates();
        }

        // Convert list of AForge.NET's points to array of .NET points
        private System.Drawing.Point[] ToPointsArray(List<IntPoint> points)
        {
            System.Drawing.Point[] array = new System.Drawing.Point[points.Count];
            for (int i = 0, n = points.Count; i < n; i++)
            {
                array[i] = new System.Drawing.Point(points[i].X, points[i].Y);
            }
            return array;
        }

        private List<IntPoint> findPolygonCorners(List<IntPoint> edgePoints)
        {
            List<IntPoint> corners = new List<IntPoint>();
            foreach (IntPoint edge in edgePoints)
            {
                if (isCorner(edge, edgePoints))
                    corners.Add(edge);
            }
            corners = eliminateStackedCorners(corners);
            return corners;
        }

        private bool isCorner(IntPoint edge, List<IntPoint> edgePoints)
        {
            List<IntPoint> neighbouringEdges = new List<IntPoint>();
            foreach (IntPoint neighbour in edgePoints)
            {
                if (getDistanceBetweenTwoPoints(edge, neighbour) < minimumCornerDistance && neighbour != edge)
                    neighbouringEdges.Add(neighbour);
            }
            double maxAngle = -1, minAngle = -1;
            foreach (IntPoint neighbour in neighbouringEdges)
            {
                double angle = getLineAngle(edge, neighbour);
                if (maxAngle == -1)
                {
                    maxAngle = angle;
                    minAngle = angle;
                }
                if (angle > maxAngle)
                    maxAngle = angle;
                else if (angle < minAngle)
                    minAngle = angle;
            }
            double gradient = Math.Abs(maxAngle - minAngle);
            if (gradient > 30)
                return true;
            else
                return false;
        }

        private double getLineAngle(IntPoint edge, IntPoint neighbour)
        {
            double slope = (double)(neighbour.Y - edge.Y) / (double)(neighbour.X - edge.X);
            if (double.IsInfinity(slope))
                return 90.0;
            else
                return Math.Atan(slope) * (180 / Math.PI);
        }

        private double getDistanceBetweenTwoPoints(IntPoint edge, IntPoint neighbour)
        {
            double xDistancePow = Math.Pow(neighbour.X - edge.X, 2);
            double yDistancePow = Math.Pow(neighbour.Y - edge.Y, 2);
            return Math.Sqrt(xDistancePow + yDistancePow);
        }

        private List<IntPoint> eliminateStackedCorners(List<IntPoint> corners)
        {
            List<IntPoint> newCorners = new List<IntPoint>();
            List<IntPoint> globalNeighbours = new List<IntPoint>();
            foreach (IntPoint corner in corners)
            {
                if (checkIfListContainsSpecifiedPoint(globalNeighbours, corner))
                    continue;
                List<IntPoint> neighbours = getNeighbouringCorners(corner, corners);
                if (neighbours == null)
                    newCorners.Add(corner);
                else
                {
                    newCorners.Add(getCentroidCorner(corner, neighbours));
                    globalNeighbours.AddRange(neighbours);
                }
            }
            if (getMinimumCornerDistance(newCorners) < minimumCornerDistance)
                return eliminateStackedCorners(newCorners);
            else
                return newCorners;
        }

        private List<IntPoint> getNeighbouringCorners(IntPoint corner, List<IntPoint> corners)
        {
            List<IntPoint> neighbours = new List<IntPoint>();
            foreach (IntPoint neighbour in corners)
            {
                if (neighbour == corner)
                    continue;
                else if (getDistanceBetweenTwoPoints(neighbour, corner) < minimumCornerDistance)
                    neighbours.Add(neighbour);
            }
            if (neighbours.Count == 0)
                return null;
            else
                return neighbours;
        }

        private IntPoint getCentroidCorner(IntPoint corner, List<IntPoint> neighbours)
        {
            double centroidX = neighbours.Sum(item => item.X) + corner.X;
            double centroidY = neighbours.Sum(item => item.Y) + corner.Y;

            return new IntPoint((int)(centroidX / (neighbours.Count() + 1)), (int)(centroidY / (neighbours.Count() + 1)));
        }

        private double getMinimumCornerDistance(List<IntPoint> corners)
        {
            double distance = 50000;
            for (int i = 0; i < corners.Count; i++)
            {
                for (int j = 0; j < corners.Count; j++)
                {
                    if (i == j)
                        continue;
                    else
                    {
                        double Tdistance = getDistanceBetweenTwoPoints(corners[i], corners[j]);
                        distance = Tdistance < distance ? Tdistance : distance;
                    }
                }
            }
            return distance;
        }

        private List<IntPoint> orderPolygonCorners(List<IntPoint> corners, List<IntPoint> edgePoints)
        {
            List<IntPoint> orderedCorners = new List<IntPoint>();
            orderedCorners.Add(corners[0]);
            while (orderedCorners.Count < corners.Count)
            {
                List<double> estimatedAngles = determineLineAngle(orderedCorners[orderedCorners.Count - 1], edgePoints);
                double angleMatch = 0;
                double edgeMatchUp = 0;
                int index = 0;
                double matchCoeficient = 5;
                while (angleMatch == 0)
                {
                    for (int j = 1; j < corners.Count(); j++)
                    {
                        if (checkIfListContainsSpecifiedPoint(orderedCorners, corners[j]) == false)
                        {
                            double currentAngleMatch = getAngleMatch(orderedCorners[orderedCorners.Count - 1],
                                corners[j], edgePoints, estimatedAngles, matchCoeficient);
                            if (angleMatch < currentAngleMatch)
                            {
                                angleMatch = currentAngleMatch;
                                index = j;
                                edgeMatchUp = getEdgePointsMatchOfLine(orderedCorners[orderedCorners.Count - 1],
                                    corners[j], edgePoints);
                            }
                            else if (angleMatch == currentAngleMatch)
                            {
                                double currentEdgeMatchUp = getEdgePointsMatchOfLine(orderedCorners[orderedCorners.Count - 1],
                                    corners[j], edgePoints);
                                if (edgeMatchUp < currentEdgeMatchUp)
                                {
                                    index = j;
                                    edgeMatchUp = currentEdgeMatchUp;
                                }
                            }
                        }
                    }
                    matchCoeficient += 5;
                }
                orderedCorners.Add(corners[index]);
            }

            orderedCorners.Add(orderedCorners[0]);
            return orderedCorners;
        }

        private double getAngleMatch(IntPoint firstCorner, IntPoint secondCorner, List<IntPoint> edgePoints,
            List<double> estimatedAngles, double matchCoeficient)
        {
            // Direction vector d = (dx, dy)
            int dx = secondCorner.X - firstCorner.X;
            int dy = secondCorner.Y - firstCorner.Y;
            // Line equation
            // x = x1 + t*dx
            // y = y1 + t*dy
            double polygonLineAngle = getLineAngle(firstCorner, secondCorner);
            IntPoint two = new IntPoint(firstCorner.X + 2 * dx, firstCorner.Y + 2 * dy);
            double doubleProlongedLineAngle = getLineAngle(firstCorner, two);
            IntPoint three = new IntPoint(firstCorner.X + 3 * dx, firstCorner.Y + 3 * dy);
            double tripleProlongedLineAngle = getLineAngle(firstCorner, three);
            double evaluatedAngle = ((Math.Abs(polygonLineAngle - doubleProlongedLineAngle)
                + Math.Abs(polygonLineAngle - tripleProlongedLineAngle))) / 2;

            double match = 0;
            foreach (double angle in estimatedAngles)
            {
                if (Math.Abs(angle - evaluatedAngle) < matchCoeficient)
                    match++;
            }

            return match;
        }

        private List<double> determineLineAngle(IntPoint firstCorner, List<IntPoint> edgePoints)
        {
            List<IntPoint> neighbours = getNeighbouringCorners(firstCorner, edgePoints);
            List<double> rawAngleData = new List<double>();
            foreach (IntPoint neighbour in neighbours)
                rawAngleData.Add(getLineAngle(firstCorner, neighbour));
            return rawAngleData;
        }

        private List<IntPoint> eliminateDuplicates(List<IntPoint> corners)
        {
            List<IntPoint> newCorners = new List<IntPoint>();
            foreach (IntPoint corner in corners)
            {
                if (checkIfListContainsSpecifiedPoint(newCorners, corner) == false)
                    newCorners.Add(corner);
            }

            return newCorners;
        }

        private bool checkIfListContainsSpecifiedPoint(List<IntPoint> list, IntPoint item)
        {
            foreach (IntPoint point in list)
            {
                if (point.X == item.X && point.Y == item.Y)
                    return true;
            }
            return false;
        }

        private double getEdgePointsMatchOfLine(IntPoint firstCorner, IntPoint secondCorner, List<IntPoint> edgePoints)
        {
            int matchingEdges = 0;
            foreach (IntPoint point in edgePoints)
            {
                double distance = Math.Abs((secondCorner.Y - firstCorner.Y) * point.X - (secondCorner.X - firstCorner.X) *
                    point.Y + secondCorner.X * firstCorner.Y - secondCorner.Y * firstCorner.X) / Math.Sqrt(
                        Math.Pow(secondCorner.Y - firstCorner.Y, 2) + Math.Pow(secondCorner.X - firstCorner.X, 2));
                if (distance < 5)
                    matchingEdges++;
            }

            return matchingEdges / getDistanceBetweenTwoPoints(firstCorner, secondCorner);
        }

        private void transformCoordinates()
        {
            // Map will be resized to original image resolution
            string path = @"C:\Users\Robert\Documents\Visual Studio 2015\Projects\SMTMotionPlanning\SMTMotionPlanning\obstacleFiles\";
            FileStream readStream = new FileStream(path + "obstacles.txt", FileMode.Open, FileAccess.Read);
            StreamReader reader = new StreamReader(readStream);
            FileStream writeStream = new FileStream(path + "new_obstacles.txt", FileMode.Create, FileAccess.Write);
            StreamWriter writer = new StreamWriter(writeStream);
            string line;
            // 1280x800 will not fit, resizing is needed
            originalImageHeight = 600;
            originalImageWidth = 800;
            while ((line = reader.ReadLine()) != null)
            {
                string[] lineSegments = line.Split(' ');
                if (lineSegments[0] == "r")
                    writer.WriteLine(adjustRectangle(lineSegments));
                if (lineSegments[0] == "e")
                    writer.WriteLine(adjustEllipse(lineSegments));
                if (lineSegments[0] == "s" || lineSegments[0] == "p")
                    writer.WriteLine(adjustPointsShape(lineSegments));
            }
            writer.Close();
            writeStream.Close();
            reader.Close();
            readStream.Close();
        }

        private string adjustRectangle(string[] rectangleSegments)
        {
            int adjustedX = int.Parse(rectangleSegments[1]) * originalImageWidth / 485;
            int adjustedY = int.Parse(rectangleSegments[2]) * originalImageHeight / 281;
            int adjustedWidth = int.Parse(rectangleSegments[3]) * originalImageWidth / 485;
            int adjustedHeight = int.Parse(rectangleSegments[4]) * originalImageHeight / 281;
            string adjustedRectangle = rectangleSegments[0] + " " + adjustedX + " " + adjustedY + " " +
                adjustedWidth + " " + adjustedHeight;
            return adjustedRectangle;
        }

        private string adjustEllipse(string[] ellipseSegments)
        {
            int adjustedX = int.Parse(ellipseSegments[1]) * originalImageWidth / 485;
            int adjustedY = int.Parse(ellipseSegments[2]) * originalImageHeight / 281;
            int adjustedWidth = int.Parse(ellipseSegments[3]) * originalImageWidth / 485;
            int adjustedHeight = int.Parse(ellipseSegments[4]) * originalImageHeight / 281;
            string adjustedEllipse = ellipseSegments[0] + " " + adjustedX + " " + adjustedY + " " +
                adjustedWidth + " " + adjustedHeight;
            return adjustedEllipse;
        }
        private string adjustPointsShape(string[] points)
        {
            List<System.Drawing.Point> pointsList = new List<System.Drawing.Point>();
            for (int i = 1; i < points.Length; i = i + 2)
            {
                int x = int.Parse(points[i]) * originalImageWidth / 485;
                int y = int.Parse(points[i + 1]) * originalImageHeight / 281;
                pointsList.Add(new System.Drawing.Point(x, y));
            }
            string adjustedPoints = points[0];
            foreach (System.Drawing.Point point in pointsList)
                adjustedPoints = string.Concat(adjustedPoints, " ", point.X, " ", point.Y);
            return adjustedPoints;
        }
    }
}
