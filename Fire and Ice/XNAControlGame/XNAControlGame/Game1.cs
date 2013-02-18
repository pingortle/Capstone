using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nine;
using Nine.Components;
using Nine.Graphics;
using Nine.Graphics.Materials;
using Nine.Graphics.ParticleEffects;
using Nine.Graphics.PostEffects;
using Nine.Graphics.Primitives;
using Nine.Physics;
using Creeper;
using System.Collections.Generic;

namespace XNAControlGame
{
    public class Game1 : XNAControl.XNAControlGame
    {
        Position _startPosition = new Position(-1, -1);
        Position _endPostion = new Position(-1, -1);
        String _selectedPeg;
        Scene _scene;
        Input _input;
        SpriteFont _font;
        Matrix _cameraView;
        Matrix _cameraProj;
        public CreeperBoard Board { get; set; }
        Vector3 _modelScale = new Vector3( 6, 6, 6 );
        bool _secondClick = false;
        CreeperColor PlayerTurn = CreeperColor.White;
        Texture2D _blankTile;
        Texture2D _whiteTile;
        Texture2D _blackTile;
        private Texture2D _whitePeg;
        private Texture2D _blackPeg;
        private Texture2D _hightlightPeg;
        private bool _isMyTurnToMakeAMove = false;
        public Move LastMoveMade { get; private set; }
        List<Move> possible = new List<Move>();


        public Move GetMove(CreeperColor currentTurn)
        {
            _isMyTurnToMakeAMove = true;
            PlayerTurn = currentTurn;

            while (_isMyTurnToMakeAMove) ;

            return LastMoveMade;
        }

        /// <summary>
        /// Convert a peg number a board position (row and column).
        /// </summary>
        static public Position NumberToPosition(int number)
        {
            Position position = new Position();

            if (number >= CreeperBoard.PegRows - 1)
            {
                number++;
            }

            if (number >= (CreeperBoard.PegRows - 1) * CreeperBoard.PegRows)
            {
                number++;
            }

            position.Row = (int)number / CreeperBoard.PegRows;
            position.Column = number % CreeperBoard.PegRows;

            return position;
        }

        public Game1(IntPtr handle, int width, int height) : base(handle, "Content", width, height)
        {
            Content = new ContentLoader(Services);
 
            Components.Add(new InputComponent(handle));
            _input = new Input();

            _input.MouseDown += new EventHandler<Nine.MouseEventArgs>(Input_MouseDown);
        }

        

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Load the peg model
            Microsoft.Xna.Framework.Graphics.Model pegModel = Content.Load<Microsoft.Xna.Framework.Graphics.Model>("Model/sphere");
            // Load the Tile sprite.
            Sprite tile = new Sprite(GraphicsDevice);
            
            _blankTile = Content.Load<Texture2D>("square");
            _whiteTile = Content.Load<Texture2D>("whiteTile");
            _blackTile = Content.Load<Texture2D>("blackTile");

            // Load a scene from a content file
            _scene = Content.Load<Scene>("Scene1");

            _scene.FindName<FreeCamera>("theCamera").Position = new Vector3(0, (float)-470.27, (float)249.5);
            _scene.FindName<FreeCamera>("theCamera").Angle = new Vector3((float)-1.13, 0, 0);
            //Find all of the dimensions of the board to determine where the peg models need to be placed in relation to the middle of the board.
            float boardHeight, boardWidth, squareWidth, squareHeight;
            boardHeight = _scene.FindName<Sprite>("boardImage").Texture.Height;
            boardWidth = _scene.FindName<Sprite>("boardImage").Texture.Width;
            squareWidth = boardWidth /6;
            squareHeight = boardHeight / 6;
            Vector3 startCoordinates = new Vector3(-boardWidth / 2, boardHeight / 2, 0);
            Position pegPosition;

