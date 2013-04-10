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
using Nine.Graphics.ParticleEffects;
using Nine.Animations;
using Microsoft.Xna.Framework.Graphics;
using Nine.Graphics.Materials;

namespace XNAControlGame
{
    /// <summary>
    /// Any methods that are not overrides go here
    /// </summary>
    public partial class Game1 : IDisposable, IHandle<SychronizeBoardMessage>
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
            if (e.Button == MouseButtons.Left)
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

        private void OnPegClicked(CreeperPeg clickedModel)
        {
            if (_SelectedPeg == clickedModel)
            {
                _selectedPeg.Attachments.Remove(_fireEffect);
                _SelectedPeg = null;
            }

            else
            {
                switch (clickedModel.PegType)
                {
                    case CreeperPegType.Fire:
                        goto case CreeperPegType.Ice;
                    case CreeperPegType.Ice:
                        if (_SelectedPeg != null )
                        {
                            _SelectedPeg.Attachments.Remove(_fireEffect);
                        }
                        _SelectedPeg = clickedModel;
                        _SelectedPeg.Attachments.Add(_fireEffect);
                        break;
                    case CreeperPegType.Possible:
                        _eventAggregator.Publish(
                            new MoveMessage()
                            {
                                PlayerType = PlayerType.Human,
                                Type = MoveMessageType.Response,
                                Move = new Move(
                                    _SelectedPeg.Position, clickedModel.Position,
                                    _SelectedPeg.PegType.ToCreeperColor()
                                )
                            }
                         );
                        _SelectedPeg.Attachments.Remove(_fireEffect);
                        _SelectedPeg = null;
                        break;
                }
            }
        }

        private void UpdatePossibleMoves(CreeperPeg clickedPeg)
        {
            ClearPossiblePegs();

            if (clickedPeg != null)
            {
                IEnumerable<Move> possibleMoves = BoardProvider.GetBoard().Pegs.At(clickedPeg.Position).PossibleMoves(BoardProvider.GetBoard());
                foreach (Position position in possibleMoves.Select(x => x.EndPosition))
                {
                    CreeperPeg peg = new CreeperPeg(_possibleModel)
                    {
                        Position = position,
                        PegType = CreeperPegType.Possible,
                    };

                    _boardGroup.Add(peg);
                }
            }
        }

        public void FlipTile(Position position, CreeperColor color)
        {
            Texture2D maskTexture = color.IsFire() ? _fireTileMask : _iceTileMask;

            Rectangle surfaceRect = new Rectangle(0, 0, (int)_boardSurface.Size.X, (int)_boardSurface.Size.Z);

            List<Texture2D> maskTextures = MaterialPaintGroup.GetMaskTextures((MaterialGroup)_boardSurface.Material).OfType<Texture2D>().ToList();
            Texture2D oldMask = maskTextures.First();

            float scale = (oldMask.Width / 6f) / maskTexture.Width;

            Vector2 texPosition = new Vector2(position.Column * (oldMask.Width / 6f), position.Row * (oldMask.Width / 6f))
                + new Vector2(maskTexture.Width * scale / 2);

            RenderTarget2D target = new RenderTarget2D(GraphicsDevice, oldMask.Width, oldMask.Height);

            GraphicsDevice.SetRenderTarget(target);

            SpriteBatch sb = new SpriteBatch(GraphicsDevice);

            sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);

            GraphicsDevice.Clear(Color.Transparent);

            sb.Draw(oldMask,
                Vector2.Zero,
                null,
                Color.White,
                0f,
                Vector2.Zero,
                1f,
                SpriteEffects.None,
                1f);

            sb.Draw(maskTexture,
                texPosition,
                null,
                Color.White,
                0f,
                new Vector2(maskTexture.Width) / 2,
                scale,
                SpriteEffects.None,
                1f);

            sb.End();

            GraphicsDevice.SetRenderTarget(null);

            maskTextures[0] = target;

            MaterialPaintGroup.SetMaskTextures((MaterialGroup)_boardSurface.Material, maskTextures);
        }

        private CreeperPeg GetClickedModel(Vector2 mousePosition)
        {
            Camera camera = _scene.FindName<Camera>("MainCamera");
            Ray selectionRay = GraphicsDevice.Viewport.CreatePickRay((int)mousePosition.X, (int)mousePosition.Y, camera.View, camera.Projection);

            List<CreeperPeg> found = new List<CreeperPeg>();
            _scene.FindAll<CreeperPeg>(ref selectionRay, (x) => x.PegType == CreeperPegType.Possible || x.PegType.ToCreeperColor() == BoardProvider.GetCurrentPlayer().Color, found);
            return found.FirstOrDefault();
        }

        private void LoadViewModels()
        {
            _creeperBoardViewModel = new CreeperBoardViewModel(_scene.FindName<Surface>("boardSurface").Heightmap.Height, _scene.FindName<Surface>("boardSurface").Heightmap.Width, _scene.FindName<Surface>("boardSurface").Heightmap.Step);
        }

        Texture2D _boardTexture;
        Surface _boardSurface;

        private void OnContentLoaded()
        {
            _boardGroup = _scene.FindName<Group>(Resources.ElementNames.BoardGroup);
            _boardSurface = _boardGroup.Find<Surface>();
            _boardTexture = _boardSurface.Material.Texture;

            LoadViewModels();
            LoadPegModels();
        }

        private void LoadPegModels()
        {
            foreach (Piece piece in BoardProvider.GetBoard().Pegs.Where(x => x.Color.IsTeamColor()))
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

        public void Handle(SychronizeBoardMessage message)
        {
            //throw new NotImplementedException("Undo functionality does not exist in Game1.UserMethods: Handle(SychronizedBoardMessage)");
            //remove all pegs
            _pegs.Apply(x => _boardGroup.Children.Remove(x));

            //add all pegs
            LoadPegModels();
        }
    }
}
