﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using XNAControl;
using CreeperCore;
using FireAndIce.Views;
using CreeperNetwork;
using System.Windows.Media;
using Creeper;
using CreeperMessages;

namespace FireAndIce.ViewModels
{
    class GameContainerViewModel : Screen, IHandle<GameOverMessage>, IHandle<MoveMessage>, IHandle<ChatMessage>
    {
        private PlayerType _player1Type;
        private PlayerType _player2Type;
        private Network _network = null;
        private AIDifficulty _aiDifficulty = AIDifficulty.Hard;

        //private SlideOutPanelViewModel _gameMenu;
        public ToggleButtonMenuViewModel GameMenu
        {
            get
            {
                return new ToggleButtonMenuViewModel()
                {
                    Buttons = new BindableCollection<OptionButtonViewModel>()
                        {
                            new OptionButtonViewModel() { ClickAction = ReturnToMainMenu, Title = "Quit" },
                        },
                    Background = AppModel.Resources["Primary1"] as SolidColorBrush,
                    Title = "Game Menu",
                };
            }
        }

        public bool IsNetworkGame
        {
            get { return _network != null; }
        }

        private String _gameOverText;
        public String GameOverText
        {
            get { return _gameOverText ?? ""; }
            set
            {
                _gameOverText = value;
                NotifyOfPropertyChange(() => GameOverText);
            }
        }

        public String CurrentTurn
        {
            get
            {
                return GameTracker.CurrentPlayer != null? GameTracker.CurrentPlayer.Color.ToString() : "Fire";
            }
        }

        private BindableCollection<String> _chatMessages;
        public BindableCollection<String> ChatMessages
        {
            get { return _chatMessages = _chatMessages ?? new BindableCollection<String>(); }
            set
            {
                _chatMessages = value;
                NotifyOfPropertyChange(() => ChatMessages);
            }
        }

        private String _message;
        public String Message
        {
            get { return _message; }
            set
            {
                _message = value;
                NotifyOfPropertyChange(() => Message);
            }
        }

        public GameContainerViewModel(PlayerType player1Type, PlayerType player2Type, Network network = null) : base()
        {
            Init(player1Type, player2Type);
            _network = network;
            NotifyOfPropertyChange(() => IsNetworkGame);
        }

        public GameContainerViewModel(PlayerType player1Type, PlayerType player2Type, AIDifficulty difficulty)
            : base()
        {
            Init(player1Type, player2Type);
            _aiDifficulty = difficulty;
        }

        private void Init(PlayerType player1Type, PlayerType player2Type)
        {
            AppModel.EventAggregator.Subscribe(this);

            _player1Type = player1Type;
            _player2Type = player2Type;
        }

        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);

            XNAUserControl xnaView = (view as GameContainerView).XNAControl;

            AppModel.Core.InitializeGameGUI(xnaView.Handle, (int)xnaView.Width, (int)xnaView.Height);

            if (_network == null)
            {
                AppModel.Core.StartLocalGame(_player1Type, _player2Type, _aiDifficulty);
            }
            else
            {
                AppModel.Core.StartNetworkGame(_player1Type, _player2Type, _network);
            }
        }

        public void ReturnToMainMenu()
        {
            AppModel.AppViewModel.ActivateItem(new MainMenuViewModel());
        }

        public void Handle(GameOverMessage message)
        {
            GameOverText = String.Format("{0} wins!", message.Sender.Color.ToString());
        }

        public void Handle(MoveMessage message)
        {
            NotifyOfPropertyChange(() => CurrentTurn);
        }

        public void SendMessage()
        {
            AppModel.EventAggregator.Publish(new ChatMessage(Message, ChatMessageType.Send));
            Message = "";
        }

        public void Handle(ChatMessage message)
        {
            ChatMessages.Add(message.Message);
        }
    }
}
