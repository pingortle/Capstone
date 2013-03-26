﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Creeper;
using CreeperNetwork;
using XNAControlGame;
using System.ComponentModel;
using System.IO;
using CreeperAI;
using Caliburn.Micro;
using CreeperMessages;

namespace CreeperCore
{
    public class CreeperGameCore : IHandle<MoveMessage>
    {
        public XNAControl.XNAControlGame XNAGame
        {
            get
            {
                return _xnaGame;
            }
            set
            {
                _xnaGame = value;
            }
        }

        private XNAControl.XNAControlGame _xnaGame;
        private Network _network;
        private IEventAggregator _eventAggregator;
        private bool _IsNetworkGame { get { return GameTracker.Player1.PlayerType == PlayerType.Network || GameTracker.Player2.PlayerType == PlayerType.Network; } }
        
        public CreeperGameCore(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.Subscribe(this);

            GameTracker.Board = new CreeperBoard();
        }

        public void InitializeGameGUI(IntPtr handle, int width, int height)
        {
            //TODO: Figure this out
            //XNAGame = new YeOldeGame1(handle, width, height, _eventAggregator);
            XNAGame = new Game1(handle, width, height, _eventAggregator);
        }

        public void StartLocalGame(PlayerType player1Type, PlayerType player2Type, AIDifficulty difficulty)
        {
            if (player1Type == PlayerType.Network || player2Type == PlayerType.Network)
            {
                throw new ArgumentException("Cannot start a local game with a player type of network.");
            }

            StartGame(player1Type, player2Type);
        }

        public void StartNetworkGame(PlayerType player1Type, PlayerType player2Type, Network network)
        {
            if (player1Type == PlayerType.Network && player2Type == PlayerType.Network)
            {
                throw new ArgumentException("Cannot start network game where both players are network players.");
            }

            _network = network;

            StartGame(player1Type, player2Type);

            _network.runGame();
        }

        private void StartGame(PlayerType player1Type, PlayerType player2Type)
        {
            GameTracker.Player1 = new Player(player1Type, CreeperColor.Fire);
            GameTracker.Player2 = new Player(player2Type, CreeperColor.Ice);
            GameTracker.CurrentPlayer = GameTracker.Player1;
            GetNextMove();
        }

        private void GetNextMove()
        {
            _eventAggregator.Publish(new MoveMessage(GameTracker.CurrentPlayer.PlayerType, MoveMessageType.Request));
        }

        public void Handle(MoveMessage message)
        {
            if (message.Type == MoveMessageType.Response)
            {
                GameTracker.Board.Move(message.Move);

                if (!GameTracker.Board.IsFinished(message.Move.PlayerColor))
                {
                    GameTracker.CurrentPlayer = GameTracker.OpponentPlayer;

                    GetNextMove();
                }
                else
                {
                    //TODO: handle draw here
                    _eventAggregator.Publish(new GameOverMessage(GameOverType.Win, GameTracker.CurrentPlayer));
                }
            }
        }
    }
}
