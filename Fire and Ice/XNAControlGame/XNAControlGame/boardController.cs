﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nine;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Nine.Graphics;
using Creeper;
using Nine.Graphics.Primitives;

namespace XNAControlGame
{
    public static class BoardHelper
    {
        public static CreeperColor ToCreeperColor(this CreeperPegType creeperPegType)
        {
            switch (creeperPegType)
            {
                case CreeperPegType.Fire:
                    return CreeperColor.Fire;
                case CreeperPegType.Ice:
                    return CreeperColor.Ice;
                default:
                    throw new ArgumentOutOfRangeException("Can't let you do that, Star Fox.\nAndross has ordered us to take you down.");
            }
        }
    }

    class BoardController : Component
    {
        public IProvideBoardState BoardProvider { get; set; }
        public Action<Move> PublishMove { get; set; }
        public Action<Position, CreeperColor> FlipTile { get; set; }
        public CreeperBoardViewModel ViewModel { get; set; }

        private PegController _selectedPeg;
        private PegController _SelectedPeg
        {
            get
            {
                return _selectedPeg;
            }
            set
            {
                if (_selectedPeg != value)
                {
                    _selectedPeg = value;
                    UpdatePossibleMoves(value);
                }
            }
        }

        private IEnumerable<PegController> _pegs
        {
            get
            {
                return Parent.Children
                    .Where(x => x.GetType() == typeof(PegController))
                    .Select(x => (PegController)x);
            }
        }

        private IEnumerable<PegController> _firePegs
        {
            get
            {
                return _pegs.Where(x => x.PegType == CreeperPegType.Fire);
            }
        }

        private IEnumerable<PegController> _icePegs
        {
            get
            {
                return _pegs.Where(x => x.PegType == CreeperPegType.Ice);
            }
        }

        private IEnumerable<PegController> _possiblePegs
        {
            get
            {
                return _pegs.Where(x => x.PegType == CreeperPegType.Possible);
            }
        }

        public void Move(Move move, System.Action callback)
        {
            List<PegController> pegs = new List<PegController>();
            Parent.Traverse(pegs);
            PegController peg = pegs.First(x => x.Position == move.StartPosition);

            MoveType moveType = MoveType.Normal;
            if (CreeperBoard.IsFlipMove(move))
            {
                moveType = MoveType.TileJump;
                FlipTile(CreeperBoard.GetFlippedPosition(move), move.PlayerColor);
            }
            else if (CreeperBoard.IsCaptureMove(move))
            {
                moveType = MoveType.PegJump;
            }

            peg.MoveTo(move.EndPosition, ViewModel.GraphicalPositions[move.EndPosition.Row, move.EndPosition.Column], moveType, callback);
        }

        private void ClearPossiblePegs()
        {
            foreach (PegController pegToRemove in _possiblePegs)
            {
                Scene.Remove(pegToRemove.Parent);
            }
        }

        private PegController _lastDownClickedModel;
        public void DetectFullClick(Nine.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                PegController clickedModel = GetClickedModel(new Vector2(e.MouseState.X, e.MouseState.Y));
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
                            BoardProvider.GetCurrentPlayer().Color == clickedModel.PegType.ToCreeperColor())
                        {
                            OnPegClicked(clickedModel);
                        }
                    }
                }
                else
                {
                    _SelectedPeg = null;
                }
            }
        }

        private void OnPegClicked(PegController clickedModel)
        {
            if (_SelectedPeg == clickedModel)
            {
                _SelectedPeg = null;
            }

            else
            {
                switch (clickedModel.PegType)
                {
                    case CreeperPegType.Fire:
                        goto case CreeperPegType.Ice;
                    case CreeperPegType.Ice:
                        _SelectedPeg = clickedModel;
                        break;
                    case CreeperPegType.Possible:
                        PublishMove(new Move(new Position(_SelectedPeg.Position), new Position(clickedModel.Position), _SelectedPeg.PegType.ToCreeperColor()));
                        _SelectedPeg = null;
                        break;
                }
            }
        }

        private PegController GetClickedModel(Vector2 mousePosition)
        {
            Camera camera = Scene.FindName<Camera>("MainCamera");
            Ray selectionRay = Scene.GetGraphicsDevice().Viewport.CreatePickRay((int)mousePosition.X, (int)mousePosition.Y, camera.View, camera.Projection);

            List<PegController> found = new List<PegController>();
            if (Parent.ComputeBounds().Intersects(selectionRay).HasValue)
            {
                Parent.Traverse<PegController>(found);
            }

            return found.FirstOrDefault(x => x.IsPegClicked(selectionRay));
        }

        private void UpdatePossibleMoves(PegController clickedPeg)
        {
            ClearPossiblePegs();

            if (clickedPeg != null)
            {
                IEnumerable<Move> possibleMoves = BoardProvider.GetBoard().Pegs.At(clickedPeg.Position).PossibleMoves(BoardProvider.GetBoard());
                foreach (Position position in possibleMoves.Select(x => x.EndPosition))
                {
                    PegController peg = new PegController()
                    {
                        Position = position,
                        PegType = CreeperPegType.Possible,
                    };

                    Group group = new Group(peg, new Sphere(Scene.GetGraphicsDevice()));
                    group.Transform = Matrix.CreateTranslation(ViewModel.GraphicalPositions[position.Row, position.Column]);
                    Parent.Add(group);
                    Scene.Remove(Parent);
                    Scene.Add(Parent);
                }
            }
        }

        public void SynchronizePegs(CreeperBoard creeperBoard)
        {
            throw new NotImplementedException();
        }
    }
}
