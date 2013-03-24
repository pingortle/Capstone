﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Creeper;
using Nine.Graphics;
using Caliburn.Micro;
using CreeperMessages;
using Nine;

namespace XNAControlGame
{
    /// <summary>
    /// Any methods that are not overrides go here
    /// </summary>
    public partial class Game1 : IDisposable, IHandle<MoveRequestMessage>, IHandle<MoveResponseMessage>
    {
        private void ClearPossiblePegs()
        {
            foreach (CreeperPeg pegToRemove in _possiblePegs)
            {
                _boardGroup.Remove(pegToRemove);
            }
        }

        private CreeperPeg _lastDownClickedModel;
        void DetectFullClick(Nine.MouseEventArgs e)
        {
            CreeperPeg clickedModel = GetClickedModel(new Vector2(e.MouseState.X, e.MouseState.Y));
            if (clickedModel != null)
            {
                //if downclick
                if (e.IsButtonDown(e.Button))
                {
                    _lastDownClickedModel = clickedModel;
                }
                //if upclick
                else if (_lastDownClickedModel == clickedModel)
                {
                    _lastDownClickedModel = null;

                    if (clickedModel.PegType == CreeperPegType.Possible ||
                        GameTracker.CurrentPlayer.Color == clickedModel.PegType.ToCreeperColor())
                    {
                        OnPegClicked(clickedModel);
                    }
                }
            }
        }

        private void OnPegClicked(CreeperPeg clickedModel)
        {
            if (_selectedPeg == clickedModel
                && _possiblePegs.Any())
            {
                ClearPossiblePegs();
            }

            else
            {
                switch (clickedModel.PegType)
                {
                    case CreeperPegType.Fire:
                        _selectedPeg = clickedModel;
                        UpdatePossibleMoves(clickedModel);
                        break;
                    case CreeperPegType.Ice:
                        _selectedPeg = clickedModel;
                        UpdatePossibleMoves(clickedModel);
                        break;
                    case CreeperPegType.Possible:
                        _eventAggregator.Publish(
                            new MoveResponseMessage(
                                new Move(_selectedPeg.Position, clickedModel.Position,
                                    _selectedPeg.PegType.ToCreeperColor()),
                                    PlayerType.Human)
                                );
                        ClearPossiblePegs();
                        break;
                }
            }
        }

        private void UpdatePossibleMoves(CreeperPeg clickedPeg)
        {
            if (_possiblePegs.Any())
            {
                ClearPossiblePegs();
            }

            IEnumerable<Move> possibleMoves = GameTracker.Board.Pegs.At(clickedPeg.Position).PossibleMoves(GameTracker.Board);
            foreach (Position position in possibleMoves.Select(x => x.EndPosition))
            {
                CreeperPeg peg = new CreeperPeg(_iceModel) { Position = position, PegType = CreeperPegType.Possible, };
                _boardGroup.Add(peg);
            }

        }

        /// <summary>
        /// Returns a ray fired from the click point to test for intersection with a model.
        /// </summary>
        Ray GetSelectionRay(Vector2 mouseCoor)
        {
            Vector3 nearsource = new Vector3(mouseCoor, 0f);
            Vector3 farsource = new Vector3(mouseCoor, 1f);

            Matrix world = Matrix.CreateTranslation(0, 0, 0);

            Vector3 nearPoint = GraphicsDevice.Viewport.Unproject(nearsource, _scene.FindName<FreeCamera>("MainCamera").Projection,
                    _scene.FindName<FreeCamera>("MainCamera").View, world);

            Vector3 farPoint = GraphicsDevice.Viewport.Unproject(farsource, _scene.FindName<FreeCamera>("MainCamera").Projection,
                    _scene.FindName<FreeCamera>("MainCamera").View, world);

            // Create a ray from the near clip plane to the far clip plane.
            Vector3 direction = farPoint - nearPoint;
            direction.Normalize();
            Ray pickRay = new Ray(nearPoint, direction);

            return pickRay;
        }

        private CreeperPeg GetClickedModel(Vector2 mousePosition)
        {
            Ray selectionRay = GetSelectionRay(mousePosition);
            List<CreeperPeg> currentTeam = ((GameTracker.CurrentPlayer.Color == CreeperColor.Fire) ? _firePegs : _icePegs).ToList();
            currentTeam.AddRange(_possiblePegs);

            CreeperPeg clickedModel = null;

            foreach (CreeperPeg peg in currentTeam)
            {
                if (selectionRay.Intersects(peg.BoundingBox).HasValue)
                {
                    clickedModel = peg;
                    break;
                }
            }
            return clickedModel;
        }

        private void LoadViewModels()
        {
            _creeperBoardViewModel = new CreeperBoardViewModel(_scene.FindName<Surface>("boardSurface").Heightmap.Height, _scene.FindName<Surface>("boardSurface").Heightmap.Width, _scene.FindName<Surface>("boardSurface").Heightmap.Step);
        }

        private void OnContentLoaded()
        {
            _boardGroup = _scene.FindName<Group>(Resources.ElementNames.BoardGroup);

            LoadViewModels();
            LoadPegModels();
        }

        private void LoadPegModels()
        {
            foreach (Piece piece in GameTracker.Board.Pegs.Where(x => x.Color.IsTeamColor()))
            {
                CreeperPeg peg;
                if (piece.Color == CreeperColor.Fire)
                {
                    peg = new CreeperPeg(_fireModel)
                    {
                        PegType = CreeperPegType.Fire,
                        Position = piece.Position,
                    };

                }
                else
                {
                    peg = new CreeperPeg(_iceModel)
                    {
                        PegType = CreeperPegType.Ice,
                        Position = piece.Position,
                    };
                }

                _boardGroup.Add(peg);
            }
        }

        protected override void Dispose(bool disposing)
        {
            //dispose stuff

            base.Dispose(disposing);
        }

        public void Handle(MoveRequestMessage message)
        {
            if (message.Responder == PlayerType.Human)
            {
                _humanMovePending = true;
            }
        }

        public void Handle(MoveResponseMessage message)
        {
            CreeperPeg pegToMove = _pegs.First(x => x.Position == message.Move.StartPosition);
            pegToMove.MoveTo(message.Move.EndPosition);
        }
    }
}
