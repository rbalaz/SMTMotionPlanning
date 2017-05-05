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
        List<string> QboCommands;
        List<string> NaoCommands;
        List<Coordinate> path;
        public int mapWidth;
        public int mapHeight;

        public PathCommandsGenerator(int usedCameraId, List<Coordinate> path)
        {
            this.usedCameraId = usedCameraId;
            this.path = path;
            mapHeight = 800;
            mapWidth = 1280;
            QboCommands = new List<string>();
            NaoCommands = new List<string>();
        }

        private void rescalePathToOriginalDimensions()
        {
            List<Coordinate> rescaledPath = new List<Coordinate>();
            foreach (Coordinate coord in path)
            {
                int newX = (int)(coord.x * 1280.0 / 800.0);
                int newY = (int)(coord.y * 800.0 / 600.0);
                rescaledPath.Add(new Coordinate(newX, newY));
            }
            path = rescaledPath;
        }

        public void generateAndSaveCommands()
        {
            rescalePathToOriginalDimensions();
            FileStream stream = new FileStream("commands.txt", FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter writer = new StreamWriter(stream);
            double currentOrientation = initialOrientation;
            for (int i = 0; i < path.Count - 1; i++)
            {
                double orientationChange = changeOrientation(path[i], path[i + 1], currentOrientation);
                if (orientationChange > 3.14)
                    orientationChange -= 2 * Math.PI;
                if (orientationChange < -3.14)
                    orientationChange += 2 * Math.PI;
                writer.WriteLine("Rotate: " + orientationChange);
                currentOrientation += orientationChange;
                double length;
                if (usedCameraId == 227)
                    length = calculateSegmentLength227(path[i], path[i + 1]);
                else
                    length = calculateSegmentLength219(path[i], path[i + 1]);
                writer.WriteLine("Move forward: " + length);
                generateQboCommand(orientationChange, length);
                generateNaoCommand(orientationChange, length);
            }
            writer.Close();
            stream.Close();

            stream = new FileStream("Qbo_commands.txt", FileMode.Create, FileAccess.Write);
            writer = new StreamWriter(stream);
            foreach (string line in QboCommands)
            {
                writer.Write("rostopic pub -1 /cmd_vel geometry_msgs/Twist");
                writer.Write(" " + line);
            }
            writer.Close();
            stream.Close();

            stream = new FileStream("Nao_commands.txt", FileMode.Create, FileAccess.Write);
            writer = new StreamWriter(stream);
            foreach (string line in NaoCommands)
            {
                writer.Write("motionProxy.moveTo");
                writer.WriteLine(line.Replace("[", "(").Replace("]", ")"));
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

        private double calculateSegmentLength227(Coordinate start, Coordinate end)
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

        private double calculateSegmentLength219(Coordinate start, Coordinate end)
        {
            /*           219
                ----------------------
                | 0.29 | 0.26 | 0.28 |
                | 0.28 | 0.26 | 0.29 |
                | 0.29 | 0.26 | 0.29 |
                ----------------------
              */
            double distance = 0;
            distance += getTopLeftDistance(start, end) * 0.29;
            distance += getTopMiddleDistance(start, end) * 0.26;
            distance += getTopRightDistance(start, end) * 0.28;
            distance += getMiddleLeftDistance(start, end) * 0.28;
            distance += getMiddleMiddleDistance(start, end) * 0.26;
            distance += getMiddleRightDistance(start, end) * 0.29;
            distance += getBottomLeftDistance(start, end) * 0.29;
            distance += getBottomMiddleDistance(start, end) * 0.26;
            distance += getBottomRightDistance(start, end) * 0.29;
            return distance;
        }

        private double getTopLeftDistance(Coordinate start, Coordinate end)
        {
            List<RealCoordinate> intersects = new List<RealCoordinate>();
            // Segment line equation:
            // x = x0 + sx*t
            // y = y0 + sy*t
            int sx = end.x - start.x;
            int sy = end.y - start.y;
            // 1. Check if path segment intersects with the bottom border of the sector
            // Bottom sector line border equation: y = 1/3 * mapHeight
            // Border points: [0,1/3*mapHeight],[1/3*mapWidth,1/3*mapHeight]
            double y0 = mapHeight / 3.0;
            double vx = mapWidth / 3.0;
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
            double x0 = mapWidth / 3.0;
            double vy = mapHeight / 3.0;
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
                if (start.x >= 0 && start.x < mapWidth / 3.0)
                    if (start.y >= 0 && start.y < mapHeight / 3.0)
                        return RealCoordinate.getDistanceBetweenCoordinates(intersects[0], new RealCoordinate(start.x, start.y));
                if (end.x >= 0 && end.x < mapWidth / 3.0)
                    if (end.y >= 0 && end.y < mapHeight / 3.0)
                        return RealCoordinate.getDistanceBetweenCoordinates(intersects[0], new RealCoordinate(end.x, end.y));
            }
            // If no intersecting points were found, segment can still be fully inside the sector without
            // intersecting its borders
            if (intersects.Count == 0)
            {
                if (start.x >= 0 && start.x < mapWidth / 3.0 && start.y >= 0 && start.y < mapHeight / 3.0)
                    if (end.x >= 0 && end.x < mapWidth / 3.0 && end.y >= 0 && end.y < mapHeight / 3.0)
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
            // Border points: [1/3*mapWidth,0],[1/3*mapWidth,1/3*mapHeight]
            double x0 = mapWidth / 3.0;
            double vy = mapHeight / 3.0;
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
            x0 = 1.0 / 3.0 * mapWidth;
            double y0 = 1.0 / 3.0 * mapHeight;
            double vx = 1.0 / 3.0 * mapWidth;
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
            x0 = 2.0 / 3.0 * mapWidth;
            vy = 1.0 / 3.0 * mapHeight;
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
                if (start.x >= mapWidth / 3.0 && start.x < 2.0 * mapWidth / 3.0)
                    if (start.y >= 0 && start.y < mapHeight / 3.0)
                        return RealCoordinate.getDistanceBetweenCoordinates(intersects[0], new RealCoordinate(start.x, start.y));
                if (end.x >= mapWidth / 3.0 && end.x < 2.0 * mapWidth / 3.0)
                    if (end.y >= 0 && end.y < mapHeight / 3.0)
                        return RealCoordinate.getDistanceBetweenCoordinates(intersects[0], new RealCoordinate(end.x, end.y));
            }
            // If no intersecting points were found, segment can still be fully inside the sector without
            // intersecting its borders
            if (intersects.Count == 0)
            {
                if (start.x >= mapWidth / 3.0 && start.x < 2.0 * mapWidth / 3.0 && start.y >= 0 && start.y < mapHeight / 3.0)
                    if (end.x >= mapWidth / 3.0 && end.x < 2.0 * mapWidth / 3.0 && end.y >= 0 && end.y < mapHeight / 3.0)
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
            double x0 = 2.0 / 3.0 * mapWidth;
            double vy = 1.0 / 3.0 * mapHeight;
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
            x0 = 2.0 / 3.0 * mapWidth;
            double y0 = 1.0 / 3.0 * mapHeight;
            double vx = 1.0 / 3.0 * mapWidth;
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
                if (start.x >= 2.0 * mapWidth / 3.0 && start.x <= mapWidth)
                    if (start.y >= 0 && start.y < mapHeight / 3.0) 
                        return RealCoordinate.getDistanceBetweenCoordinates(intersects[0], new RealCoordinate(start.x, start.y));
                if (end.x >= 2.0 * mapWidth / 3.0 && end.x <= mapWidth) 
                    if (end.y >= 0 && end.y < mapHeight / 3.0)
                        return RealCoordinate.getDistanceBetweenCoordinates(intersects[0], new RealCoordinate(end.x, end.y));
            }
            // If no intersecting points were found, segment can still be fully inside the sector without
            // intersecting its borders
            if (intersects.Count == 0)
            {
                if (start.x >= 2.0 * mapWidth / 3.0 && start.x <= mapWidth && start.y >= 0 && start.y < mapHeight / 3.0)
                    if (end.x >= 2.0 * mapWidth / 3.0 && end.x <= mapWidth && end.y >= 0 && end.y < mapHeight / 3.0)
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
            // Top sector line border equation: y = 1/3 * mapHeight
            // Border points: [0,1/3*mapHeight],[1/3*mapWidth,1/3*mapHeight]
            double y0 = mapHeight / 3.0;
            double vx = mapWidth / 3.0;
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
            double x0 = mapWidth / 3.0;
            y0 = mapHeight / 3.0;
            double vy = mapHeight / 3.0;
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
            // Bottom sector line border equation: y = 2/3 * mapHeight
            // Border points: [0,2/3*mapHeight],[1/3*mapWidth,2/3*mapHeight]
            y0 = 2.0 * mapHeight / 3.0;
            vx = mapWidth / 3.0;
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
                if (start.x >= 0 && start.x < mapWidth / 3.0)
                    if (start.y >= mapHeight / 3.0 && start.y < 2.0 * mapHeight / 3.0) 
                        return RealCoordinate.getDistanceBetweenCoordinates(intersects[0], new RealCoordinate(start.x, start.y));
                if (end.x >= 0 && end.x < mapWidth / 3.0)
                    if (end.y >= mapHeight / 3.0 && end.y < 2.0 * mapHeight / 3.0) 
                        return RealCoordinate.getDistanceBetweenCoordinates(intersects[0], new RealCoordinate(end.x, end.y));
            }
            // If no intersecting points were found, segment can still be fully inside the sector without
            // intersecting its borders
            if (intersects.Count == 0)
            {
                if (start.x >= 0 && start.x < mapWidth / 3.0 && start.y >= mapHeight / 3.0 && start.y < 2.0 * mapHeight / 3.0)
                    if (end.x >= 0 && end.x < mapWidth / 3.0 && end.y >= mapHeight / 3.0 && end.y < 2.0 * mapHeight / 3.0)
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
            double x0 = mapWidth / 3.0;
            double y0 = mapHeight / 3.0;
            double vy = mapHeight / 3.0;
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
            x0 = 1.0 / 3.0 * mapWidth;
            y0 = 1.0 / 3.0 * mapHeight;
            double vx = 1.0 / 3.0 * mapWidth;
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
            x0 = 1.0 / 3.0 * mapWidth;
            y0 = 2.0 / 3.0 * mapHeight;
            vx = 1.0 / 3.0 * mapWidth;
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
            x0 = 2.0 * mapWidth / 3.0;
            y0 = mapHeight / 3.0;
            vy = mapHeight / 3.0;
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
                if (start.x >= mapWidth / 3.0 && start.x < 2.0 * mapWidth / 3.0)
                    if (start.y >= mapHeight / 3.0 && start.y < 2.0 * mapHeight / 3.0) 
                        return RealCoordinate.getDistanceBetweenCoordinates(intersects[0], new RealCoordinate(start.x, start.y));
                if (end.x >= mapWidth / 3.0 && end.x < 2.0 * mapWidth / 3.0)
                    if (end.y >= mapHeight / 3.0 && end.y < 2.0 * mapHeight / 3.0) 
                        return RealCoordinate.getDistanceBetweenCoordinates(intersects[0], new RealCoordinate(end.x, end.y));
            }
            // If no intersecting points were found, segment can still be fully inside the sector without
            // intersecting its borders
            if (intersects.Count == 0)
            {
                if (start.x >= mapWidth / 3.0 && start.x < 2.0 * mapWidth / 3.0 && start.y >= mapHeight / 3.0 && start.y < 2.0 * mapHeight / 3.0)
                    if (end.x >= mapWidth / 3.0 && end.x < 2.0 * mapWidth / 3.0 && end.y >= mapHeight / 3.0 && end.y < 2.0 * mapHeight / 3.0)
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
            double x0 = 2.0 * mapWidth / 3.0;
            double y0 = mapHeight / 3.0;
            double vy = mapHeight / 3.0;
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
            x0 = 2.0 / 3.0 * mapWidth;
            y0 = 1.0 / 3.0 * mapHeight;
            double vx = 1.0 / 3.0 * mapWidth;
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
            x0 = 2.0 / 3.0 * mapWidth;
            y0 = 2.0 / 3.0 * mapHeight;
            vx = 1.0 / 3.0 * mapWidth;
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
                if (start.x >= 2.0 * mapWidth / 3.0 && start.x <= mapWidth) 
                    if (start.y >= mapHeight / 3.0 && start.y < 2.0 * mapHeight / 3.0)
                        return RealCoordinate.getDistanceBetweenCoordinates(intersects[0], new RealCoordinate(start.x, start.y));
                if (end.x >= 2.0 * mapWidth / 3.0 && end.x <= mapWidth) 
                    if (end.y >= mapHeight / 3.0 && end.y < 2.0 * mapHeight / 3.0) 
                return RealCoordinate.getDistanceBetweenCoordinates(intersects[0], new RealCoordinate(end.x, end.y));
            }
            // If no intersecting points were found, segment can still be fully inside the sector without
            // intersecting its borders
            if (intersects.Count == 0)
            {
                if (start.x >= 2.0 * mapWidth / 3.0 && start.x <= mapWidth && start.y >= mapHeight / 3.0 && start.y < 2.0 * mapHeight / 3.0)
                    if (end.x >= 2.0 * mapWidth / 3.0 && end.x <= mapWidth && end.y >= mapHeight / 3.0 && end.y < 2.0 * mapHeight / 3.0)
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
            double y0 = 2.0 * mapHeight / 3.0;
            double vx = mapWidth / 3.0;
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
            double x0 = mapWidth / 3.0;
            y0 = 2.0 * mapHeight / 3.0;
            double vy = mapHeight / 3.0;
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
                if (start.x >= 0 && start.x < mapWidth / 3.0)
                    if (start.y >= 2.0 * mapHeight / 3.0 && start.y <= mapHeight) 
                        return RealCoordinate.getDistanceBetweenCoordinates(intersects[0], new RealCoordinate(start.x, start.y));
                if (end.x >= 0 && end.x < mapWidth / 3)
                    if (end.y >= 2.0 * mapHeight / 3.0 && end.y <= mapHeight) 
                        return RealCoordinate.getDistanceBetweenCoordinates(intersects[0], new RealCoordinate(end.x, end.y));
            }
            // If no intersecting points were found, segment can still be fully inside the sector without
            // intersecting its borders
            if (intersects.Count == 0)
            {
                if (start.x >= 0 && start.x < mapWidth / 3.0 && start.y >= 2.0 * mapHeight / 3.0 && start.y <= mapHeight)
                    if (end.x >= 0 && end.x < mapWidth / 3.0 && end.y >= 2.0 * mapHeight / 3.0 && end.y <= mapHeight)
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
            // Left sector line border equation: x = 1/3 * mapWidth
            // Border points: [1/3*mapWidth,2/3*mapHeight],[1/3*mapWidth,mapHeight]
            double x0 = mapWidth / 3.0;
            double y0 = 2.0 * mapHeight / 3.0;
            double vy = mapHeight / 3.0;
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
            // Top sector line border equation: y = 2/3 * mapHeight
            // Border points: [1/3*mapWidth,2/3*mapHeight],[2/3*mapWidth,2/3*mapHeight]
            x0 = 1.0 / 3.0 * mapWidth;
            y0 = 2.0 / 3.0 * mapHeight;
            double vx = 1.0 / 3.0 * mapWidth;
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
            x0 = 2.0 * mapWidth / 3.0;
            y0 = 2.0 * mapHeight / 3.0;
            vy = mapHeight / 3.0;
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
                if (start.x >= mapWidth / 3.0 && start.x < 2.0 * mapWidth / 3.0) 
                    if (start.y >= 2.0 * mapHeight / 3.0 && start.y <= mapHeight)
                        return RealCoordinate.getDistanceBetweenCoordinates(intersects[0], new RealCoordinate(start.x, start.y));
                if (end.x >= mapWidth / 3.0 && end.x < 2.0 * mapWidth / 3.0) 
                    if (end.y >= 2.0 * mapHeight / 3.0 && end.y <= mapHeight)
                        return RealCoordinate.getDistanceBetweenCoordinates(intersects[0], new RealCoordinate(end.x, end.y));
            }
            // If no intersecting points were found, segment can still be fully inside the sector without
            // intersecting its borders
            if (intersects.Count == 0)
            {
                if (start.x >= mapWidth / 3.0 && start.x < 2.0 * mapWidth / 3.0 && start.y >= 2.0 * mapHeight / 3.0 && start.y <= mapHeight)
                    if (end.x >= mapWidth / 3.0 && end.x < 2.0 * mapWidth / 3.0 && end.y >= 2.0 * mapHeight / 3.0 && end.y <= mapHeight)
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
            // Left sector line border equation: x = 2/3 * mapWidth
            // Border points: [2/3*mapWidth,2/3*mapHeight],[2/3*mapWidth,mapHeight]
            double x0 = 2.0 * mapWidth / 3.0;
            double y0 = 2.0 * mapHeight / 3.0;
            double vy = mapHeight / 3.0;
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
            // Top sector line border equation: y = 2/3 * mapHeight
            // Border points: [2/3*mapWidth,2/3*mapHeight],[mapWidth,2/3*mapHeight]
            x0 = 2.0 / 3.0 * mapWidth;
            y0 = 2.0 / 3.0 * mapHeight;
            double vx = 1.0 / 3.0 * mapWidth;
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
                if (start.x >= 2.0 * mapWidth / 3.0 && start.x <= mapWidth) 
                    if (start.y >= 2.0 * mapHeight / 3.0 && start.y <= mapHeight)
                        return RealCoordinate.getDistanceBetweenCoordinates(intersects[0], new RealCoordinate(start.x, start.y));
                if (end.x >= 2.0 * mapWidth / 3.0 && end.x <= mapWidth) 
                    if (end.y >= 2.0 * mapHeight / 3.0 && end.y <= mapHeight)
                        return RealCoordinate.getDistanceBetweenCoordinates(intersects[0], new RealCoordinate(end.x, end.y));
            }
            // If no intersecting points were found, segment can still be fully inside the sector without
            // intersecting its borders
            if (intersects.Count == 0)
            {
                if (start.x >= 2.0 * mapWidth / 3.0 && start.x <= mapWidth && start.y >= 2.0 * mapHeight / 3.0 && start.y <= mapHeight)
                    if (end.x >= 2.0 * mapWidth / 3.0 && end.x <= mapWidth && end.y >= 2.0 * mapHeight / 3.0 && end.y <= mapHeight)
                        return RealCoordinate.getDistanceBetweenCoordinates(new RealCoordinate(start.x, start.y), new RealCoordinate(end.x, end.y));
            }
            return 0;
        }

        private void generateQboCommand(double orientationChange, double distance)
        {
            // rostopic pub -1 /cmd_vel geometry_msgs/Twist [1,0,0] [0,0,0]
            // 1    ... 45 counter-clockwise
            // 1/45 ...  1 counter-clockwise
            string roundedOrientationChange = string.Format("{0:F2}", 4 * orientationChange / Math.PI);
            QboCommands.Add("[0,0,0] [0,0," + roundedOrientationChange.Replace(",", ".") + "]");
            // distance command
            string roundedDistance = string.Format("{0:F2}", distance / 52.0);
            string movementDirectionCorrection = string.Format("{0:F2}", distance / 104.0);
            // 1    ... 52cm
            // 1/52 ...  1cm
            QboCommands.Add("[" + roundedDistance.Replace(",", ".") + ",0,0] [0,0," + movementDirectionCorrection + "]");
        }

        private void generateNaoCommand(double orientationChange, double distance)
        {
            // motion.moveTo(0, 0, 3.14)
            // orientation change
            string roundedOrientationChange = string.Format("{0:F2}", orientationChange);
            NaoCommands.Add("[0,0," + roundedOrientationChange.Replace(",", ".") + "]");
            // Nao 9 moves approximately 22.5% more than he should
            string roundedDistance = string.Format("{0:F2}", distance / 122.5);
            string movementDirectionCorrection = "0.03";
            NaoCommands.Add("[" + roundedDistance.Replace(",", ".") + "," + movementDirectionCorrection + ",0]");
        }
    }
}
