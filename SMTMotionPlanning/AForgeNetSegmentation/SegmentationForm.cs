using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;

namespace SMTMotionPlanning
{
    public partial class SegmentationForm : Form
    {
        private int minimumCornerDistance;
        private int originalImageHeight;
        private int originalImageWidth;
        private Bitmap image;
        private Graphics tableLayoutGraphics;
        private int[] greenBounds;
        private int[] redBounds;
        private int[] blueBounds;
        ToolTip locationToolTip;

        public SegmentationForm()
        {
            InitializeComponent();
            tableLayoutGraphics = tableLayoutPanel1.CreateGraphics();
            minimumCornerDistance = 6;
            greenBounds = new int[2];
            redBounds = new int[2];
            blueBounds = new int[2];
            originalImageWidth = 0;
            originalImageHeight = 0;
            StartPosition = FormStartPosition.Manual;
            Location = new Point(Screen.PrimaryScreen.Bounds.Right / 4, 0);
            locationToolTip = new ToolTip();
        }

        private void executeButton_Click(object sender, EventArgs e)
        {
            greenBounds[0] = greenLowerBar.Value;
            greenBounds[1] = greenUpperBar.Value;
            redBounds[0] = redLowerBar.Value;
            redBounds[1] = redUpperBar.Value;
            blueBounds[0] = blueLowerBar.Value;
            blueBounds[1] = blueUpperBar.Value;
            if (greenBounds[0] < greenBounds[1] && redBounds[0] < redBounds[1] && blueBounds[0] < blueBounds[1])
            {
                Segmentation segmentation = new Segmentation(redBounds, greenBounds, blueBounds, minimumCornerDistance,
                    originalImageHeight, originalImageWidth);
                Bitmap resizedImage = ResizeImage(image, 485, 281);
                Thread segmentiser = new Thread(() => segmentation.ProcessImage(resizedImage));
                segmentiser.Start();
                bool success = segmentiser.Join(10000);
                if (success == true)
                {
                    Rectangle rect = new Rectangle(5, 286, 485, 281);
                    tableLayoutGraphics.DrawImage(resizedImage, rect);
                }
                else
                    MessageBox.Show("Segmentation failed. Please try different color borders.", "Segmentation error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
                MessageBox.Show("Invalid background color boundary settings. Lower boundary must be lower than upper boundary",
                    "Background boundaries error", MessageBoxButtons.OK, MessageBoxIcon.Error);

        }

        private void loadButton_Click(object sender, EventArgs e)
        {
            // width: 485
            // height: 281
            string path = new DirectoryInfo(Environment.CurrentDirectory).Parent.Parent.Parent.FullName;
            path = Path.Combine(path, @"Pictures");
            openFileDialog1.Filter = "Png Images(*.png)|*.png|Jpeg Images(*.jpg)|*.jpg|Bitmaps(*.bmp)|*.bmp";
            openFileDialog1.InitialDirectory = path;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                image = new Bitmap(openFileDialog1.FileName);
                originalImageHeight = image.Height;
                originalImageWidth = image.Width;
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
            Close();
        }

        private void captureButton_Click(object sender, EventArgs e)
        {
            string cameraIPString = cameraBox.Text;
            int cameraIP;
            if (int.TryParse(cameraIPString, out cameraIP))
            {
                ImageCapture capture = new ImageCapture(cameraIPString);
                try
                {
                    capture.captureFrame();
                }
                catch (ImageCapture.CameraNotFoundException)
                {
                    MessageBox.Show("Camera with given IP address is inaccessible.", "IP adress error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Invalid camera IP number!", "IP adress error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void stitchButton_Click(object sender, EventArgs e)
        {
            Bitmap image1 = null, image2 = null;
            string path = new DirectoryInfo(Environment.CurrentDirectory).Parent.Parent.Parent.FullName;
            path = Path.Combine(path, @"Pictures");
            openFileDialog1.Filter = "Png Images(*.png)|*.png|Jpeg Images(*.jpg)|*.jpg|Bitmaps(*.bmp)|*.bmp";
            openFileDialog1.InitialDirectory = path;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                image1 = new Bitmap(openFileDialog1.FileName);
            }
            openFileDialog2.Filter = "Png Images(*.png)|*.png|Jpeg Images(*.jpg)|*.jpg|Bitmaps(*.bmp)|*.bmp";
            openFileDialog2.InitialDirectory = path;
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

        private void tableLayoutPanel1_MouseClick(object sender, MouseEventArgs e)
        {
            if (image != null)
            {
                if (e.X >= 5 && e.X <= 490 && e.Y >= 5 && e.Y <= 290)
                    locationToolTip.Show("X: " + e.X + " Y: " + e.Y, this, e.X, e.Y, 2500);
                else
                {
                    Image resizedImage = ResizeImage(image, 485, 281);
                    Rectangle rect = new Rectangle(5, 5, 485, 281);
                    tableLayoutGraphics.DrawImage(resizedImage, rect);
                    Invalidate();
                }
                    
            }
        }
    }
}
