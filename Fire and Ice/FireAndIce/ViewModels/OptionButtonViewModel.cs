﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using System.Windows;

namespace FireAndIce.ViewModels
{
    public class OptionButtonViewModel : PropertyChangedBase
    {
        public System.Action ClickAction { get; set; }
        public event EventHandler WasClicked;

        private bool _isOptionChecked;
        public bool IsOptionChecked
        {
            get
            {
                return _isOptionChecked;
            }

            set
            {
                if (_isOptionChecked != value)
                {
                    _isOptionChecked = value;
                    NotifyOfPropertyChange(() => IsOptionChecked);

                    // TODO: Fix this nastiness...
                    NotifyOfPropertyChange(() => CanClick);
                }
            }
        }

        private string _title;
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
                NotifyOfPropertyChange(() => Title);
            }
        }

        public bool CanClick { get { return !IsOptionChecked; } }

        public void Click()
        {
            ClickAction();
            if (WasClicked != null)
            {
                WasClicked(this, null);
            }
        }
    }
}
