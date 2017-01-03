using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Diagnostics;

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

        public Form1()
        {
            InitializeComponent();
            obstaclePen = new Pen(Color.Red, 5);
            pathPen = new Pen(Color.Black, 2);
            brush = new SolidBrush(Color.Red);
            // 21.10.2016 fixed graphics being created in form instead of the table layout
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
            // 21.10.2016 fixed parsing start y into goal y
            // 22.10.2016 fixed distance not being assigned to class data member
            // 22.10.2016 added graphics reinitialisation for table layout after being scaled.
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
                if (world == null)
                {
                    // 24.10.2016 fixed obstacles not being loaded into world, making them invisible for the solver
                    if (int.TryParse(widthEntryBox.Text, out width) && int.TryParse(lengthEntryBox.Text, out length))
                    {
                        world = new Space2D(width, length);
                        foreach (Obstacle o in obstacles)
                            world.obstacles.Add(o);
                    }
                    else
                        showParameterError();
                }
                else
                {
                    foreach (Obstacle o in obstacles)
                        world.obstacles.Add(o);
                    scaleForm();
                    graphicsObj = tableLayoutPanel1.CreateGraphics();
                }
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
        }

        private void executeButton_Click(object sender, EventArgs e)
        {
            if (world != null && obstacles != null && agent != null)
            {
                // 25.10.2016 fixed cycle trying to create path with 0 segments
                // 25.10.2016 fixed cycle not ending when path was found(added break sentence)
                FileStream stream = new FileStream("compTime.txt", FileMode.Create, FileAccess.Write);
                StreamWriter writer = new StreamWriter(stream);
                Stopwatch watch = new Stopwatch();
                path = new List<Coordinate>();
                transformNonRectangularObstacles();
                for (int i = 1; i <= 100; i++)
                {
                    watch.Reset();
                    watch.Start();
                    PathFinding finder = new PathFinding(agent.currentLocation, goalLocation, i, distance, world);
                    try
                    {
                        path.AddRange(finder.findPath());
                    }
                    catch (PathFinding.TestFailedException)
                    {
                        watch.Stop();
                        writer.WriteLine(watch.ElapsedMilliseconds);
                        continue;
                    }
                    break;
                }
                writer.Close();
                stream.Close();
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
            // 21.10.2016 fixed wrong parameter order for rectangles
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

        private void drawPathSegment(Coordinate c)
        {
            // 21.10.2016 added relative location 
            Coordinate predecessor = path[path.IndexOf(c) - 1];
            Coordinate relativePredecessorLocation = calculateRelativeCanvasPosition(predecessor);
            Coordinate relativeSuccessorLocation = calculateRelativeCanvasPosition(c); 
            graphicsObj.DrawLine(pathPen, relativePredecessorLocation.x, relativePredecessorLocation.y, 
                relativeSuccessorLocation.x, relativeSuccessorLocation.y);
        }
        private void drawPathSegment(Coordinate c1, Coordinate c2)
        {
            Coordinate relativePredecessorLocation = calculateRelativeCanvasPosition(c1);
            Coordinate relativeSuccessorLocation = calculateRelativeCanvasPosition(c2);
            graphicsObj.DrawLine(pathPen, relativePredecessorLocation.x, relativePredecessorLocation.y,
                relativeSuccessorLocation.x, relativeSuccessorLocation.y);
        }

        private void prettyPathDrawing(Coordinate c)
        {
            Coordinate predecessor = path[path.IndexOf(c) - 1];
            Coordinate relativePredecessorLocation = calculateRelativeCanvasPosition(predecessor);
            Coordinate relativeSuccessorLocation = calculateRelativeCanvasPosition(c);
            int xDistance = Coordinate.getXDistanceBetweenCoordinates(c, predecessor);
            int yDistance = Coordinate.getYDistanceBetweenCoordinates(c, predecessor);
            if (xDistance != 0)
            {
                int xChange = predecessor.x < c.x ? 1 : -1;
                Coordinate[] segments = new Coordinate[xDistance];
                segments[0] = predecessor;
                segments[xDistance - 1] = c;
                for (int i = 1; i < xDistance - 1; i++)
                {
                    segments[i] = new Coordinate(new int[] { segments[i - 1].x + xChange, segments[i - 1].y });
                    drawPathSegment(segments[i - 1], segments[i]);
                    Thread.Sleep((int)Math.Ceiling((double)(1000 / xDistance)));
                }
            }
            else
            {
                int yChange = predecessor.y < c.y ? 1 : -1;
                Coordinate[] segments = new Coordinate[yDistance];
                segments[0] = predecessor;
                segments[yDistance - 1] = c;
                for (int i = 1; i < yDistance - 1; i++)
                {
                    segments[i] = new Coordinate(new int[] { segments[i - 1].x, segments[i - 1].y + yChange});
                    drawPathSegment(segments[i - 1], segments[i]);
                    Thread.Sleep((int)Math.Ceiling((double)(1000 / yDistance)));
                }
            }  
        }

        private void drawStartAndGoalLocation()
        {
            // 19.10.2016 Fixed wrong referencing for start and goal location.
            // 21.10.2016 Fixed wrong parameter order in start and finish location
            // 21.10.2016 Added missing relative canvas position for start and goal location
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
    }
}
