using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTMotionPlanning
{
    class PathCommandsGenerator
    {
        private int usedCameraId;
        public double initialOrientation { get; set; }
        List<Coordinate> path;
        public int mapWidth;
        public int mapHeight;

        public PathCommandsGenerator(int usedCameraId, List<Coordinate> path, int mapWidth, int mapHeight)
        {
            this.usedCameraId = usedCameraId;
            this.path = path;
            this.mapHeight = mapHeight;
            this.mapWidth = mapWidth;
        }

        public void generateAndSaveCommands()
        {
            FileStream stream = new FileStream("commands.txt", FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter writer = new StreamWriter(stream);
            double currentOrientation = initialOrientation;
            for (int i = 0; i < path.Count - 1; i++)
            {
                double orientationChange = changeOrientation(path[i], path[i + 1], currentOrientation);
                writer.WriteLine("Rotate: " + orientationChange);
                currentOrientation += orientationChange;
                double length = calculateSegmentLength(path[i], path[i + 1]);
                writer.WriteLine("Move forward: " + length);
            }
            writer.Close();
            stream.Close();
        }

        private double changeOrientation(Coordinate start, Coordinate end, double currentOrientation)
        {
            /*
                        0
                        |
                 270 ---|--- 90
                        |
                       180
            */
            // A = [0 ,0 ] B = [0 ,-1] u = A - B
            // C = [xs,ys] D = [xd,yd] v = A - B
            int ux =  0;
            int uy = -1;
            int vx = end.x - start.x;
            int vy = end.y - start.y;
            double uLength = 1;
            double vLength = Math.Sqrt(Math.Pow(vx, 2) + Math.Pow(vy, 2));
            int uDotv = ux * vx + uy * vy;
            double angle = Math.Acos(uDotv / (uLength * vLength));
            if (isThirdOrFourthQuadrant(start, end))
                angle = 2 * Math.PI - angle;
            return angle - currentOrientation;
        }

        private bool isThirdOrFourthQuadrant(Coordinate start, Coordinate end)
        {
            if (end.x < start.x)
                return true;
            else
                return false;

        }

        private double calculateSegmentLength(Coordinate start, Coordinate end)
        {
            /*           227
                ----------------------
                | 0.3  | 0.26 | 0.29 |
                | 0.35 | 0.25 | 0.34 |
                | 0.31 | 0.26 | 0.29 |
                ----------------------
              */
            double distance = 0;
            distance += getTopLeftDistance(start, end) * 0.3;
            distance += getTopMiddleDistance(start, end) * 0.26;
            distance += getTopRightDistance(start, end) * 0.29;
            distance += getMiddleLeftDistance(start, end) * 0.35;
            distance += getMiddleMiddleDistance(start, end) * 0.25;
            distance += getMiddleRightDistance(start, end) * 0.34;
            distance += getBottomLeftDistance(start, end) * 0.31;
            distance += getBottomMiddleDistance(start, end) * 0.26;
            distance += getBottomRightDistance(start, end) * 0.29;
            return distance;
        }

        private double getTopLeftDistance(Coordinate start, Coordinate end)
        {
            List<RealCoordinate> intersects = new List<RealCoordinate>();
            // x = x0 + sx*t
            // y = y0 + sy*t
            int sx = end.x - start.x;
            int sy = end.y - start.y;
            // 1. Check if path segment intersects with the bottom border of the sector
            // Bottom sector line border equation: y = 1/3 * mapHeight
            // Border points: [0,1/3*mapHeight],[1/3*mapWidth,1/3*mapHeight]
            double y0 = mapHeight / 3;
            double vx = mapWidth / 3;
            double t = (double)(y0 - start.y) / (double)sy;
            double s = (start.x + sx * t) / vx;
            if (t <= 1 && t >= 0 && s <= 1 && s >= 0)
            {
                double intersectX = start.x + sx * t;
                double intersectY = start.y + sy * t;
                RealCoordinate intersect = new RealCoordinate(intersectX, intersectY);
                intersects.Add(intersect);
            }
            // 2. Check if path segment intersects right border of the sector 
            // Right sector line border equation: x = 1/3 * mapWidth
            // Border points: [1/3*mapWidth,0],[1/3*mapWidth,1/3*mapHeight]
            double x0 = mapWidth / 3;
            double vy = mapHeight / 3;
            t = (double)(x0 - start.x) / (double)sx;
            s = (start.y + sy * t) / vy;
            if (t <= 1 && t >= 0 && s <= 1 && s >= 0)
            {
                double intersectX = start.x + sx * t;
                double intersectY = start.y + sy * t;
                RealCoordinate intersect = new RealCoordinate(intersectX, intersectY);
                intersects.Add(intersect);
            }
            // If 2 intersects were found, distance between them is measured:
            if (intersects.Count == 2)
            {
                return RealCoordinate.getDistanceBetweenCoordinates(intersects[0], intersects[1]);
            }
            // If 1 intersecting point, there are 2 possibilities:
            // a) starting point is inside the sector
            // b) end point is inside the sector
            if (intersects.Count == 1)
            {
                if (start.x >= 0 && start.x < mapWidth / 3)
                    if (start.y >= 0 && start.y < mapHeight / 3)
                        return RealCoordinate.getDistanceBetweenCoordinates(intersects[0], new RealCoordinate(start.x, start.y));
                if (end.x >= 0 && end.x < mapWidth / 3)
                    if (end.y >= 0 && end.y < mapHeight / 3)
                        return RealCoordinate.getDistanceBetweenCoordinates(intersects[0], new RealCoordinate(end.x, end.y));
            }
            // If no intersecting points were found, segment can still be fully inside the sector without
            // intersecting its borders
            if (intersects.Count == 0)
            {
                if (start.x >= 0 && start.x < mapWidth / 3 && start.y >= 0 && start.y < mapHeight / 3)
                    if (end.x >= 0 && end.x < mapWidth / 3 && end.y >= 0 && end.y < mapHeight / 3)
                        return RealCoordinate.getDistanceBetweenCoordinates(new RealCoordinate(start.x, start.y), new RealCoordinate(end.x, end.y));
            }
            return 0;
        }

        private double getTopMiddleDistance(Coordinate start, Coordinate end)
        {
            List<RealCoordinate> intersects = new List<RealCoordinate>();
            int sx = end.x - start.x;
            int sy = end.y - start.y;
            // 1. Check if segments intersects with the left border of the sector
            // Left sector line border equation: x = 1/3 * mapWidth
            // Border points: [1/3*mapWidth,0],[2/3*mapWidth,1/3*mapHeight]
            double x0 = mapWidth / 3;
            double vy = mapHeight / 3;
            double t = (double)(x0 - start.x) / (double)sx;
            double s = (start.y + sy * t) / vy;
            if (t <= 1 && t >= 0 && s <= 1 && s >= 0)
            {
                double intersectX = start.x + sx * t;
                double intersectY = start.y + sy * t;
                RealCoordinate intersect = new RealCoordinate(intersectX, intersectY);
                intersects.Add(intersect);
            }
            // 2. Check if segments intersect with the bottom border of the sector
            // Bottom sector line border equation: y = 1/3 * mapHeight
            // Border points: [1/3*mapWidth,1/3*mapHeight],[2/3*mapWidth,1/3*mapHeight]
            x0 = 1 / 3 * mapWidth;
            double y0 = 1 / 3 * mapHeight;
            double vx = 1 / 3 * mapWidth;
            t = (double)(y0 - start.y) / (double)sy;
            s = (start.x + sx * t - x0) / vx;
            if (t <= 1 && t >= 0 && s <= 1 && s >= 0)
            {
                double intersectX = start.x + sx * t;
                double intersectY = start.y + sy * t;
                RealCoordinate intersect = new RealCoordinate(intersectX, intersectY);
                intersects.Add(intersect);
            }
            // 3. Check if segments intersect with the right border of the sector
            // Right sector line border equation: x = 2/3 * mapHeight
            // Border points: [2/3*mapWidth,0],[2/3*mapWidth,1/3*mapHeight]
            x0 = 2 / 3 * mapWidth;
            vy = 1 / 3 * mapHeight;
            t = (double)(x0 - start.x) / (double)sx;
            s = (start.y + sy * t) / vy;
            if (t <= 1 && t >= 0 && s <= 1 && s >= 0)
            {
                double intersectX = start.x + sx * t;
                double intersectY = start.y + sy * t;
                RealCoordinate intersect = new RealCoordinate(intersectX, intersectY);
                intersects.Add(intersect);
            }
            // If 2 intersects were found, distance between them is measured:
            if (intersects.Count == 2)
            {
                return RealCoordinate.getDistanceBetweenCoordinates(intersects[0], intersects[1]);
            }
            // If 1 intersecting point, there are 2 possibilities:
            // a) starting point is inside the sector
            // b) end point is inside the sector
            if (intersects.Count == 1)
            {
                if (start.x >= mapWidth / 3 && start.x < 2 * mapWidth / 3)
                    if (start.y >= 0 && start.y < mapHeight / 3)
                        return RealCoordinate.getDistanceBetweenCoordinates(intersects[0], new RealCoordinate(start.x, start.y));
                if (end.x >= mapWidth / 3 && end.x < 2 * mapWidth / 3)
                    if (end.y >= 0 && end.y < mapHeight / 3)
                        return RealCoordinate.getDistanceBetweenCoordinates(intersects[0], new RealCoordinate(end.x, end.y));
            }
            // If no intersecting points were found, segment can still be fully inside the sector without
            // intersecting its borders
            if (intersects.Count == 0)
            {
                if (start.x >= mapWidth / 3 && start.x < 2 * mapWidth / 3 && start.y >= 0 && start.y < mapHeight / 3)
                    if (end.x >= mapWidth / 3 && end.x < 2 * mapWidth / 3 && end.y >= 0 && end.y < mapHeight / 3)
                        return RealCoordinate.getDistanceBetweenCoordinates(new RealCoordinate(start.x, start.y), new RealCoordinate(end.x, end.y));
            }
            return 0;
        }

        private double getTopRightDistance(Coordinate start, Coordinate end)
        {
            List<RealCoordinate> intersects = new List<RealCoordinate>();
            int sx = end.x - start.x;
            int sy = end.y - start.y;
            // 1. Check if segments intersect with the left border of the sector
            // Right sector line border equation: x = 2/3 * mapHeight
            // Border points: [2/3*mapWidth,0],[2/3*mapWidth,1/3*mapHeight]
            double x0 = 2 / 3 * mapWidth;
            double vy = 1 / 3 * mapHeight;
            double t = (double)(x0 - start.x) / (double)sx;
            double s = (start.y + sy * t) / vy;
            if (t <= 1 && t >= 0 && s <= 1 && s >= 0)
            {
                double intersectX = start.x + sx * t;
                double intersectY = start.y + sy * t;
                RealCoordinate intersect = new RealCoordinate(intersectX, intersectY);
                intersects.Add(intersect);
            }
            // 2. Check if segments intersect with the bottom border of the sector
            // Bottom sector line border equation: y = 1/3 * mapHeight
            // Border points: [2/3*mapWidth,1/3*mapHeight],[mapWidth,1/3*mapHeight]
            x0 = 2 / 3 * mapWidth;
            double y0 = 1 / 3 * mapHeight;
            double vx = 1 / 3 * mapWidth;
            t = (double)(y0 - start.y) / (double)sy;
            s = (start.x + sx * t - x0) / vx;
            if (t <= 1 && t >= 0 && s <= 1 && s >= 0)
            {
                double intersectX = start.x + sx * t;
                double intersectY = start.y + sy * t;
                RealCoordinate intersect = new RealCoordinate(intersectX, intersectY);
                intersects.Add(intersect);
            }
            // If 2 intersects were found, distance between them is measured:
            if (intersects.Count == 2)
            {
                return RealCoordinate.getDistanceBetweenCoordinates(intersects[0], intersects[1]);
            }
            // If 1 intersecting point, there are 2 possibilities:
            // a) starting point is inside the sector
            // b) end point is inside the sector
            if (intersects.Count == 1)
            {
                if (start.x >= 2 * mapWidth / 3 && start.x <= mapWidth)
                    if (start.y >= 0 && start.y < mapHeight / 3) 
                        return RealCoordinate.getDistanceBetweenCoordinates(intersects[0], new RealCoordinate(start.x, start.y));
                if (end.x >= 2 * mapWidth / 3 && end.x <= mapWidth) 
                    if (end.y >= 0 && end.y < mapHeight / 3)
                        return RealCoordinate.getDistanceBetweenCoordinates(intersects[0], new RealCoordinate(end.x, end.y));
            }
            // If no intersecting points were found, segment can still be fully inside the sector without
            // intersecting its borders
            if (intersects.Count == 0)
            {
                if (start.x >= 2 * mapWidth / 3 && start.x <= mapWidth && start.y >= 0 && start.y < mapHeight / 3)
                    if (end.x >= 2 * mapWidth / 3 && end.x <= mapWidth && end.y >= 0 && end.y < mapHeight / 3)
                        return RealCoordinate.getDistanceBetweenCoordinates(new RealCoordinate(start.x, start.y), new RealCoordinate(end.x, end.y));
            }
            return 0;
        }

        private double getMiddleLeftDistance(Coordinate start, Coordinate end)
        {
            List<RealCoordinate> intersects = new List<RealCoordinate>();
            int sx = end.x - start.x;
            int sy = end.y - start.y;
            // 1. Check if path segment intersects with the top border of the sector
            // Bottom sector line border equation: y = 1/3 * mapHeight
            // Border points: [0,1/3*mapHeight],[1/3*mapWidth,1/3*mapHeight]
            double y0 = mapHeight / 3;
            double vx = mapWidth / 3;
            double t = (double)(y0 - start.y) / (double)sy;
            double s = (start.x + sx * t) / vx;
            if (t <= 1 && t >= 0 && s <= 1 && s >= 0)
            {
                double intersectX = start.x + sx * t;
                double intersectY = start.y + sy * t;
                RealCoordinate intersect = new RealCoordinate(intersectX, intersectY);
                intersects.Add(intersect);
            }
            // 2. Check if path segment intersects right border of the sector 
            // Right sector line border equation: x = 1/3 * mapWidth
            // Border points: [1/3*mapWidth,1/3*mapHeight],[1/3*mapWidth,2/3*mapHeight]
            double x0 = mapWidth / 3;
            y0 = mapHeight / 3;
            double vy = mapHeight / 3;
            t = (double)(x0 - start.x) / (double)sx;
            s = (start.y + sy * t - y0) / vy;
            if (t <= 1 && t >= 0 && s <= 1 && s >= 0)
            {
                double intersectX = start.x + sx * t;
                double intersectY = start.y + sy * t;
                RealCoordinate intersect = new RealCoordinate(intersectX, intersectY);
                intersects.Add(intersect);
            }
            // 3. Check if path segment intersects with the bottom border of the sector
            // Bottom sector line border equation: y = 1/3 * mapHeight
            // Border points: [0,2/3*mapHeight],[1/3*mapWidth,2/3*mapHeight]
            y0 = 2 * mapHeight / 3;
            vx = mapWidth / 3;
            t = (double)(y0 - start.y) / (double)sy;
            s = (start.x + sx * t) / vx;
            if (t <= 1 && t >= 0 && s <= 1 && s >= 0)
            {
                double intersectX = start.x + sx * t;
                double intersectY = start.y + sy * t;
                RealCoordinate intersect = new RealCoordinate(intersectX, intersectY);
                intersects.Add(intersect);
            }
            // If 2 intersects were found, distance between them is measured:
            if (intersects.Count == 2)
            {
                return RealCoordinate.getDistanceBetweenCoordinates(intersects[0], intersects[1]);
            }
            // If 1 intersecting point, there are 2 possibilities:
            // a) starting point is inside the sector
            // b) end point is inside the sector
            if (intersects.Count == 1)
            {
                if (start.x >= 0 && start.x < mapWidth / 3)
                    if (start.y >= mapHeight / 3 && start.y < 2 * mapHeight / 3) 
                        return RealCoordinate.getDistanceBetweenCoordinates(intersects[0], new RealCoordinate(start.x, start.y));
                if (end.x >= 0 && end.x < mapWidth / 3)
                    if (end.y >= mapHeight / 3 && end.y < 2 * mapHeight / 3) 
                        return RealCoordinate.getDistanceBetweenCoordinates(intersects[0], new RealCoordinate(end.x, end.y));
            }
            // If no intersecting points were found, segment can still be fully inside the sector without
            // intersecting its borders
            if (intersects.Count == 0)
            {
                if (start.x >= 0 && start.x < mapWidth / 3 && start.y >= mapHeight / 3 && start.y < 2 * mapHeight / 3)
                    if (end.x >= 0 && end.x < mapWidth / 3 && end.y >= mapHeight / 3 && end.y < 2 * mapHeight / 3)
                        return RealCoordinate.getDistanceBetweenCoordinates(new RealCoordinate(start.x, start.y), new RealCoordinate(end.x, end.y));
            }
            return 0;
        }

        private double getMiddleMiddleDistance(Coordinate start, Coordinate end)
        {
            List<RealCoordinate> intersects = new List<RealCoordinate>();
            int sx = end.x - start.x;
            int sy = end.y - start.y;
            // 1. Check if path segment intersects with the left border of the sector
            // Left sector line border equation: x = 1/3 * mapWidth
            // Border points: [1/3*mapWidth,1/3*mapHeight],[1/3*mapWidth,2/3*mapHeight]
            double x0 = mapWidth / 3;
            double y0 = mapHeight / 3;
            double vy = mapHeight / 3;
            double t = (double)(x0 - start.x) / (double)sx;
            double s = (start.y + sy * t - y0) / vy;
            if (t <= 1 && t >= 0 && s <= 1 && s >= 0)
            {
                double intersectX = start.x + sx * t;
                double intersectY = start.y + sy * t;
                RealCoordinate intersect = new RealCoordinate(intersectX, intersectY);
                intersects.Add(intersect);
            }
            // 2. Check if path segment intersects with the top border of the sector
            // Top sector line border equation: y = 1/3 * mapWidth
            // Border points: [1/3*mapWidth,1/3*mapHeight],[2/3*mapWidth,1/3*mapHeight]
            x0 = 1 / 3 * mapWidth;
            y0 = 1 / 3 * mapHeight;
            double vx = 1 / 3 * mapWidth;
            t = (double)(y0 - start.y) / (double)sy;
            s = (start.x + sx * t - x0) / vx;
            if (t <= 1 && t >= 0 && s <= 1 && s >= 0)
            {
                double intersectX = start.x + sx * t;
                double intersectY = start.y + sy * t;
                RealCoordinate intersect = new RealCoordinate(intersectX, intersectY);
                intersects.Add(intersect);
            }
            // 3. Check if path segment intersects with the bottom border of the sector
            // Bottom sector line border equation: y = 2/3 * mapWidth
            // Border points: [1/3*mapWidth,2/3*mapHeight],[2/3*mapWidth,2/3*mapHeight]
            x0 = 1 / 3 * mapWidth;
            y0 = 2 / 3 * mapHeight;
            vx = 1 / 3 * mapWidth;
            t = (double)(y0 - start.y) / (double)sy;
            s = (start.x + sx * t - x0) / vx;
            if (t <= 1 && t >= 0 && s <= 1 && s >= 0)
            {
                double intersectX = start.x + sx * t;
                double intersectY = start.y + sy * t;
                RealCoordinate intersect = new RealCoordinate(intersectX, intersectY);
                intersects.Add(intersect);
            }
            // 4. Check if path segment intersects with the right border of the sector
            // Right sector line border equation: x = 2/3 * mapHeight
            // Border points: [2/3*mapWidth,1/3*mapHeight],[2/3*mapWidth,2/3*mapHeight]
            x0 = 2 * mapWidth / 3;
            y0 = mapHeight / 3;
            vy = mapHeight / 3;
            t = (double)(x0 - start.x) / (double)sx;
            s = (start.y + sy * t - y0) / vy;
            if (t <= 1 && t >= 0 && s <= 1 && s >= 0)
            {
                double intersectX = start.x + sx * t;
                double intersectY = start.y + sy * t;
                RealCoordinate intersect = new RealCoordinate(intersectX, intersectY);
                intersects.Add(intersect);
            }
            // If 2 intersects were found, distance between them is measured:
            if (intersects.Count == 2)
            {
                return RealCoordinate.getDistanceBetweenCoordinates(intersects[0], intersects[1]);
            }
            // If 1 intersecting point, there are 2 possibilities:
            // a) starting point is inside the sector
            // b) end point is inside the sector
            if (intersects.Count == 1)
            {
                if (start.x >= mapWidth / 3 && start.x < 2 * mapWidth / 3)
                    if (start.y >= mapHeight / 3 && start.y < 2 * mapHeight / 3) 
                        return RealCoordinate.getDistanceBetweenCoordinates(intersects[0], new RealCoordinate(start.x, start.y));
                if (end.x >= mapWidth / 3 && end.x < 2 * mapWidth / 3)
                    if (end.y >= mapHeight / 3 && end.y < 2 * mapHeight / 3) 
                        return RealCoordinate.getDistanceBetweenCoordinates(intersects[0], new RealCoordinate(end.x, end.y));
            }
            // If no intersecting points were found, segment can still be fully inside the sector without
            // intersecting its borders
            if (intersects.Count == 0)
            {
                if (start.x >= mapWidth / 3 && start.x < 2 * mapWidth / 3 && start.y >= mapHeight / 3 && start.y < 2 * mapHeight / 3)
                    if (end.x >= mapWidth / 3 && end.x < 2 * mapWidth / 3 && end.y >= mapHeight / 3 && end.y < 2 * mapHeight / 3)
                        return RealCoordinate.getDistanceBetweenCoordinates(new RealCoordinate(start.x, start.y), new RealCoordinate(end.x, end.y));
            }
            return 0;
        }

        private double getMiddleRightDistance(Coordinate start, Coordinate end)
        {
            List<RealCoordinate> intersects = new List<RealCoordinate>();
            int sx = end.x - start.x;
            int sy = end.y - start.y;
            // 1. Check if path segment intersects with the left border of the sector
            // Left sector line border equation: x = 2/3 * mapWidth
            // Border points: [2/3*mapWidth,1/3*mapHeight],[2/3*mapWidth,2/3*mapHeight]
            double x0 = 2 * mapWidth / 3;
            double y0 = mapHeight / 3;
            double vy = mapHeight / 3;
            double t = (double)(x0 - start.x) / (double)sx;
            double s = (start.y + sy * t - y0) / vy;
            if (t <= 1 && t >= 0 && s <= 1 && s >= 0)
            {
                double intersectX = start.x + sx * t;
                double intersectY = start.y + sy * t;
                RealCoordinate intersect = new RealCoordinate(intersectX, intersectY);
                intersects.Add(intersect);
            }
            // 2. Check if the path segment intersects with the top border of the sector
            // Top sector line border equation: y = 1/3 * mapHeight
            // Border points: [2/3*mapWidth,1/3*mapHeight],[mapWidth,1/3*mapHeight]
            x0 = 2 / 3 * mapWidth;
            y0 = 1 / 3 * mapHeight;
            double vx = 1 / 3 * mapWidth;
            t = (double)(y0 - start.y) / (double)sy;
            s = (start.x + sx * t - x0) / vx;
            if (t <= 1 && t >= 0 && s <= 1 && s >= 0)
            {
                double intersectX = start.x + sx * t;
                double intersectY = start.y + sy * t;
                RealCoordinate intersect = new RealCoordinate(intersectX, intersectY);
                intersects.Add(intersect);
            }
            // 3. Check if the path segment intersects with the bottom border of the sector
            // Bottom sector line border equation: y = 2/3 * mapHeight
            // Border points: [2/3*mapWidth,2/3*mapHeight],[mapWidth,2/3*mapHeight]
            x0 = 2 / 3 * mapWidth;
            y0 = 2 / 3 * mapHeight;
            vx = 1 / 3 * mapWidth;
            t = (double)(y0 - start.y) / (double)sy;
            s = (start.x + sx * t - x0) / vx;
            if (t <= 1 && t >= 0 && s <= 1 && s >= 0)
            {
                double intersectX = start.x + sx * t;
                double intersectY = start.y + sy * t;
                RealCoordinate intersect = new RealCoordinate(intersectX, intersectY);
                intersects.Add(intersect);
            }
            // If 2 intersects were found, distance between them is measured:
            if (intersects.Count == 2)
            {
                return RealCoordinate.getDistanceBetweenCoordinates(intersects[0], intersects[1]);
            }
            // If 1 intersecting point, there are 2 possibilities:
            // a) starting point is inside the sector
            // b) end point is inside the sector
            if (intersects.Count == 1)
            {
                if (start.x >= 2 * mapWidth / 3 && start.x <= mapWidth) 
                    if (start.y >= mapHeight / 3 && start.y < 2 * mapHeight / 3)
                        return RealCoordinate.getDistanceBetweenCoordinates(intersects[0], new RealCoordinate(start.x, start.y));
                if (end.x >= 2 * mapWidth / 3 && end.x <= mapWidth) 
                    if (end.y >= mapHeight / 3 && end.y < 2 * mapHeight / 3) 
                return RealCoordinate.getDistanceBetweenCoordinates(intersects[0], new RealCoordinate(end.x, end.y));
            }
            // If no intersecting points were found, segment can still be fully inside the sector without
            // intersecting its borders
            if (intersects.Count == 0)
            {
                if (start.x >= 2 * mapWidth / 3 && start.x <= mapWidth && start.y >= mapHeight / 3 && start.y < 2 * mapHeight / 3)
                    if (end.x >= 2 * mapWidth / 3 && end.x <= mapWidth && end.y >= mapHeight / 3 && end.y < 2 * mapHeight / 3)
                        return RealCoordinate.getDistanceBetweenCoordinates(new RealCoordinate(start.x, start.y), new RealCoordinate(end.x, end.y));
            }
            return 0;
        }

        private double getBottomLeftDistance(Coordinate start, Coordinate end)
        {
            List<RealCoordinate> intersects = new List<RealCoordinate>();
            int sx = end.x - start.x;
            int sy = end.y - start.y;
            // 1. Check if path segment intersects with the top border of the sector
            // Top sector line border equation: y = 1/3 * mapHeight
            // Border points: [0,2/3*mapHeight],[1/3*mapWidth,2/3*mapHeight]
            double y0 = 2 * mapHeight / 3;
            double vx = mapWidth / 3;
            double t = (double)(y0 - start.y) / (double)sy;
            double s = (start.x + sx * t) / vx;
            if (t <= 1 && t >= 0 && s <= 1 && s >= 0)
            {
                double intersectX = start.x + sx * t;
                double intersectY = start.y + sy * t;
                RealCoordinate intersect = new RealCoordinate(intersectX, intersectY);
                intersects.Add(intersect);
            }
            // 2. Check if path segment intersects with the right border of the sector
            // Right sector line border equation: x = 1/3 * mapHeight
            // Border points: [1/3*mapWidth,2/3*mapHeight],[1/3*mapWidth,mapHeight]
            double x0 = mapWidth / 3;
            y0 = 2 * mapHeight / 3;
            double vy = mapHeight / 3;
            t = (double)(x0 - start.x) / (double)sx;
            s = (start.y + sy * t - y0) / vy;
            if (t <= 1 && t >= 0 && s <= 1 && s >= 0)
            {
                double intersectX = start.x + sx * t;
                double intersectY = start.y + sy * t;
                RealCoordinate intersect = new RealCoordinate(intersectX, intersectY);
                intersects.Add(intersect);
            }
            // If 2 intersects were found, distance between them is measured:
            if (intersects.Count == 2)
            {
                return RealCoordinate.getDistanceBetweenCoordinates(intersects[0], intersects[1]);
            }
            // If 1 intersecting point, there are 2 possibilities:
            // a) starting point is inside the sector
            // b) end point is inside the sector
            if (intersects.Count == 1)
            {
                if (start.x >= 0 && start.x < mapWidth / 3)
                    if (start.y >= 2 * mapHeight / 3 && start.y <= mapHeight) 
                        return RealCoordinate.getDistanceBetweenCoordinates(intersects[0], new RealCoordinate(start.x, start.y));
                if (end.x >= 0 && end.x < mapWidth / 3)
                    if (end.y >= 2 * mapHeight / 3 && end.y <= mapHeight) 
                        return RealCoordinate.getDistanceBetweenCoordinates(intersects[0], new RealCoordinate(end.x, end.y));
            }
            // If no intersecting points were found, segment can still be fully inside the sector without
            // intersecting its borders
            if (intersects.Count == 0)
            {
                if (start.x >= 0 && start.x < mapWidth / 3 && start.y >= 2 * mapHeight / 3 && start.y <= mapHeight)
                    if (end.x >= 0 && end.x < mapWidth / 3 && end.y >= 2 * mapHeight / 3 && end.y <= mapHeight)
                        return RealCoordinate.getDistanceBetweenCoordinates(new RealCoordinate(start.x, start.y), new RealCoordinate(end.x, end.y));
            }
            return 0;
        }

        private double getBottomMiddleDistance(Coordinate start, Coordinate end)
        {
            List<RealCoordinate> intersects = new List<RealCoordinate>();
            int sx = end.x - start.x;
            int sy = end.y - start.y;
            // 1. Check if path segment intersects with the left border of the sector
            // Left sector line border equation: x = 1/3 * mapHeight
            // Border points: [1/3*mapWidth,2/3*mapHeight],[1/3*mapWidth,mapHeight]
            double x0 = mapWidth / 3;
            double y0 = 2 * mapHeight / 3;
            double vy = mapHeight / 3;
            double t = (double)(x0 - start.x) / (double)sx;
            double s = (start.y + sy * t - y0) / vy;
            if (t <= 1 && t >= 0 && s <= 1 && s >= 0)
            {
                double intersectX = start.x + sx * t;
                double intersectY = start.y + sy * t;
                RealCoordinate intersect = new RealCoordinate(intersectX, intersectY);
                intersects.Add(intersect);
            }
            // 2. Check if path segment intersects with the top border of the sector
            // Top sector line border equation: y = 2/3 * mapWidth
            // Border points: [1/3*mapWidth,2/3*mapHeight],[2/3*mapWidth,2/3*mapHeight]
            x0 = 1 / 3 * mapWidth;
            y0 = 2 / 3 * mapHeight;
            double vx = 1 / 3 * mapWidth;
            t = (double)(y0 - start.y) / (double)sy;
            s = (start.x + sx * t - x0) / vx;
            if (t <= 1 && t >= 0 && s <= 1 && s >= 0)
            {
                double intersectX = start.x + sx * t;
                double intersectY = start.y + sy * t;
                RealCoordinate intersect = new RealCoordinate(intersectX, intersectY);
                intersects.Add(intersect);
            }
            // 3. Check if path segment intersects with the right border of the sector
            // Right sector line border equation: x = 2/3 * mapHeight
            // Border points: [2/3*mapWidth,2/3*mapHeight],[2/3*mapWidth,mapHeight]
            x0 = 2 * mapWidth / 3;
            y0 = 2 * mapHeight / 3;
            vy = mapHeight / 3;
            t = (double)(x0 - start.x) / (double)sx;
            s = (start.y + sy * t - y0) / vy;
            if (t <= 1 && t >= 0 && s <= 1 && s >= 0)
            {
                double intersectX = start.x + sx * t;
                double intersectY = start.y + sy * t;
                RealCoordinate intersect = new RealCoordinate(intersectX, intersectY);
                intersects.Add(intersect);
            }
            // If 2 intersects were found, distance between them is measured:
            if (intersects.Count == 2)
            {
                return RealCoordinate.getDistanceBetweenCoordinates(intersects[0], intersects[1]);
            }
            // If 1 intersecting point, there are 2 possibilities:
            // a) starting point is inside the sector
            // b) end point is inside the sector
            if (intersects.Count == 1)
            {
                if (start.x >= mapWidth / 3 && start.x < 2 * mapWidth / 3) 
                    if (start.y >= 2 * mapHeight / 3 && start.y <= mapHeight)
                        return RealCoordinate.getDistanceBetweenCoordinates(intersects[0], new RealCoordinate(start.x, start.y));
                if (end.x >= mapWidth / 3 && end.x < 2 * mapWidth / 3) 
                    if (end.y >= 2 * mapHeight / 3 && end.y <= mapHeight)
                        return RealCoordinate.getDistanceBetweenCoordinates(intersects[0], new RealCoordinate(end.x, end.y));
            }
            // If no intersecting points were found, segment can still be fully inside the sector without
            // intersecting its borders
            if (intersects.Count == 0)
            {
                if (start.x >= mapWidth / 3 && start.x < 2 * mapWidth / 3 && start.y >= 2 * mapHeight / 3 && start.y <= mapHeight)
                    if (end.x >= mapWidth / 3 && end.x < 2 * mapWidth / 3 && end.y >= 2 * mapHeight / 3 && end.y <= mapHeight)
                        return RealCoordinate.getDistanceBetweenCoordinates(new RealCoordinate(start.x, start.y), new RealCoordinate(end.x, end.y));
            }
            return 0;
        }

        private double getBottomRightDistance(Coordinate start, Coordinate end)
        {
            List<RealCoordinate> intersects = new List<RealCoordinate>();
            int sx = end.x - start.x;
            int sy = end.y - start.y;
            // 1. Check if path segment intersects with the left border of the sector
            // Left sector line border equation: x = 2/3 * mapHeight
            // Border points: [2/3*mapWidth,2/3*mapHeight],[2/3*mapWidth,mapHeight]
            double x0 = 2 * mapWidth / 3;
            double y0 = 2 * mapHeight / 3;
            double vy = mapHeight / 3;
            double t = (double)(x0 - start.x) / (double)sx;
            double s = (start.y + sy * t - y0) / vy;
            if (t <= 1 && t >= 0 && s <= 1 && s >= 0)
            {
                double intersectX = start.x + sx * t;
                double intersectY = start.y + sy * t;
                RealCoordinate intersect = new RealCoordinate(intersectX, intersectY);
                intersects.Add(intersect);
            }
            // 2. Check if path segment intersects with the top border of the sector
            // Top sector line border equation: y = 2/3 * mapWidth
            // Border points: [2/3*mapWidth,2/3*mapHeight],[mapWidth,2/3*mapHeight]
            x0 = 2 / 3 * mapWidth;
            y0 = 2 / 3 * mapHeight;
            double vx = 1 / 3 * mapWidth;
            t = (double)(y0 - start.y) / (double)sy;
            s = (start.x + sx * t - x0) / vx;
            if (t <= 1 && t >= 0 && s <= 1 && s >= 0)
            {
                double intersectX = start.x + sx * t;
                double intersectY = start.y + sy * t;
                RealCoordinate intersect = new RealCoordinate(intersectX, intersectY);
                intersects.Add(intersect);
            }
            // If 2 intersects were found, distance between them is measured:
            if (intersects.Count == 2)
            {
                return RealCoordinate.getDistanceBetweenCoordinates(intersects[0], intersects[1]);
            }
            // If 1 intersecting point, there are 2 possibilities:
            // a) starting point is inside the sector
            // b) end point is inside the sector
            if (intersects.Count == 1)
            {
                if (start.x >= 2 * mapWidth / 3 && start.x <= mapWidth) 
                    if (start.y >= 2 * mapHeight / 3 && start.y <= mapHeight)
                        return RealCoordinate.getDistanceBetweenCoordinates(intersects[0], new RealCoordinate(start.x, start.y));
                if (end.x >= 2 * mapWidth / 3 && end.x <= mapWidth) 
                    if (end.y >= 2 * mapHeight / 3 && end.y <= mapHeight)
                        return RealCoordinate.getDistanceBetweenCoordinates(intersects[0], new RealCoordinate(end.x, end.y));
            }
            // If no intersecting points were found, segment can still be fully inside the sector without
            // intersecting its borders
            if (intersects.Count == 0)
            {
                if (start.x >= 2 * mapWidth / 3 && start.x <= mapWidth && start.y >= 2 * mapHeight / 3 && start.y <= mapHeight)
                    if (end.x >= 2 * mapWidth / 3 && end.x <= mapWidth && end.y >= 2 * mapHeight / 3 && end.y <= mapHeight)
                        return RealCoordinate.getDistanceBetweenCoordinates(new RealCoordinate(start.x, start.y), new RealCoordinate(end.x, end.y));
            }
            return 0;
        }
    }
}
