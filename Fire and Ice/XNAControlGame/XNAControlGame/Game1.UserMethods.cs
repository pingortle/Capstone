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
using Microsoft.Xna.Framework.Content;

namespace XNAControlGame
{
    /// <summary>
    /// Any methods that are not overrides go here
    /// </summary>
    public partial class Game1 : IDisposable, IHandle<SychronizeBoardMessage>
    {
        private void LoadViewModels()
        {
            _creeperBoardViewModel = new CreeperBoardViewModel(_scene.FindName<Surface>("boardSurface").Heightmap.Height, _scene.FindName<Surface>("boardSurface").Heightmap.Width, _scene.FindName<Surface>("boardSurface").Heightmap.Step);
        }

        MaterialGroup _boardMaterial;
        Surface _boardSurface;

        private void OnContentLoaded()
        {
            LoadViewModels();

            _boardGroup = _scene.FindName<Group>(Resources.ElementNames.BoardGroup);
            _boardGroup.Add(_boardController = new BoardController(_eventAggregator)
            {
                FlipTile = FlipTile,
                BoardProvider = BoardProvider,
                ViewModel = _creeperBoardViewModel,
                PublishMove = (move) =>
                {
                    _eventAggregator.Publish(new MoveMessage()
                    {
                        Board = BoardProvider.GetBoard(),
                        Move = move,
                        PlayerType = PlayerType.Human,
                        Type = MoveMessageType.Response,
                        TurnColor = BoardProvider.GetCurrentPlayer().Color,
                    });
                }
            });
            _eventAggregator.Subscribe(_boardController);
            _moveAnimationListener.BoardController = _boardController;
            _boardGroup.Add(_moveAnimationListener);
            _boardSurface = _boardGroup.Find<Surface>();
            _boardMaterial = (MaterialGroup)_boardSurface.Material;

            _input.MouseDown += new EventHandler<Nine.MouseEventArgs>((s, e) =>
            {
                if (BoardProvider.GetCurrentPlayer().Type == PlayerType.Human && !_moveAnimationListener.IsAnimating)
                {
                    _boardController.DetectFullClick(e);
                }
            });

            LoadPegModels();

            _eventAggregator.Publish(new ComponentInitializedMessage() { Component = InitComponent.Content, });
        }

        private void LoadPegModels()
        {
            foreach (Piece piece in BoardProvider.GetBoard().Pegs.Where(x => x.Color.IsTeamColor()))
            {
                int InitialDegreeRotaion = 0;
                if (piece.Position.Row == 0)
                {
                    InitialDegreeRotaion = 180;
                }
                if (piece.Position.Column == 0)
                {
                    InitialDegreeRotaion = 270;
                }
                if (piece.Position.Column == 6)
                {
                    InitialDegreeRotaion = 90;
                }

                if (piece.Color == CreeperColor.Fire)
                {
                    _fireGroup = _fireModel1.CreateInstance<Group>(Content.ServiceProvider);
                    _fireGroup.Transform = Matrix.CreateRotationY( MathHelper.ToRadians(InitialDegreeRotaion) ) * Matrix.CreateTranslation(_creeperBoardViewModel.GraphicalPositions[piece.Position.Row, piece.Position.Column]);
                    _fireGroup.Add(new PegController() { Position = new Position(piece.Position), PegType = CreeperPegType.Fire, });
                    _scene.Add(_fireGroup);
                }
                else
                {
                    _iceGroup = _iceModel1.CreateInstance<Group>(Content.ServiceProvider);
                    _iceGroup.Transform = Matrix.CreateRotationY(MathHelper.ToRadians(InitialDegreeRotaion)) * Matrix.CreateTranslation(_creeperBoardViewModel.GraphicalPositions[piece.Position.Row, piece.Position.Column]);
                    _iceGroup.Add(new PegController() { Position = new Position(piece.Position), PegType = CreeperPegType.Ice, });
                    _scene.Add(_iceGroup);
                }
            }
        }

        #region NittyGrittyTileFlippingCode
        private void FlipTile(Position position, CreeperColor color)
        {
            if (BoardProvider.GetBoard().Tiles.At(position).Color != CreeperColor.Invalid)
            {
                Texture2D maskTexture = color.IsFire() ? _fireTileMask : _iceTileMask;

                Rectangle surfaceRect = new Rectangle(0, 0, (int)_boardSurface.Size.X, (int)_boardSurface.Size.Z);

                List<Texture2D> maskTextures = MaterialPaintGroup.GetMaskTextures(_boardMaterial).OfType<Texture2D>().ToList();
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

                maskTextures[0].Dispose();
                maskTextures[0] = target;

                MaterialPaintGroup.SetMaskTextures(_boardMaterial, maskTextures);
            }
        }

        public void SynchronizeTiles(CreeperBoard board)
        {
            Rectangle surfaceRect = new Rectangle(0, 0, (int)_boardSurface.Size.X, (int)_boardSurface.Size.Z);

            List<Texture2D> maskTextures = MaterialPaintGroup.GetMaskTextures(_boardMaterial).OfType<Texture2D>().ToList();

            RenderTarget2D target = new RenderTarget2D(GraphicsDevice, maskTextures[0].Width, maskTextures[0].Height);

            GraphicsDevice.SetRenderTarget(target);

            SpriteBatch sb = new SpriteBatch(GraphicsDevice);

            sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);

            GraphicsDevice.Clear(new Color(1f, 0f, 0f, 0f));

            foreach (Piece piece in board.Tiles.Where((x) => x.Color.IsTeamColor()))
            {
                Texture2D maskTexture = piece.Color.IsFire() ? _fireTileMask : _iceTileMask;
                float scale = (target.Width / 6f) / maskTexture.Width;
                Vector2 position = new Vector2(piece.Position.Column * (target.Width / 6f), piece.Position.Row * (target.Width / 6f))
                    + new Vector2(maskTexture.Width * scale / 2);

                sb.Draw(maskTexture,
                    position,
                    null,
                    Color.White,
                    0f,
                    new Vector2(maskTexture.Width) / 2,
                    scale,
                    SpriteEffects.None,
                    1f);
            }

            sb.End();

            GraphicsDevice.SetRenderTarget(null);

            maskTextures[0].Dispose();
            maskTextures[0] = target;

            MaterialPaintGroup.SetMaskTextures(_boardMaterial, maskTextures);
        }
        #endregion

        public void Handle(SychronizeBoardMessage message)
        {
            //throw new NotImplementedException("Undo functionality does not exist in Game1.UserMethods: Handle(SychronizedBoardMessage)");
            //remove all pegs
            //_boardGroup.Children.Apply(x => _boardGroup.Children.Remove(x));
            _boardController.SynchronizePegs(message.Board);

            //add all pegs
            LoadPegModels();
            SynchronizeTiles(message.Board);
            if (message.Callback != null)
            {
                message.Callback();
            }
        }
    }
}
