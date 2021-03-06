﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Drawing.Imaging;

namespace SMTMotionPlanning
{
    public partial class PlanningForm : Form
    {
        class InvalidFileFormatException : Exception { }

        private List<Obstacle> obstacles;
        private List<Coordinate> path;
        // Temporary list to check fitness of paths
        private List<Coordinate[]> paths = new List<Coordinate[]>();
        private Coordinate goalLocation;
        private Pen obstaclePen;
        private Pen pathPen;
        private Brush brush;
        private Graphics graphicsObj;
        private bool pathCalculated;
        private bool runIssue;
        private bool pathDrawn;
        private Space world;
        private Agent agent;
        private int distance;
        private bool curvedPath;

        public PlanningForm()
        {
            InitializeComponent();
            obstaclePen = new Pen(Color.Red, 5);
            pathPen = new Pen(Color.Black, 2);
            brush = new SolidBrush(Color.Red);
            graphicsObj = tableLayoutPanel1.CreateGraphics();
            pathCalculated = false;
            runIssue = false;
            pathDrawn = false;
            StartPosition = FormStartPosition.Manual;
            Location = new Point(0, 0);
        }

        public PlanningForm(List<Obstacle> obstacles) : base()
        {
            this.obstacles = obstacles;
        }

        private void confirmButton_Click(object sender, EventArgs e)
        {
            int agentStartX = 0, agentStartY = 0, distance = 0;
            int agentGoalX = 0, agentGoalY = 0;
            int width = 0, length = 0;
            if (int.TryParse(startXBox.Text, out agentStartX) && int.TryParse(startYBox.Text, out agentStartY)
                && int.TryParse(distanceBox.Text, out distance) && int.TryParse(goalXBox.Text, out agentGoalX)
                && int.TryParse(goalYBox.Text, out agentGoalY))
            {
                this.distance = distance;
                agent = new Agent(new Coordinate(agentStartX, agentStartY), distance, distance);
                goalLocation = new Coordinate(agentGoalX, agentGoalY);
                if (world == null && obstacles != null)
                {
                    if (int.TryParse(widthEntryBox.Text, out width) && int.TryParse(lengthEntryBox.Text, out length))
                    {
                        world = new Space(width, length);
                        if (checkIfAgentIsInWorld() == false)
                        {
                            showParameterError();
                            return;
                        }
                        foreach (Obstacle o in obstacles)
                            world.obstacles.Add(o);
                        scaleForm();
                        graphicsObj = tableLayoutPanel1.CreateGraphics();
                        Invalidate();
                    }
                    else
                        showParameterError();
                }
                else if (obstacles != null)
                {
                    foreach (Obstacle o in obstacles)
                        world.obstacles.Add(o);
                    scaleForm();
                    graphicsObj = tableLayoutPanel1.CreateGraphics();
                    Invalidate();
                }
                else
                    showParameterError();
            }
            else
                showParameterError();
        }

        private void resetButton_Click(object sender, EventArgs e)
        {
            obstacles = null;
            distance = 0;
            agent = null;
            path = null;
            world = null;
            pathCalculated = false;
            runIssue = false;
            pathDrawn = false;
            progressLabel.Text = "";
            Invalidate();
            Refresh();
        }

