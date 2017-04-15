using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using System.Drawing.Imaging;
using System.IO;
using System.Collections.Generic;

namespace SMTMotionPlanning
{
    public partial class SegmentationForm : Form
    {
        private int minimumCornerDistance;
        private Bitmap image;
        private Graphics tableLayoutGraphics;
        private int[] greenBounds;
        private int[] redBounds;
        private int[] blueBounds;
        private bool segmentationExecuted;
        private double pixelToCentimeterConversionCoefficient;

        public SegmentationForm()
        {
            InitializeComponent();
            tableLayoutGraphics = tableLayoutPanel1.CreateGraphics();
            minimumCornerDistance = 6;
            greenBounds = new int[2];
            redBounds = new int[2];
            blueBounds = new int[2];
            segmentationExecuted = false;
        }

        private void executeButton_Click(object sender, EventArgs e)
        {
            // Successful segmentation attempt on the UVT hallway photo
            // Green - <65,217> 
            // Red - <80,217>
            // Blue - <50,217>
            greenBounds[0] = greenLowerBar.Value;
            greenBounds[1] = greenUpperBar.Value;
            redBounds[0] = redLowerBar.Value;
            redBounds[1] = redUpperBar.Value;
            blueBounds[0] = blueLowerBar.Value;
            blueBounds[1] = blueUpperBar.Value;
            if (greenBounds[0] < greenBounds[1] && redBounds[0] < redBounds[1] && blueBounds[0] < blueBounds[1])
            {
                Segmentation segmentation = new Segmentation(redBounds, greenBounds, blueBounds, minimumCornerDistance);
                //segmentation.ProcessImage(image);
                Bitmap resizedImage = ResizeImage(image, 485, 281);
                segmentation.ProcessImage(resizedImage);
                Rectangle rect = new Rectangle(5, 286, 485, 281);
                tableLayoutGraphics.DrawImage(resizedImage, rect);
                segmentationExecuted = true;
            }
            else
                MessageBox.Show("Invalid background color boundary settings. Lower boundary must be lower than upper boundary",
                    "Background boundaries error", MessageBoxButtons.OK, MessageBoxIcon.Error);

        }

        private void loadButton_Click(object sender, EventArgs e)
        {
            // width: 485
            // height: 281
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                image = new Bitmap(openFileDialog1.FileName);
                Image resizedImage = ResizeImage(image, 485, 281);
                Rectangle rect = new Rectangle(5, 5, 485, 281);
                tableLayoutGraphics.DrawImage(resizedImage, rect);
            }
        }

