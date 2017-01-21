using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace SMTMotionPlanning
{
    public partial class Form1 : Form
    {
        class InvalidFileFormatException : Exception { }

        private List<Obstacle> obstacles;
        private List<Coordinate> path;
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

        public Form1()
        {
            InitializeComponent();
            obstaclePen = new Pen(Color.Red, 5);
            pathPen = new Pen(Color.Black, 2);
            brush = new SolidBrush(Color.Red);
            graphicsObj = tableLayoutPanel1.CreateGraphics();
            pathCalculated = false;
            runIssue = false;
            pathDrawn = false;
        }

        public Form1(List<Obstacle> obstacles) : base()
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
                        world = new Space2D(width, length);
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
                for (int i = 1; i <= 100; i++)
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
                    optimisePath(i);
                    break;
                }
                if (path.Count == 0)
                    showPathError("There is no clear available path to goal location.");
                else
                    pathCalculated = true;
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
        }

        private void worldLoader_Click(object sender, EventArgs e)
        {
            // Expected format used for world in file:
            // 2D or 3D 
            // w number
            // l number
            // h number(only if 3D)
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                StreamReader sr = new StreamReader(openFileDialog1.FileName);
                string line;
                bool is2D = false;
                int width = 0;
                int length = 0;
                int height = 0;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.Equals("2D"))
                        is2D = true;
                    else if (line.ToUpper().Contains("W"))
                        width = int.Parse(line.Split()[1]);
                    else if (line.ToUpper().Contains("L"))
                        length = int.Parse(line.Split()[1]);
                    else if (line.ToUpper().Contains("H"))
                        height = int.Parse(line.Split()[1]);
                }
                if (is2D)
                    world = new Space2D(width, length);
                else
                    world = new Space3D(width, length, height);
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
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                StreamReader sr = new StreamReader(openFileDialog1.FileName);
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    string[] obstacleParts = line.Split(' ');
                    if(obstacleParts[0].Equals("r"))
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
                if (o.type == Obstacle.ObstacleType.Polygon)
                {
                    List<RectangularObstacle> discreteObstacles = ((PolygonalObstacle)o).transformObstacle(distance);
                    newObstacles = newObstacles.Concat(discreteObstacles).ToList();
                }
                else if (o.type == Obstacle.ObstacleType.Spline)
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
                    drawRectangularObstacle((RectangularObstacle) o);
                    break;
                case Obstacle.ObstacleType.Polygon:
                    drawPolygonalObstacle((PolygonalObstacle) o);
                    break;
                case Obstacle.ObstacleType.Ellipse:
                    drawEllipticalObstacle((EllipticalObstacle) o);
                    break;
                case Obstacle.ObstacleType.Spline:
                    drawSplineObstacle((SplineObstacle) o);
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
            Coordinate relLoc = calculateRelativeCanvasPosition(new Coordinate(o.location.x - o.width/2,
                o.location.y - o.length/2));
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
                    segments[i] = new Coordinate(new int[] { segments[i - 1].x, segments[i - 1].y + yChange});
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
            Rectangle start = new Rectangle(relativeStartLocation.x - distance/2, relativeStartLocation.y - distance/2, 
                distance, distance);
            Rectangle finish = new Rectangle(relativeGoalLocation.x - distance/2, relativeGoalLocation.y - distance/2,
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
            // 20 - 100% of the actual width of form is the canvas for drawing
            // Purpose of this method is to calculate the position of the object in relation to these proportions
            // The actual [0,0] point is not going to be left-top corner of the form, but the left-top corner
            // of the canvas itself, leaving out parts of the form where the controls are

            double relativeX = location.x + Math.Ceiling((double)(Width / 5));
            int relativeY = location.y;

            return new Coordinate((int)relativeX, relativeY);
        }

        private RealCoordinate calculateRelativeCanvasPosition(RealCoordinate location)
        {
            double relativeX = location.x + Math.Ceiling((double)(Width / 5));
            double relativeY = location.y;

            return new RealCoordinate(relativeX, relativeY);
        }

        private void scaleForm()
        {
            // Minimum Form size: 640 * 480
            double newWidth = world.width / 0.8 + 16;
            double newLength = world.length / 0.8 + 18;

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

        private void optimisePath(int pathSegments)
        {
            int pathLength = 0;
            for (int i = 0; i < path.Count - 1; i++)
            {
                pathLength += Coordinate.getXDistanceBetweenCoordinates(path[i], path[i + 1]) +
                    Coordinate.getYDistanceBetweenCoordinates(path[i], path[i + 1]);
            }

            int decrementor = pathLength / 10;
            while (decrementor > 0)
            {
                pathLength -= decrementor;
                PathFinding finder = new PathFinding(agent.currentLocation, goalLocation, pathSegments,
                    distance, pathLength, world, curvedPath);
                try
                {
                    Coordinate[] newPath = finder.findPath();
                    path = new List<Coordinate>();
                    path.AddRange(newPath);
                }
                catch (PathFinding.TestFailedException)
                {
                    pathLength += decrementor;
                    decrementor = (int)(decrementor * 0.9);
                }
            }
        }
    }
}