        private void executeButton_Click(object sender, EventArgs e)
        {
            if (world != null && obstacles != null && agent != null)
            {
                curvedPath = curvedCheckBox.Checked;
                path = new List<Coordinate>();
                transformNonRectangularObstacles();
                progressLabel.Text = "Finding path...";
                progressLabel.Invalidate();
                int pathSegments = 0;
                for (int i = 1; i <= 20; i++)
                {
                    PathFinding finder = new PathFinding(agent.currentLocation, goalLocation, i, distance, 50000, world, curvedPath);
                    try
                    {
                        path.AddRange(finder.findPath());
                    }
                    catch (PathFinding.TestFailedException)
                    {
                        continue;
                    }
                    pathSegments = i;
                    break;
                }
                if (path.Count == 0)
                {
                    progressLabel.Text = "Path not found.";
                    showPathError("There is no clear available path to goal location.");
                }
                else
                {
                    paths.Add(path.ToArray());
                    progressLabel.Text = "Optimising...";
                    progressLabel.Invalidate();
                    Thread.Sleep(10);
                    optimisePath(pathSegments, distance);
                    if (commandsCheckBox.Checked == true)
                    {
                        PathCommandsGenerator generator = new PathCommandsGenerator(227, path);
                        generator.initialOrientation = 0;
                        generator.generateAndSaveCommands(1.35);
                    }
                    pathCalculated = true;
                    progressLabel.Text = "Path calculated.";
                }
            }
            else
                showPathError("Some parameters were not set.");
        }

