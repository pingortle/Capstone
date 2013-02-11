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
using System.Windows.Media.Animation;
using System.Windows.Controls.Primitives;

namespace FireAndIce
{
    /// <summary>
    /// Interaction logic for MainMenuScreen.xaml
    /// </summary>
    public partial class MainMenuScreen : UserControl
    {
        private Border _activeBorder;
        private MainWindow _mainWindow;

        public MainMenuScreen(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
        }

        private void SlideOutBorder(Border border, double width)
        {
            border.Opacity = 1;
            border.Visibility = Visibility.Visible;

            DoubleAnimation slideIn = new DoubleAnimation();
            slideIn.From = 0;
            slideIn.To = width;
            slideIn.Duration = new Duration(TimeSpan.FromMilliseconds(500));

            Storyboard.SetTargetName(slideIn, border.Name);
            Storyboard.SetTargetProperty(slideIn, new PropertyPath(Border.WidthProperty));

            Storyboard storyBoard = new Storyboard();
            storyBoard.Children.Add(slideIn);

            storyBoard.Begin(border);
        }

        private void ToggleBorder(Border border, double width)
        {
            if (_activeBorder == border)
            {
                _activeBorder.Visibility = Visibility.Collapsed;
                _activeBorder = null;
            }
            else
            {
                if (_activeBorder != null)
                {
                    _activeBorder.Visibility = Visibility.Collapsed;
                }

                SlideOutBorder(border, width);
                _activeBorder = border;
            }
        }

        private void ToggleButtons(ToggleButton activeButton, StackPanel buttonPanel)
        {
            IEnumerable<ToggleButton> toggles = buttonPanel.Children.OfType<ToggleButton>();
            foreach (ToggleButton toggle in toggles)
            {
                if (toggle != activeButton)
                {
                    toggle.IsChecked = false;
                }
            }
        }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            ToggleBorder(NewGameMenu, 200d);
            ToggleButtons(NewGameButton, MainMenuButtonPanel);
        }

        private void HighScoreButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleBorder(HighScoreBorder, 400d);
            ToggleButtons(HighScoreButton, MainMenuButtonPanel);
            //Initialize high scores here
            HighScoreList.ItemsSource = new List<String> { "score 1", "score 2", "score 3" };
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleBorder(SettingsBorder, 400d);
            ToggleButtons(SettingsButton, MainMenuButtonPanel);
        }

        private void CreditsButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleBorder(CreditsBorder, 400d);
            ToggleButtons(CreditsButton, MainMenuButtonPanel);
        }

        private void PlayHumanGameButton_Click(object sender, RoutedEventArgs e)
        {
            //This will change to expand a menu to ask if they want to play locally or over the network
            _mainWindow.Content = new GameControl();
        }

        private void PlayComputerGameButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.Content = new GameControl();
        }
    }
}
