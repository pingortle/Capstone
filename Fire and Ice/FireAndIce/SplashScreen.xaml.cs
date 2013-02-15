﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Timers;

namespace FireAndIce
{
    /// <summary>
    /// Interaction logic for SplashScreen.xaml
    /// </summary>
    public partial class SplashScreen : UserControl
    {
        private MainWindow _mainWindow;

        public SplashScreen(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            SplashScreenVideo.Play();
            SplashScreenVideo.MediaEnded += new RoutedEventHandler(SplashScreenVideo_MediaEnded);
        }

        void SplashScreenVideo_MediaEnded(object sender, RoutedEventArgs e)
        {
            SkipToIntro();
        }

        void SkipToIntro()
        {
            _mainWindow.StartIntroScreen();
        }

        private void SplashScreenVideo_MouseUp(object sender, MouseButtonEventArgs e)
        {
            SkipToIntro();
        }
    }
}