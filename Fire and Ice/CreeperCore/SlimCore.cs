﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using CreeperMessages;
using Creeper;

namespace CreeperCore
{
    public class SlimCore : IProvideBoardState, IHandle<MoveMessage>, IHandle<NetworkErrorMessage>, IHandle<StartGameMessage>
    {
        private IEventAggregator _eventAggregator;
        private Player _player1;
        private Player _player2;

        //Keeping a reference to the network so the GC doesn't eat it.
        private IHandle _networkReference;
        private IHandle _aiReference;

        //State Variables
        private CreeperBoard _board
        {
            get
            {
                if (_boardHistory.Count > 0)
                {
                    return _boardHistory.Peek();
                }
                else
                {
                    throw new IndexOutOfRangeException("No boards left in board history.");
                }
            }
        }
        private Player _currentPlayer;

        private Stack<CreeperBoard> _boardHistory;

        public SlimCore(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.Subscribe(this);
            _boardHistory = new Stack<CreeperBoard>();
            _boardHistory.Push(new CreeperBoard());
        }

        private void StartGame(GameSettings settings)
        {
            _player1 = new Player(settings.Player1Type, settings.StartingColor);
            _player2 = new Player(settings.Player2Type, settings.StartingColor.Opposite());
            _currentPlayer = _player1;
            _boardHistory.Clear();
            _boardHistory.Push(settings.Board);

            //if networked game
            if (_player1.Type == PlayerType.Network
                || _player2.Type == PlayerType.Network)
            {
                _networkReference = settings.Network;
                _eventAggregator.Subscribe(_networkReference);
            }

            //else if ai game
            if (_player1.Type == PlayerType.AI
                || _player2.Type == PlayerType.AI)
            {
                _aiReference = settings.AI;
                _eventAggregator.Subscribe(_aiReference);
            }

            RequestMove();
        }

        #region IProvideBoardState
        public CreeperBoard GetBoard()
        {
            return new CreeperBoard(_board);
        }

        public Player GetCurrentPlayer()
        {
            return new Player(_currentPlayer);
        }
        #endregion

        #region IHandle
        public void Handle(MoveMessage message)
        {
            if (message.Type == MoveMessageType.Response)
            {
                //TODO: throw some exceptions if something went wrong
                CreeperBoard board = new CreeperBoard(_board);
                board.Move(message.Move);
                _boardHistory.Push(board);

                switch (_board.GetGameState(_currentPlayer.Color))
                {
                    case CreeperGameState.Unfinished:
                        _currentPlayer = (_currentPlayer == _player1) ? _player2 : _player1;
                        RequestMove();
                        break;
                    case CreeperGameState.Complete:
                        _eventAggregator.Publish(new GameOverMessage() { Winner = _currentPlayer.Color, });
                        break;
                    case CreeperGameState.Draw:
                        _eventAggregator.Publish(new GameOverMessage() { Winner = null, });
                        break;
                }
            }
            else
            {
                if (message.Type == MoveMessageType.Undo)
                {
                    _boardHistory.Pop();
                    _currentPlayer = (_currentPlayer == _player1) ? _player2 : _player1;
                    _eventAggregator.Publish(new SychronizeBoardMessage() { Board = _board, });
                    //publish new move request
                }
            }
        }

        public void Handle(StartGameMessage message)
        {
            if (message.Settings.Player1Type == PlayerType.Tutorial
                || message.Settings.Player2Type == PlayerType.Tutorial)
            {
                _eventAggregator.Unsubscribe(this);
            }
            else
            {
                StartGame(message.Settings);
            }
        }

        public void Handle(NetworkErrorMessage message)
        {
            throw new NotImplementedException("Core did not handle network error.");
        }
        #endregion

        private void RequestMove()
        {
            _eventAggregator.Publish(new MoveMessage()
            {
                PlayerType = _currentPlayer.Type,
                Type = MoveMessageType.Request,
                Board = new CreeperBoard(_board),
                TurnColor = _currentPlayer.Color,
            });
        }
    }
}