            _font = Content.Load<SpriteFont>("defaultFont");
            //Create a Nine Model peg with a XNA model, set its properties, and place it in its correct location relative to the middle of the board.
            //Do this for ever possible peg location on the board.
            for (int pegNumber = 1; pegNumber < 46; pegNumber++)
            {
                BasicMaterial defaultMaterial = new BasicMaterial(GraphicsDevice);
                defaultMaterial.Texture = Content.Load<Texture2D>("Textures/Grid");
                pegPosition = NumberToPosition(pegNumber);
                String pegName = 'p' + pegPosition.Row.ToString() + 'x' + pegPosition.Column.ToString();
                Vector3 pegCoordinates = new Vector3(startCoordinates.X + squareWidth * pegPosition.Column, startCoordinates.Y - squareHeight * pegPosition.Row, 0);
                _scene.Add(new Nine.Graphics.Model(pegModel) { Transform = Matrix.CreateScale( _modelScale ) * Matrix.CreateTranslation(pegCoordinates), Name = pegName, Material = defaultMaterial });
            }
            //Place a transparent sprite for tiles in every possible tile position.
            startCoordinates += new Vector3(squareWidth / 2, -(squareHeight / 2), 0);
            for (int tileNumber = 0; tileNumber < 35; tileNumber++)
            {
                Position tilePosition = CreeperUtility.NumberToPosition(tileNumber);
                String tileName = 't' + tilePosition.Row.ToString() + 'x' + tilePosition.Column.ToString();
                tile.Position = new Vector2(startCoordinates.X + squareWidth * tilePosition.Column, startCoordinates.Y - squareHeight * tilePosition.Row);
                tile.ZOrder = 1;
                tile.Name = tileName;
                tile.Texture = _blankTile;
                _scene.Add(tile);
                tile = new Sprite(GraphicsDevice);
            }


            
            base.LoadContent();
        }

        /// <summary>
        /// Handle mouse input events
        /// </summary>
        private void Input_MouseDown(object sender, Nine.MouseEventArgs e)
        {
            if (_isMyTurnToMakeAMove)
            {
                //Create a ray fired from the point of click point.
                Ray pickRay = GetSelectionRay(new Vector2(e.X, e.Y));
                float maxDistance = float.MaxValue;

                //Test all 45 locations to see if one was click. If one was, determine which one was clicked.
                Position pegLocation;
                bool intersectionNotFound = true;
                for (int pegNum = 1; pegNum <= 45 && intersectionNotFound; pegNum++)
                {
                    pegLocation = NumberToPosition(pegNum);
                    String currentPeg = 'p' + pegLocation.Row.ToString() + 'x' + pegLocation.Column.ToString();
                    BoundingBox modelIntersect = new BoundingBox(_scene.FindName<Nine.Graphics.Model>(currentPeg).BoundingBox.Min,
                        _scene.FindName<Nine.Graphics.Model>(currentPeg).BoundingBox.Max);
                    Nullable<float> intersect = pickRay.Intersects(modelIntersect);

                    //Selection Logic

                    //If a model was selected
                    if (intersect.HasValue == true)
                    {
                        intersectionNotFound = false;

                        if (intersect.Value < maxDistance)
                        {
                            //And if the peg to move has not been selected or the peg clicked matches the current turn
                            if (!_secondClick || Board.Pegs.At(pegLocation).Color == PlayerTurn)
                            {
                                _selectedPeg = currentPeg;
                                _startPosition = new Position(Convert.ToInt32(currentPeg[1] - '0'), Convert.ToInt32(currentPeg[3] - '0'));
                                _secondClick = true;
                                if (Board.Pegs.At(pegLocation).Color == PlayerTurn)
                                {
                                    possible = CreeperUtility.PossibleMoves(Board.Pegs.At(_startPosition), Board).ToList();
                                }
                            }
                            //Otherwise the end point of the move is being selected
                            else
                            {
                                //Check to see if the location being selected is an empty peg location. It must be so to be moved to.
                                if (Board.Pegs.At(pegLocation).Color == CreeperColor.Empty && _secondClick)
                                {
                                    _endPostion = new Position(Convert.ToInt32(currentPeg[1] - '0'), Convert.ToInt32(currentPeg[3] - '0'));
                                }
                                //If it isn't, deselect the peg
                                else
                                {
                                    _startPosition = _endPostion = new Position(-1, -1);
                                    _selectedPeg = "";
                                }
                                _secondClick = false;
                                possible.Clear();
                            }
                        }
                    }
                }

                //If both the start and end position have been determined, send the move to the board and reset start and end.
                if (_endPostion.Row != -1)
                {
                    Move move = new Move(_startPosition, _endPostion, PlayerTurn);

                    if (_scene.FindName<Nine.Graphics.Model>(_selectedPeg).Visible == true)
                    {
                        if (Board.IsValidMove(move))
                        {
                            LastMoveMade = move;
                            _isMyTurnToMakeAMove = false;
                        }
                    }

                    _startPosition = new Position(-1, -1);
                    _endPostion = new Position(-1, -1);
                    _secondClick = false;
                    possible.Clear();
                    _selectedPeg = "";
                }
            }           
        }

        /// <summary>
        /// Returns a ray fired from the click point to test for intersection with a model.
        /// </summary>
        Ray GetSelectionRay( Vector2 mouseCoor )
        {
            Vector3 nearsource = new Vector3(mouseCoor, 0f);
            Vector3 farsource = new Vector3(mouseCoor, 1f);

            Matrix world = Matrix.CreateTranslation(0, 0, 0);

            Vector3 nearPoint = GraphicsDevice.Viewport.Unproject(nearsource, _cameraProj, _cameraView, world);

            Vector3 farPoint = GraphicsDevice.Viewport.Unproject(farsource, _cameraProj, _cameraView, world);

            // Create a ray from the near clip plane to the far clip plane.
            Vector3 direction = farPoint - nearPoint;
            direction.Normalize();
            Ray pickRay = new Ray(nearPoint, direction);

            return pickRay;
        }

