﻿using Emgu.CV;
using Emgu.CV.Structure;
using System;

namespace SMTMotionPlanning
{
    class ImageCapture
    {
        public class CameraNotFoundException : Exception { }
        private string source;
        public ImageCapture(string source)
        {
            this.source = "rtsp://147.232.24." + source + "/live.sdp";
        }

        public void captureFrame()
        {
            Mat frame = capture(source);
            Image<Bgr, byte> image = frame.ToImage<Bgr, byte>();
            string[] segments = source.Split('.');
            string[] moreSegments = segments[3].Split('/');
            string path = @"C:\Users\Robert\Documents\Visual Studio 2015\Projects\SMTMotionPlanning\SMTMotionPlanning\Pictures\";
            image.Save(path + "fromCamera" + moreSegments[0] + ".png");
        }

        private Mat capture(string address)
        {
            Capture cameraCapture;
            cameraCapture = new Capture(address);
            Mat frame = cameraCapture.QueryFrame();
            if (frame == null)
                throw new CameraNotFoundException();
            return frame;
        }
    }
}
