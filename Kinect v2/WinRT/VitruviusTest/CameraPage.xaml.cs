﻿using VitruviusTest.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using WindowsPreview.Kinect;
using LightBuzz.Vitruvius;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace VitruviusTest
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class CameraPage : Page
    {
        NavigationHelper _navigationHelper;

        KinectSensor _sensor;
        MultiSourceFrameReader _reader;
        IEnumerable<Body> _bodies;
        UsersReporter _userReporter;

        bool _displaySkeleton;

        public NavigationHelper NavigationHelper
        {
            get { return _navigationHelper; }
        }

        public CameraPage()
        {
            InitializeComponent();

            _navigationHelper = new NavigationHelper(this);

            _sensor = KinectSensor.GetDefault();

            if (_sensor != null)
            {
                _sensor.Open();

                _reader = _sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.Infrared | FrameSourceTypes.Body);
                _reader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;

                _userReporter = new UsersReporter();
                _userReporter.BodyEntered += UserReporter_BodyEntered;
                _userReporter.BodyLeft += UserReporter_BodyLeft;
                _userReporter.Start();
            }
        }

        private void PageRoot_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_userReporter != null)
            {
                _userReporter.Stop();
            }

            if (_reader != null)
            {
                _reader.Dispose();
            }

            if (_bodies != null)
            {
                if (_bodies.Count() > 0)
                {
                    foreach (var body in _bodies)
                    {
                        body.Dispose();
                    }
                }
            }

            if (_sensor != null)
            {
                _sensor.Close();
            }
        }

        private void Color_Click(object sender, RoutedEventArgs e)
        {
            viewer.Visualization = Visualization.Color;
        }

        private void Depth_Click(object sender, RoutedEventArgs e)
        {
            viewer.Visualization = Visualization.Depth;
        }

        private void Infrared_Click(object sender, RoutedEventArgs e)
        {
            viewer.Visualization = Visualization.Infrared;
        }

        private void Skeleton_Checked(object sender, RoutedEventArgs e)
        {
            _displaySkeleton = true;
        }

        private void Skeleton_Unchecked(object sender, RoutedEventArgs e)
        {
            _displaySkeleton = false;
        }

        void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            var reference = e.FrameReference.AcquireFrame();

            // Color
            using (var frame = reference.ColorFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    if (viewer.Visualization == Visualization.Color)
                    {
                        viewer.Image = frame.ToBitmap();
                    }
                }
            }

            // Depth
            using (var frame = reference.DepthFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    if (viewer.Visualization == Visualization.Depth)
                    {
                        viewer.Image = frame.ToBitmap();
                    }
                }
            }

            // Infrared
            using (var frame = reference.InfraredFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    if (viewer.Visualization == Visualization.Infrared)
                    {
                        viewer.Image = frame.ToBitmap();
                    }
                }
            }

            // Body
            using (var frame = reference.BodyFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    _bodies = frame.Bodies();
                    _userReporter.Update(_bodies);

                    foreach (Body body in _bodies)
                    {
                        if (_displaySkeleton)
                        {
                            viewer.DrawBody(body);
                        }
                    }
                }
            }
        }

        void UserReporter_BodyEntered(object sender, ActiveUserReporterEventArgs e)
        {
        }

        void UserReporter_BodyLeft(object sender, ActiveUserReporterEventArgs e)
        {
            viewer.Clear();
        }
    }
}