        private void runButton_Click(object sender, EventArgs e)
        {
            if (pathCalculated)
            {
                runIssue = true;
                Invalidate();
            }
            else
            {
                string message = "No path calculated to display.";
                string caption = "Error displaying path";
                MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            Close();
            Environment.Exit(0);
        }

        private void worldLoader_Click(object sender, EventArgs e)
        {
            // Expected format used for world in file:
            // 2D
            // w number
            // l number
            // h number(only if 3D)
            string path = new DirectoryInfo(Environment.CurrentDirectory).Parent.Parent.Parent.FullName;
            path = Path.Combine(path, @"worldFiles");
            openFileDialog1.Filter = "Text|*.txt|All|*.*";
            openFileDialog1.InitialDirectory = path;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                StreamReader sr = new StreamReader(openFileDialog1.FileName);
                string line;
                int width = 0;
                int length = 0;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.Equals("2D"))
                        continue;
                    else if (line.ToUpper().Contains("W"))
                        width = int.Parse(line.Split()[1]);
                    else if (line.ToUpper().Contains("L"))
                        length = int.Parse(line.Split()[1]);
                    else
                        throw new IOException();
                }
                world = new Space(width, length);
            }
        }

        private void obstacleLoader_Click(object sender, EventArgs e)
        {
            // Expected format used for rectangular obstacles in file:
            // r lefttopX lefttopY width length
            // Example: o 10 10 50 20
            // Expected format used for polygonal obstacles in file:
            // p x1 y1 ... xn yn
            // Expected format used for spline obstacles in file:
            // s x1 y1 ... xn yn
            // Expected format used for elliptical obstacles in file:
            // e sx sy width length
            obstacles = new List<Obstacle>();
            string path = new DirectoryInfo(Environment.CurrentDirectory).Parent.Parent.Parent.FullName;
            path = Path.Combine(path, @"obstacleFiles");
            openFileDialog1.Filter = "Text|*.txt|All|*.*";
            openFileDialog1.InitialDirectory = path;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                StreamReader sr = new StreamReader(openFileDialog1.FileName);
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    string[] obstacleParts = line.Split(' ');
                    if (obstacleParts[0].Equals("r"))
                        obstacles.Add(new RectangularObstacle(int.Parse(obstacleParts[4]), int.Parse(obstacleParts[3]),
                            new Coordinate(int.Parse(obstacleParts[1]), int.Parse(obstacleParts[2]))));
                    if (obstacleParts[0].Equals("p"))
                    {
                        List<Coordinate> points = new List<Coordinate>();
                        for (int i = 1; i < obstacleParts.Length; i = i + 2)
                            points.Add(new Coordinate(int.Parse(obstacleParts[i]), int.Parse(obstacleParts[i + 1])));
                        obstacles.Add(new PolygonalObstacle(points));
                    }
                    if (obstacleParts[0].Equals("s"))
                    {
                        List<Coordinate> points = new List<Coordinate>();
                        for (int i = 1; i < obstacleParts.Length; i = i + 2)
                            points.Add(new Coordinate(int.Parse(obstacleParts[i]), int.Parse(obstacleParts[i + 1])));
                        obstacles.Add(new SplineObstacle(points));
                    }
                    if (obstacleParts[0].Equals("e"))
                    {
                        obstacles.Add(new EllipticalObstacle(new Coordinate(int.Parse(obstacleParts[1]),
                            int.Parse(obstacleParts[2])), int.Parse(obstacleParts[3]), int.Parse(obstacleParts[4])));
                    }
                }
                sr.Close();
            }
        }

        private void transformNonRectangularObstacles()
        {
            List<Obstacle> newObstacles = new List<Obstacle>();
            foreach (Obstacle o in world.obstacles)
            {
                if (o.type == Obstacle.ObstacleType.Spline)
                {
                    List<RectangularObstacle> discreteObstacles = ((SplineObstacle)o).transformObstacle(distance);
                    newObstacles = newObstacles.Concat(discreteObstacles).ToList();
                }
                else
                    newObstacles.Add(o);
            }

            world.obstacles = newObstacles;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (obstacles != null)
            {
                foreach (Obstacle o in obstacles)
                    drawObstacle(o);
            }
            if (agent != null)
            {
                drawStartAndGoalLocation();
            }
            if (pathCalculated && runIssue)
            {
                if (pathDrawn == false)
                    foreach (Coordinate c in path.Skip(1))
                    {
                        prettyPathDrawing(c);
                        pathDrawn = true;
                    }
                else
                    foreach (Coordinate c in path.Skip(1))
                    {
                        drawPathSegment(path.ElementAt(path.LastIndexOf(c) - 1), c);
                    }
            }
        }

        private void drawObstacle(Obstacle o)
        {
            switch (o.type)
            {
                case Obstacle.ObstacleType.Rectangle:
                    drawRectangularObstacle((RectangularObstacle)o);
                    break;
                case Obstacle.ObstacleType.Polygon:
                    drawPolygonalObstacle((PolygonalObstacle)o);
                    break;
                case Obstacle.ObstacleType.Ellipse:
                    drawEllipticalObstacle((EllipticalObstacle)o);
                    break;
                case Obstacle.ObstacleType.Spline:
                    drawSplineObstacle((SplineObstacle)o);
                    break;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            const string message =
                "Are you sure that you would like to close the form?";
            const string caption = "Form Closing";
            var result = MessageBox.Show(message, caption,
                                         MessageBoxButtons.YesNo,
                                         MessageBoxIcon.Question);

            // If the no button was pressed ...
            if (result == DialogResult.No)
            {
                // cancel the closure of the form.
                e.Cancel = true;
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void drawRectangularObstacle(RectangularObstacle o)
        {
            Coordinate relLoc = calculateRelativeCanvasPosition(o.location);
            Rectangle rectangle = new Rectangle(relLoc.x, relLoc.y, o.width, o.length);
            graphicsObj.DrawRectangle(obstaclePen, rectangle);
            graphicsObj.FillRectangle(brush, rectangle);
        }

        private void drawPolygonalObstacle(PolygonalObstacle o)
        {
            Coordinate[] relLocs = new Coordinate[o.points.Count];
            for (int i = 0; i < o.points.Count; i++)
            {
                Coordinate c = calculateRelativeCanvasPosition(o.points[i]);
                relLocs[i] = c;
            }
            graphicsObj.DrawPolygon(obstaclePen, relLocs.Select(item => new Point(item.x, item.y)).ToArray());
        }

        private void drawEllipticalObstacle(EllipticalObstacle o)
        {
            Coordinate relLoc = calculateRelativeCanvasPosition(new Coordinate(o.location.x - o.width / 2,
                o.location.y - o.length / 2));
            Rectangle rectangle = new Rectangle(relLoc.x, relLoc.y, o.width, o.length);
            graphicsObj.DrawEllipse(obstaclePen, rectangle);
            graphicsObj.FillEllipse(brush, rectangle);
        }

        private void drawSplineObstacle(SplineObstacle o)
        {
            Coordinate[] relLocs = new Coordinate[o.points.Count];
            for (int i = 0; i < o.points.Count; i++)
            {
                Coordinate c = calculateRelativeCanvasPosition(o.points[i]);
                relLocs[i] = c;
            }
            graphicsObj.DrawCurve(obstaclePen, relLocs.Select(item => new Point(item.x, item.y)).ToArray());
        }

        private void drawPathSegment(Coordinate c1, Coordinate c2)
        {
            Coordinate relativePredecessorLocation = calculateRelativeCanvasPosition(c1);
            Coordinate relativeSuccessorLocation = calculateRelativeCanvasPosition(c2);
            graphicsObj.DrawLine(pathPen, relativePredecessorLocation.x, relativePredecessorLocation.y,
                relativeSuccessorLocation.x, relativeSuccessorLocation.y);
        }

        private void drawPathSegment(RealCoordinate c1, RealCoordinate c2)
        {
            RealCoordinate relativePredecessorLocation = calculateRelativeCanvasPosition(c1);
            RealCoordinate relativeSuccessorLocation = calculateRelativeCanvasPosition(c2);
            graphicsObj.DrawLine(pathPen, (float)relativePredecessorLocation.x, (float)relativePredecessorLocation.y,
                (float)relativeSuccessorLocation.x, (float)relativeSuccessorLocation.y);
        }

        private void prettyPathDrawing(Coordinate c)
        {
            Coordinate predecessor = path[path.IndexOf(c) - 1];
            Coordinate relativePredecessorLocation = calculateRelativeCanvasPosition(predecessor);
            Coordinate relativeSuccessorLocation = calculateRelativeCanvasPosition(c);
            int xDistance = Coordinate.getXDistanceBetweenCoordinates(c, predecessor);
            int yDistance = Coordinate.getYDistanceBetweenCoordinates(c, predecessor);
            if (xDistance != 0 && yDistance == 0)
            {
                int xChange = predecessor.x < c.x ? 1 : -1;
                Coordinate[] segments = new Coordinate[xDistance + 1];
                segments[0] = predecessor;
                segments[xDistance - 1] = c;
                for (int i = 1; i < xDistance; i++)
                {
                    segments[i] = new Coordinate(new int[] { segments[i - 1].x + xChange, segments[i - 1].y });
                    drawPathSegment(segments[i - 1], segments[i]);
                    Thread.Sleep((int)Math.Ceiling((double)(1000 / xDistance)));
                }
            }
            if (yDistance != 0 && xDistance == 0)
            {
                int yChange = predecessor.y < c.y ? 1 : -1;
                Coordinate[] segments = new Coordinate[yDistance + 1];
                segments[0] = predecessor;
                segments[yDistance - 1] = c;
                for (int i = 1; i < yDistance; i++)
                {
                    segments[i] = new Coordinate(new int[] { segments[i - 1].x, segments[i - 1].y + yChange });
                    drawPathSegment(segments[i - 1], segments[i]);
                    Thread.Sleep((int)Math.Ceiling((double)(1000 / yDistance)));
                }
            }
            if (yDistance != 0 && xDistance != 0)
            {
                double k = (double)(c.y - predecessor.y) / (double)(c.x - predecessor.x);
                double q = predecessor.y - k * predecessor.x;
                if (yDistance > xDistance)
                {
                    int yChange = predecessor.y < c.y ? 1 : -1;
                    RealCoordinate[] segments = new RealCoordinate[yDistance + 1];
                    segments[0] = new RealCoordinate(predecessor.x, predecessor.y);
                    for (int i = 1; i < yDistance; i++)
                    {
                        float currentY = (float)segments[i - 1].y + yChange;
                        float currentX = (float)((currentY - q) / k);
                        segments[i] = new RealCoordinate(currentX, currentY);
                        drawPathSegment(segments[i - 1], segments[i]);
                        Thread.Sleep((int)Math.Ceiling((double)(1000 / yDistance)));
                    }
                }
                if (xDistance > yDistance)
                {
                    int xChange = predecessor.x < c.x ? 1 : -1;
                    RealCoordinate[] segments = new RealCoordinate[xDistance + 1];
                    segments[0] = new RealCoordinate(predecessor.x, predecessor.y);
                    for (int i = 1; i < xDistance; i++)
                    {
                        float currentX = (float)segments[i - 1].x + xChange;
                        float currentY = (float)(k * currentX + q);
                        segments[i] = new RealCoordinate(currentX, currentY);
                        drawPathSegment(segments[i - 1], segments[i]);
                        Thread.Sleep((int)Math.Ceiling((double)(1000 / xDistance)));
                    }
                }
            }
        }

        private void drawStartAndGoalLocation()
        {
            Coordinate relativeStartLocation = calculateRelativeCanvasPosition(agent.currentLocation);
            Coordinate relativeGoalLocation = calculateRelativeCanvasPosition(goalLocation);
            Rectangle start = new Rectangle(relativeStartLocation.x - distance / 2, relativeStartLocation.y - distance / 2,
                distance, distance);
            Rectangle finish = new Rectangle(relativeGoalLocation.x - distance / 2, relativeGoalLocation.y - distance / 2,
                distance, distance);
            Brush finishBrush = new SolidBrush(Color.Blue);
            Pen finishPen = new Pen(Color.Green, 5);

            graphicsObj.DrawEllipse(obstaclePen, start);
            graphicsObj.FillEllipse(brush, start);
            graphicsObj.DrawEllipse(finishPen, finish);
            graphicsObj.FillEllipse(finishBrush, finish);
        }

        private Coordinate calculateRelativeCanvasPosition(Coordinate location)
        {
            // 0 - 90% of the actual length of form is the canvas for drawing
            // 25 - 100% of the actual width of form is the canvas for drawing
            // Purpose of this method is to calculate the position of the object in relation to these proportions
            // The actual [0,0] point is not going to be left-top corner of the form, but the left-top corner
            // of the canvas itself, leaving out parts of the form where the controls are

            double relativeX = location.x + 160;
            int relativeY = location.y;

            return new Coordinate((int)relativeX, relativeY);
        }

        private RealCoordinate calculateRelativeCanvasPosition(RealCoordinate location)
        {
            double relativeX = location.x + 160;
            double relativeY = location.y;

            return new RealCoordinate(relativeX, relativeY);
        }

        private void scaleForm()
        {
            // 16 is the fixed width of form borders
            // 38 is the fixed height of form borders
            double newWidth = world.width + 160 + 16;
            double newLength = world.length + 45 + 38;

            TableLayoutRowStyleCollection rowStyles;
            TableLayoutColumnStyleCollection columnStyles;
            columnStyles = tableLayoutPanel1.ColumnStyles;
            rowStyles = tableLayoutPanel1.RowStyles;
            columnStyles[0].SizeType = SizeType.Absolute;
            columnStyles[0].Width = 160;
            columnStyles[1].SizeType = SizeType.Absolute;
            columnStyles[1].Width = world.width;
            rowStyles[0].SizeType = SizeType.Absolute;
            rowStyles[0].Height = world.length;
            rowStyles[1].SizeType = SizeType.Absolute;
            rowStyles[1].Height = 45;

            if (newWidth != Width || newWidth != Height)
            {
                Width = (int)Math.Ceiling(newWidth);
                Height = (int)Math.Ceiling(newLength);
            }
        }

        private void showParameterError()
        {
            string message = "Not all parameters were properly set.";
            string caption = "Error setting parameters";
            MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void showPathError(string message)
        {
            string caption = "Error calculating path";
            MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void optimisePath(int pathSegments, int obstaclePassDistance)
        {
            PathFinding finder = new PathFinding(agent.currentLocation, goalLocation, pathSegments + 1, obstaclePassDistance, 50000, world, curvedPath);
            Coordinate[] longerPathOne = null;
            try
            {
                longerPathOne = finder.findPath();
                paths.Add(longerPathOne);
            }
            catch (PathFinding.TestFailedException)
            { }

            finder = new PathFinding(agent.currentLocation, goalLocation, pathSegments + 2, obstaclePassDistance, 50000, world, curvedPath);
            Coordinate[] longerPathTwo = null;
            try
            {
                longerPathTwo = finder.findPath();
                paths.Add(longerPathTwo);
            }
            catch (PathFinding.TestFailedException)
            { }

            optimisePathLength(pathSegments, path);
            if (longerPathOne != null)
                optimisePathLength(longerPathOne.Length - 1, longerPathOne.ToList());
            if (longerPathTwo != null)
                optimisePathLength(longerPathTwo.Length - 1, longerPathTwo.ToList());
        }

        private void optimisePathLength(int pathSegments, List<Coordinate> path)
        {
            int pathLength = 0;
            for (int i = 0; i < path.Count - 1; i++)
            {
                pathLength += Coordinate.getXDistanceBetweenCoordinates(path[i], path[i + 1]) +
                    Coordinate.getYDistanceBetweenCoordinates(path[i], path[i + 1]);
            }
            double bestFitness = evaluateFitnessValueOfPath(this.path.ToArray());

            int decrementor = pathLength / 25;
            bool hasFailed = false;
            while (decrementor > 0)
            {
                pathLength -= decrementor;
                PathFinding finder = new PathFinding(agent.currentLocation, goalLocation, pathSegments,
                    distance, pathLength, world, curvedPath);
                try
                {
                    Coordinate[] newPath = null;
                    Exception exception = null;
                    Thread optimiser = new Thread(() => SafeExecute(() => newPath = finder.findPath(), out exception));
                    optimiser.Start();
                    bool success = optimiser.Join(750);
                    if (exception != null)
                    {
                        if (hasFailed)
                            break;
                        else
                        {
                            hasFailed = true;
                            continue;
                        }
                    }
                    if (success == true)
                    {
                        paths.Add(newPath);
                        double newPathFitness = evaluateFitnessValueOfPath(newPath);
                        if (bestFitness > newPathFitness)
                        {
                            this.path = new List<Coordinate>();
                            this.path.AddRange(newPath);
                            bestFitness = evaluateFitnessValueOfPath(newPath);
                        }
                    }
                    else
                    {
                        if (hasFailed)
                            break;
                        else
                        {
                            hasFailed = true;
                            continue;
                        }
                    }
                }
                catch (PathFinding.TestFailedException)
                {
                    break;
                }
            }
        }

        private void SafeExecute(Action test, out Exception exception)
        {
            exception = null;
            try
            {
                test.Invoke();
            }
            catch (Exception ex)
            {
                exception = ex;
            }
        }

        private bool checkIfAgentIsInWorld()
        {
            if (agent.currentLocation.x < 0 || agent.currentLocation.x > world.width)
                return false;
            if (agent.currentLocation.y < 0 || agent.currentLocation.y > world.length)
                return false;
            return true;
        }

        private double evaluateFitnessValueOfPath(Coordinate[] path)
        {
            double fitness = 0;

            // Sharpness of turns
            // 90 degree turn... 150
            // 0 degree turn... 0
            // linear dependence
            if (path.Length > 2)
            {
                for (int i = 0; i < path.Length - 2; i++)
                {
                    double k1 = (double)(path[i + 1].y - path[i].y) / (double)(path[i + 1].x - path[i].x);
                    double q1 = path[i].y - path[i].x * k1;
                    double k2 = (double)(path[i + 2].y - path[i + 1].y) / (double)(path[i + 2].x - path[i + 1].x);
                    double q2 = path[i + 1].y - path[i + 1].x * k1;
                    LineSegment first = new LineSegment(k1, q1, path[i], path[i + 1]);
                    LineSegment second = new LineSegment(k2, q2, path[i + 1], path[i + 2]);
                    double angle = evaluateAngleBetweenTwoLineSegments(first, second);
                    fitness += 15.0 / 9.0 * angle;
                }
            }

            // Path length
            double airDistance = Math.Sqrt(Math.Pow(Coordinate.getXDistanceBetweenCoordinates(path[0], path[path.Length - 1]), 2) +
                Math.Pow(Coordinate.getYDistanceBetweenCoordinates(path[0], path[path.Length - 1]), 2));
            fitness += evaluatePathLength(path) / airDistance * 100;

            return fitness;
        }

        private double evaluateAngleBetweenTwoLineSegments(LineSegment first, LineSegment second)
        {
            int firstSx = first.end.x - first.start.x;
            int firstSy = first.end.y - first.start.y;
            int secondSx = second.end.x - second.start.x;
            int secondSy = second.end.y - second.start.y;

            int dotProduct = firstSx * secondSx + firstSy * secondSy;
            double firstLength = Math.Sqrt(firstSx * firstSx + firstSy * firstSy);
            double secondLength = Math.Sqrt(secondSx * secondSx + secondSy * secondSy);

            double angle = Math.Acos(dotProduct / (firstLength * secondLength));

            return angle * 180 / Math.PI;
        }

        private double evaluatePathLength(Coordinate[] path)
        {
            double pathLength = 0;

            for (int i = 0; i < path.Length - 2; i++)
            {
                pathLength += Math.Sqrt(Math.Pow(Coordinate.getXDistanceBetweenCoordinates(path[i], path[i + 1]), 2) +
                Math.Pow(Coordinate.getYDistanceBetweenCoordinates(path[i], path[i + 1]), 2));
            }

            return pathLength;
        }

        private void captureButton_Click(object sender, EventArgs e)
        {
            SegmentationForm segForm = new SegmentationForm();
            segForm.Show();
        }

        private void drawButton_Click(object sender, EventArgs e)
        {
            if (this.path == null)
            {
                MessageBox.Show("Path needs to be calculated before it can be drown on a selected picture.", "Path error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                string path = new DirectoryInfo(Environment.CurrentDirectory).Parent.Parent.Parent.FullName;
                path = Path.Combine(path, @"Pictures");
                openFileDialog1.Filter = "Png Images(*.png)|*.png|Jpeg Images(*.jpg)|*.jpg|Bitmaps(*.bmp)|*.bmp";
                openFileDialog1.InitialDirectory = path;
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    Bitmap image = new Bitmap(openFileDialog1.FileName);
                    Graphics imageGraphics = Graphics.FromImage(image);
                    Pen imagePen = new Pen(Color.Red, 4);
                    if (world.width != image.Width || world.length != image.Height)
                    {
                        List<Coordinate> rescaledPath = new List<Coordinate>();
                        foreach (Coordinate c in this.path)
                        {
                            Coordinate rescaled = new Coordinate(c.x * image.Width / world.width, c.y * image.Height / world.length);
                            rescaledPath.Add(rescaled);
                        }
                        for (int i = 0; i < rescaledPath.Count - 1; i++)
                        {
                            imageGraphics.DrawLine(imagePen, rescaledPath[i].x, rescaledPath[i].y, rescaledPath[i + 1].x, rescaledPath[i + 1].y);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < this.path.Count - 1; i++)
                        {
                            imageGraphics.DrawLine(imagePen, this.path[i].x, this.path[i].y, this.path[i + 1].x, this.path[i + 1].y);
                        }
                    }
                    Directory.SetCurrentDirectory(path);
                    image.Save("imagePath.png", ImageFormat.Png);
                }
            }
        }
    }
}

