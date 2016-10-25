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
                            world.addObstacle(o);
                    }
                    else
                        showParameterError();
                }
                else
                {
                    foreach (Obstacle o in obstacles)
                        world.addObstacle(o);
                    scaleForm();
                    graphicsObj = tableLayoutPanel1.CreateGraphics();
                }
            }
            else
                showParameterError();
            Invalidate();
            Refresh();
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
                path = new List<Coordinate>();
                for (int i = 1; i < 100; i++)
                {
                    PathFinding finder = new PathFinding(agent.getLocation(), goalLocation, i, distance, world);
                    try
                    {
                        path.AddRange(finder.findPath());
                    }
                    catch (PathFinding.TestFailedException)
                    {
                        continue;
                    }
                    break;
                }
                // Does adding null to list increase its range?
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
            // Expected format used for obstacles in file:
            // o lefttopX lefttopY width length
            // Example: o 10 10 50 20
            obstacles = new List<Obstacle>();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                StreamReader sr = new StreamReader(openFileDialog1.FileName);
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    string[] obstacleParts = line.Split(' ');
                    // Fix: 21.10.2016. Length and width of obstacle was swapped with each other.
                    obstacles.Add(new Obstacle(int.Parse(obstacleParts[4]), int.Parse(obstacleParts[3]),
                        new Coordinate(int.Parse(obstacleParts[1]), int.Parse(obstacleParts[2]))));
                }
                sr.Close();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (obstacles != null)
            {
                foreach (Obstacle o in obstacles)
                    drawRectangle(o);
            }
            if (agent != null)
            {
                drawStartAndGoalLocation();
            }
            if (pathCalculated && runIssue)
            {
                foreach (Coordinate c in path.Skip(1))
                {
                    prettyPathDrawing(c);
                }
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

        private void drawRectangle(Obstacle o)
        {
            // 21.10.2016 fixed wrong parameter order for rectangles
            Coordinate relLoc = calculateRelativeCanvasPosition(o.getLocation());
            Rectangle rectangle = new Rectangle(relLoc.getX(), relLoc.getY(), o.getWidth(), o.getLength());
            graphicsObj.DrawRectangle(obstaclePen, rectangle);
            graphicsObj.FillRectangle(brush, rectangle);
        }

        private void drawPathSegment(Coordinate c)
        {
            // 21.10.2016 added relative location 
            Coordinate predecessor = path[path.IndexOf(c) - 1];
            Coordinate relativePredecessorLocation = calculateRelativeCanvasPosition(predecessor);
            Coordinate relativeSuccessorLocation = calculateRelativeCanvasPosition(c); 
            graphicsObj.DrawLine(pathPen, relativePredecessorLocation.getX(), relativePredecessorLocation.getY(), 
                relativeSuccessorLocation.getX(), relativeSuccessorLocation.getY());
        }
        private void drawPathSegment(Coordinate c1, Coordinate c2)
        {
            Coordinate relativePredecessorLocation = calculateRelativeCanvasPosition(c1);
            Coordinate relativeSuccessorLocation = calculateRelativeCanvasPosition(c2);
            graphicsObj.DrawLine(pathPen, relativePredecessorLocation.getX(), relativePredecessorLocation.getY(),
                relativeSuccessorLocation.getX(), relativeSuccessorLocation.getY());
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
                int xChange = predecessor.getX() < c.getX() ? 1 : -1;
                Coordinate[] segments = new Coordinate[xDistance];
                segments[0] = predecessor;
                segments[xDistance - 1] = c;
                for (int i = 1; i < xDistance - 1; i++)
                {
                    segments[i] = new Coordinate(new int[] { segments[i - 1].getX() + xChange, segments[i - 1].getY() });
                    drawPathSegment(segments[i - 1], segments[i]);
                    Thread.Sleep((int)Math.Ceiling((double)(1000 / xDistance)));
                }
            }
            else
            {
                int yChange = predecessor.getY() < c.getY() ? 1 : -1;
                Coordinate[] segments = new Coordinate[yDistance];
                segments[0] = predecessor;
                segments[yDistance - 1] = c;
                for (int i = 1; i < yDistance - 1; i++)
                {
                    segments[i] = new Coordinate(new int[] { segments[i - 1].getX(), segments[i - 1].getY() + yChange});
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
            Coordinate relativeStartLocation = calculateRelativeCanvasPosition(agent.getLocation());
            Coordinate relativeGoalLocation = calculateRelativeCanvasPosition(goalLocation);
            Rectangle start = new Rectangle(relativeStartLocation.getX() - distance/2, relativeStartLocation.getY() - distance/2, 
                distance, distance);
            Rectangle finish = new Rectangle(relativeGoalLocation.getX() - distance/2, relativeGoalLocation.getY() - distance/2,
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

            double relativeX = location.getX() + Math.Ceiling((double)(Width / 5));
            int relativeY = location.getY();

            return new Coordinate((int)relativeX, relativeY);
        }

        private void scaleForm()
        {
            // Minimum Form size: 640 * 480
            double newWidth = world.getWidth() / 0.8 + 16;
            double newLength = world.getLength() / 0.8 + 18;

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
