﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UpdateControls;
using System.Windows;

namespace FireAndIce
{
    public class MainMenuViewModel
    {
        private Visibility _newGameMenuVisibility;
        private Visibility _newHumanGameMenuVisibility;
        private Visibility _newComputerGameMenuVisibility;
        private Visibility _helpBorder;
        private Visibility _highScoreBorder;
        private Visibility _settingsBorder;
        private Visibility _creditsBorder;

        public MainMenuViewModel()
        {
            _newGameMenuVisibility = Visibility.Collapsed;
            _newHumanGameMenuVisibility = Visibility.Collapsed;
            _newComputerGameMenuVisibility = Visibility.Collapsed;
            _helpBorder = Visibility.Collapsed;
            _highScoreBorder = Visibility.Collapsed;
            _settingsBorder = Visibility.Collapsed;
            _creditsBorder = Visibility.Collapsed;
        }

        #region Independent properties
        // Generated by Update Controls --------------------------------
        private Independent _indNewGameMenuVisibility = new Independent();
        private Independent _indNewHumanGameMenuVisibility = new Independent();
        private Independent _indNewComputerGameMenuVisibility = new Independent();
        private Independent _indHelpBorder = new Independent();
        private Independent _indHighScoreBorder = new Independent();
        private Independent _indSettingsBorder = new Independent();
        private Independent _indCreditsBorder = new Independent();

        public Visibility NewGameMenuVisibility
        {
            get { _indNewGameMenuVisibility.OnGet(); return _newGameMenuVisibility; }
            set { _indNewGameMenuVisibility.OnSet(); _newGameMenuVisibility = value; }
        }

        public Visibility NewHumanGameMenuVisibility
        {
            get { _indNewHumanGameMenuVisibility.OnGet(); return _newHumanGameMenuVisibility; }
            set { _indNewHumanGameMenuVisibility.OnSet(); _newHumanGameMenuVisibility = value; }
        }

        public Visibility NewComputerGameMenuVisibility
        {
            get { _indNewComputerGameMenuVisibility.OnGet(); return _newComputerGameMenuVisibility; }
            set { _indNewComputerGameMenuVisibility.OnSet(); _newComputerGameMenuVisibility = value; }
        }

        public Visibility HelpBorder
        {
            get { _indHelpBorder.OnGet(); return _helpBorder; }
            set { _indHelpBorder.OnSet(); _helpBorder = value; }
        }

        public Visibility HighScoreBorder
        {
            get { _indHighScoreBorder.OnGet(); return _highScoreBorder; }
            set { _indHighScoreBorder.OnSet(); _highScoreBorder = value; }
        }

        public Visibility SettingsBorder
        {
            get { _indSettingsBorder.OnGet(); return _settingsBorder; }
            set { _indSettingsBorder.OnSet(); _settingsBorder = value; }
        }

        public Visibility CreditsBorder
        {
            get { _indCreditsBorder.OnGet(); return _creditsBorder; }
            set { _indCreditsBorder.OnSet(); _creditsBorder = value; }
        }
        // End generated code --------------------------------
        #endregion
    }
}