        private Bitmap ResizeImage(Bitmap image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        private void greenLowerBar_ValueChanged(object sender, EventArgs e)
        {
            greenLowerLabel.Text = greenLowerBar.Value.ToString();
        }

        private void greenUpperBar_ValueChanged(object sender, EventArgs e)
        {
            greenUpperLabel.Text = greenUpperBar.Value.ToString();
        }

        private void redLowerBar_ValueChanged(object sender, EventArgs e)
        {
            redLowerLabel.Text = redLowerBar.Value.ToString();
        }

        private void redUpperBar_ValueChanged(object sender, EventArgs e)
        {
            redUpperLabel.Text = redUpperBar.Value.ToString();
        }

        private void blueLowerBar_ValueChanged(object sender, EventArgs e)
        {
            blueLowerLabel.Text = blueLowerBar.Value.ToString();
        }

        private void blueUpperBar_ValueChanged(object sender, EventArgs e)
        {
            blueUpperLabel.Text = blueUpperBar.Value.ToString();
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void captureButton_Click(object sender, EventArgs e)
        {
            ImageCapture capture = new ImageCapture("227");
            capture.captureFrame();
        }

        private void stitchButton_Click(object sender, EventArgs e)
        {
            Bitmap image1 = null, image2 = null;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                image1 = new Bitmap(openFileDialog1.FileName);
            }
            if (openFileDialog2.ShowDialog() == DialogResult.OK)
            {
                image2 = new Bitmap(openFileDialog2.FileName);
            }

            ImageStitching stitching = new ImageStitching(image1, image2);
            stitching.doItAll();
        }

        private void checkButton_Click(object sender, EventArgs e)
        {
            if (image != null)
            {
                Bitmap resizedImage = ResizeImage(image, 485, 281);
                int red, green, blue;
                string[] pixelCoordinates = checkBox.Text.Split(' ');
                int x = int.Parse(pixelCoordinates[0]);
                int y = int.Parse(pixelCoordinates[1]);
                if (x < 0 || x > 485 || y < 0 || y > 281)
                    MessageBox.Show("Pixel coordinates are out of range.", "Coordinates error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                {
                    Color pixelColor = resizedImage.GetPixel(x, y);
                    red = pixelColor.R;
                    green = pixelColor.G;
                    blue = pixelColor.B;
                    MessageBox.Show("Red: " + red + " Blue: " + blue + " Green: "
                        + green, "Image checkup", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
                MessageBox.Show("Image not found!", "Error loading image", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void resolveButton_Click(object sender, EventArgs e)
        {
            if (segmentationExecuted)
            {
                FileStream stream = new FileStream("obstacles.txt", FileMode.Open, FileAccess.Read);
                StreamReader reader = new StreamReader(stream);
                List<Circle> circles = new List<Circle>();
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] lineParts = line.Split(' ');
                    if (lineParts[0] == "e")
                    {
                        int x = int.Parse(lineParts[1]);
                        int y = int.Parse(lineParts[2]);
                        int radius = int.Parse(lineParts[3]);
                        circles.Add(new Circle(x, y, radius));
                    }
                }
                reader.Close();
                stream.Close();
                Circle reference = getTopCircle(circles);
                double pixelDistance = getPixelConversion(circles, reference);
                int realDistance = int.Parse(realDistanceBox.Text);
                pixelToCentimeterConversionCoefficient = pixelDistance / realDistance;
                MessageBox.Show("1 pixel on image is " + pixelToCentimeterConversionCoefficient + " cm.", "Conversion info.",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                adjustCoordinatesInFile(reference);
            }
        }

        private Circle getTopCircle(List<Circle> circles)
        {
            Circle topCircle = null;
            double minimumDistance = 0.0;
            foreach (Circle c in circles)
            {
                if (minimumDistance == 0)
                {
                    topCircle = c;
                    minimumDistance = Math.Sqrt(c.x * c.x + c.y * c.y);
                }
                else
                {
                    double currentDistance = Math.Sqrt(c.x * c.x + c.y * c.y);
                    if (currentDistance < minimumDistance)
                    {
                        topCircle = c;
                        minimumDistance = currentDistance;
                    }
                }
            }
            return topCircle;
        }

        private double getPixelConversion(List<Circle> circles, Circle reference)
        {
            Circle closestToReferenceCircle = null;
            double closestDistance = 0.0;
            foreach (Circle circle in circles)
            {
                if (circle.Equals(reference))
                    continue;
                else
                {
                    if (closestToReferenceCircle == null)
                    {
                        closestToReferenceCircle = circle;
                        closestDistance = Math.Sqrt(Math.Pow(closestToReferenceCircle.x - reference.x, 2) +
                            Math.Pow(closestToReferenceCircle.y - reference.y, 2));
                    }
                    else
                    {
                        double currentDistance = Math.Sqrt(Math.Pow(circle.x - reference.x, 2) +
                            Math.Pow(circle.y - reference.y, 2));
                        if (currentDistance < closestDistance)
                        {
                            closestDistance = currentDistance;
                            closestToReferenceCircle = circle;
                        }
                    }
                }
            }

            return closestDistance;
        }

        private void adjustCoordinatesInFile(Circle reference)
        {
            // Map will be resized to 640x480 resolution, with top left corner being a red ball
            // and bottom right corner having coordinates [640,480]
            // Original picture is resized to 485x281 resolution
            FileStream readStream = new FileStream("obstacles.txt", FileMode.Open, FileAccess.Read);
            StreamReader reader = new StreamReader(readStream);
            FileStream writeStream = new FileStream("new_obstacles.txt", FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter writer = new StreamWriter(writeStream);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] lineSegments = line.Split(' ');
                if (lineSegments[0] == "r")
                    writer.WriteLine(adjustRectangle(lineSegments, reference));
                if (lineSegments[0] == "e")
                    writer.WriteLine(adjustEllipse(lineSegments, reference));
                if (lineSegments[0] == "s" || lineSegments[0] == "p")
                    writer.WriteLine(adjustPointsShape(lineSegments, reference));
            }
            writer.Close();
            writeStream.Close();
            reader.Close();
            readStream.Close();
        }

        private string adjustRectangle(string[] rectangleSegments, Circle reference)
        {
            int adjustedX = (int.Parse(rectangleSegments[1]) - reference.x) * 640 / 485;
            int adjustedY = (int.Parse(rectangleSegments[2]) - reference.y) * 480 / 281;
            int adjustedWidth = int.Parse(rectangleSegments[3]) * 640 / 485;
            int adjustedHeight = int.Parse(rectangleSegments[4]) * 480 / 281;
            string adjustedRectangle = rectangleSegments[0] + " " + adjustedX + " " + adjustedY + " " +
                adjustedWidth + " " + adjustedHeight;
            return adjustedRectangle;
        }

        private string adjustEllipse(string[] ellipseSegments, Circle reference)
        {
            int adjustedX = (int.Parse(ellipseSegments[1]) - reference.x) * 640 / 485;
            int adjustedY = (int.Parse(ellipseSegments[2]) - reference.y) * 480 / 281;
            int adjustedWidth = int.Parse(ellipseSegments[3]) * 640 / 485;
            int adjustedHeight = int.Parse(ellipseSegments[4]) * 480 / 281;
            string adjustedEllipse = ellipseSegments[0] + " " + adjustedX + " " + adjustedY + " " +
                adjustedWidth + " " + adjustedHeight;
            return adjustedEllipse;
        }
        private string adjustPointsShape(string[] points, Circle reference)
        {
            List<Point> pointsList = new List<Point>();
            for (int i = 1; i < points.Length; i = i + 2)
            {
                int x = (int.Parse(points[i]) - reference.x) * 640 / 480;
                int y = (int.Parse(points[i]) - reference.y) * 480 / 281;
                pointsList.Add(new Point(x, y));
            }
            string adjustedPoints = points[0];
            foreach (Point point in pointsList)
                adjustedPoints = string.Concat(adjustedPoints, " ", point.X, " ", point.Y);
            return adjustedPoints;
        }
    }
}