        /// <summary>
        /// This is called when the game should update itself.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            _scene.Update(gameTime.ElapsedGameTime);
            _scene.UpdatePhysicsAsync(gameTime.ElapsedGameTime);
            _cameraView = _scene.Find<FreeCamera>().View;
            _cameraProj = _scene.Find<FreeCamera>().Projection;
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            DrawBoard();
            _scene.Draw(GraphicsDevice, gameTime.ElapsedGameTime);

            //Test String drawing
            SpriteBatch spritebatch = new SpriteBatch(GraphicsDevice);
            spritebatch.Begin();
            spritebatch.DrawString(_font, "Player Turn = " + PlayerTurn.ToString(), new Vector2(0, 0), Color.Black);
            spritebatch.DrawString(_font, "CameraPosition = (" + _scene.FindName<FreeCamera>("theCamera").Position.X.ToString() + ","
                + _scene.FindName<FreeCamera>("theCamera").Position.Y.ToString() + ","
                + _scene.FindName<FreeCamera>("theCamera").Position.Z.ToString() + ")"
            , new Vector2(0, 25), Color.Black);
            spritebatch.DrawString(_font, "CameraAngle = (" + _scene.FindName<FreeCamera>("theCamera").Angle.X.ToString() + ","
                + _scene.FindName<FreeCamera>("theCamera").Angle.Y.ToString() + ","
                + _scene.FindName<FreeCamera>("theCamera").Angle.Z.ToString() + ")"
            , new Vector2(0, 50), Color.Black);
            
            spritebatch.End();
                
            base.Draw(gameTime);
        }

        /// <summary>
        /// Determine what the display properties of each board location should and set them.
        /// </summary>
        private void DrawBoard()
        {
            string location;

            BasicMaterial black = new BasicMaterial(GraphicsDevice);
            black.DiffuseColor = new Vector3(0, 0, 0);
            BasicMaterial white = new BasicMaterial(GraphicsDevice);
            white.DiffuseColor = new Vector3(255, 255, 255);
            BasicMaterial yellow = new BasicMaterial(GraphicsDevice);
            yellow.DiffuseColor = new Vector3(255, 255, 0);

            for (int r = 0; r < CreeperBoard.PegRows; r++)
            {
                for (int c = 0; c < CreeperBoard.PegRows; c++)
                {
                    location = 'p' + r.ToString() + 'x' + c.ToString();

                    if (Board.Pegs.At(new Position(r, c)).Color == CreeperColor.White)
                    {
                        _scene.FindName<Nine.Graphics.Model>(location).Visible = true;
                        _scene.FindName<Nine.Graphics.Model>(location).Material = white;
                    }
                    else if (Board.Pegs.At(new Position(r, c)).Color == CreeperColor.Black)
                    {
                        _scene.FindName<Nine.Graphics.Model>(location).Visible = true;
                        _scene.FindName<Nine.Graphics.Model>(location).Material = black;
                    }
                    else
                    {
                        if (_scene.FindName<Nine.Graphics.Model>(location) != null)
                        {
                            _scene.FindName<Nine.Graphics.Model>(location).Visible = false;
                        }
                    }
                }
            }


            foreach (Move move in possible)
            {
               if (move.EndPosition.Row != null)
                {
                    location = 'p' + move.EndPosition.Row.ToString() + 'x' + move.EndPosition.Column.ToString();
                    _scene.FindName<Nine.Graphics.Model>(location).Visible = true;
                    _scene.FindName<Nine.Graphics.Model>(location).Material = yellow;
                }
            }

            for (int r = 0; r < CreeperBoard.TileRows; r++)
            {
                for (int c = 0; c < CreeperBoard.TileRows; c++)
                {
                    location = 't' + r.ToString() + 'x' + c.ToString();

                    if (Board.Tiles.At(new Position(r, c)).Color == CreeperColor.White)
                    {
                        _scene.FindName<Sprite>(location).Texture = _whiteTile;
                    }
                    else if (Board.Tiles.At(new Position(r, c)).Color == CreeperColor.Black)
                    {
                        _scene.FindName<Sprite>(location).Texture = _blackTile;
                    }
                    else
                    {
                        if (_scene.FindName<Sprite>(location) != null)
                        {
                            _scene.FindName<Sprite>(location).Texture = _blankTile;
                        }
                    }
                }
            }
        }
    }
}