using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MGL;
using MGL.Graphics;
using Chess;
using System;

namespace ChessGame
{
    public class ChessGame : Game
    {
        public const int GAME_WIDTH = 1920;
        public const int GAME_HEIGHT = 1080;
        public readonly float _gameAspectRaio;

        private GraphicsDeviceManager _graphics;
        private Screen _screen;
        private Camera _camera;
        private Sprites _sprites;
        private Shapes _shapes;

        private BoardScheme _scheme;
        private Texture2D _lightTexture;
        private Texture2D _darkTexture;

        private int _tileSize;
        private int _x0;
        private int _y0;

        private Board _board;
        private Piece _clickedPiece;
        private GameState _gameState;

        Texture2D _whiteRook;
        Texture2D _whiteKnight;
        Texture2D _whiteBishop;
        Texture2D _whiteQueen;
        Texture2D _whiteKing;
        Texture2D _whitePawn;
        Texture2D _blackRook;
        Texture2D _blackKnight;
        Texture2D _blackBishop;
        Texture2D _blackQueen;
        Texture2D _blackKing;
        Texture2D _blackPawn;
      
        public enum BoardScheme
        {
            Brown,
            Gray
        }

        public ChessGame()
        {
            _gameAspectRaio = (float)GAME_WIDTH / GAME_HEIGHT;

            _graphics = new GraphicsDeviceManager(this);
            _graphics.SynchronizeWithVerticalRetrace = true;

            Content.RootDirectory = "Content";

            IsMouseVisible = true;
            IsFixedTimeStep = true;
        }

        protected override void Initialize()
        {
            // Util.SetRelativeBackBufferSize(_graphics, 0.8f);
            TempLib.SetRelativeBackBufferSize_Temp(_graphics, 0.8f, _gameAspectRaio);

            _screen = new Screen(this, GAME_WIDTH, GAME_HEIGHT);
            _camera = new Camera(_screen);
            _sprites = new Sprites(this);
            _shapes = new Shapes(this);
            _board = new Board();
            _gameState = GameState.Active;

            _scheme = BoardScheme.Brown;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            LoadTileContent();

            _whiteRook = Content.Load<Texture2D>("Pieces/w_rook");
            _whiteKnight = Content.Load<Texture2D>("Pieces/w_knight");
            _whiteBishop = Content.Load<Texture2D>("Pieces/w_bishop");
            _whiteQueen = Content.Load<Texture2D>("Pieces/w_queen");
            _whiteKing = Content.Load<Texture2D>("Pieces/w_king");
            _whitePawn = Content.Load<Texture2D>("Pieces/w_pawn");
            _blackRook = Content.Load<Texture2D>("Pieces/b_rook");
            _blackKnight = Content.Load<Texture2D>("Pieces/b_knight");
            _blackBishop = Content.Load<Texture2D>("Pieces/b_bishop");
            _blackQueen = Content.Load<Texture2D>("Pieces/b_queen");
            _blackKing = Content.Load<Texture2D>("Pieces/b_king");
            _blackPawn = Content.Load<Texture2D>("Pieces/b_pawn");
        }

        protected override void Update(GameTime gameTime)
        {
            MGL.Input.Keyboard keyboard = MGL.Input.Keyboard.Instance;
            keyboard.Update();
            MGL.Input.Mouse mouse = MGL.Input.Mouse.Instance;
            mouse.Update();

            if (keyboard.IsKeyClicked(Keys.Escape))
                Exit();

            if (keyboard.IsKeyClicked(Keys.F11))
                MGL.Util.ToggleFullScreen(_graphics);

            _tileSize = _screen.Height / 11;
            _x0 = -4 * _tileSize;
            _y0 = -4 * _tileSize;

         
            Vector2 mousePos = mouse.GetScreenPosition(_screen);
            int row = 7 - (int)Math.Floor((mousePos.Y - _screen.Height / 2 + 4 * _tileSize) / _tileSize);
            int col = (int)Math.Floor((mousePos.X - _screen.Width / 2 + 4 * _tileSize) / _tileSize);

            if (IsActivePieceHovered(mousePos, row, col))
            {
                Mouse.SetCursor(MouseCursor.Hand);
                if (mouse.IsLeftButtonClicked())
                {
                    foreach (Piece piece in _board.Pieces)
                    {
                        if (piece.square == Chess.Util.GetSquare(row, col))
                        {
                            _clickedPiece = _clickedPiece == piece ? null : piece;
                            break;
                        }
                    }
                }
            }
            else if (_clickedPiece != null && IsTargetSquareHovered(mousePos, row, col))
            {
                Mouse.SetCursor(MouseCursor.Hand);
                if (mouse.IsLeftButtonClicked())
                {
                    _gameState = _board.MakeMove(_clickedPiece, Chess.Util.GetSquare(row, col));
                    _clickedPiece = null;
                }
            }
            else
                Mouse.SetCursor(MouseCursor.Arrow);

            if (_gameState != GameState.Active)
            {
                System.Diagnostics.Debug.WriteLine(_gameState.ToString());
            }


            base.Update(gameTime);
        }

        private bool IsActivePieceHovered(Vector2 mousePos, int row, int col)
        {
            if (row >= 0 && row < 8 && col >= 0 && col < 8 && _board.BoardState[row, col] != "")
            {
                string pieceName = _board.BoardState[row, col];

                if (pieceName == "" || (_board.ActiveColor == PieceColor.White && char.IsLower(char.Parse(pieceName))) || (_board.ActiveColor == PieceColor.Black && char.IsUpper(char.Parse(pieceName))))
                    return false;

                int height = (int)(0.8f * _tileSize);
                if (pieceName == "p" || pieceName == "P")
                    height = (int)(0.7f * _tileSize);

                int tileX = (int)Math.Floor(mousePos.X - _screen.Width / 2 + (4 - col) * _tileSize);
                int tileY = _tileSize - (int)Math.Floor(mousePos.Y - _screen.Height / 2 + (4 - 7 + row) * _tileSize);

                if (tileX > (_tileSize - height) / 2 && tileX < (_tileSize + height) / 2 && tileY > (_tileSize - height) / 2 && tileY < (_tileSize + height) / 2)
                    return true;
            }

            return false;
        }

        private bool IsTargetSquareHovered(Vector2 mousePos, int row, int col)
        {
            if (row >= 0 && row < 8 && col >= 0 && col < 8)
            {
                foreach (string square in _clickedPiece.possibleMoves)
                {
                    int[] arrayID = Chess.Util.GetArrayIndex(square);

                    if (arrayID[0] == row && arrayID[1] == col)
                        return true;
                }
            }

            return false;
        }

        protected override void Draw(GameTime gameTime)
        {
            _screen.Set();
            GraphicsDevice.Clear(Color.Gray);

            _sprites.Begin(_camera, false);
            DrawBoard();
            DrawPieces();
            _sprites.End();  
     
            _screen.UnSet();
            _screen.Present(_sprites);

            base.Draw(gameTime);
        }

        private void DrawBoard()
        {
            _shapes.Begin(_camera);
            _shapes.DrawRectangleFilled(Vector2.Zero, (float)_tileSize * 9.5f, (float)_tileSize * 9.5f, Color.DimGray);
            // _shapes.End();

            // _sprites.DrawString(_testFont, (char)(x + 65), pos, Color.White);
            for (int x = 0; x < 8; x++)
            {
                Vector2 pos = new Vector2(_x0 + (x + 0.5f) * _tileSize, _y0 + 8.5f * _tileSize);
                _shapes.DrawPoint(pos, Shapes.PointShape.Circle, Color.Red);

                pos = new Vector2(_x0 + (x + 0.5f) * _tileSize, _y0 - 0.5f * _tileSize);
                _shapes.DrawPoint(pos, Shapes.PointShape.FilledCircle, Color.Red);
            }
            for (int y = 0; y < 8; y++)
            {
                Vector2 pos = new Vector2(_x0 + 8.5f * _tileSize, _y0 + (7 - y + 0.5f) * _tileSize);
                _shapes.DrawPoint(pos, Shapes.PointShape.Diamond, Color.Red);

                pos = new Vector2(_x0 - 0.5f * _tileSize, _y0 + (7 - y + 0.5f) * _tileSize);
                _shapes.DrawPoint(pos, Shapes.PointShape.FilledDiamond, Color.Red);
            }
            _shapes.End();

            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    Texture2D sprite;
                    if ((x % 2 == 0 && y % 2 == 0) || (x % 2 != 0 && y % 2 != 0))
                        sprite = _lightTexture;
                    else
                        sprite = _darkTexture;

                    Point pos = new Point(_x0 + x * _tileSize, _y0 + (7 - y) * _tileSize);
                    Rectangle destRectangle = new Rectangle(pos, new Point(_tileSize, _tileSize));
                    _sprites.Draw(sprite, null, destRectangle);
                }
            }

            if (_clickedPiece != null)
            {
                int size = _tileSize / 3;
                foreach (string square in _clickedPiece.possibleMoves)
                {
                    int[] arrayID = Chess.Util.GetArrayIndex(square);
                    
                    Point pos = new Point((int)(_x0 + (arrayID[1] + 0.5f) * _tileSize), (int)(_y0 + (7 - arrayID[0] + 0.5f) * _tileSize));
                    Rectangle destRectangle = new Rectangle(pos, new Point(size, size));
                    _sprites.Draw(_blackRook, null, destRectangle);
                }
            }
        }

        private void DrawPieces()
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (_board.BoardState[i, j] == "")
                        continue;

                    Texture2D sprite;
                    if (_board.BoardState[i, j] == "r")
                        sprite = _blackRook;
                    else if (_board.BoardState[i, j] == "n")
                        sprite = _blackKnight;
                    else if (_board.BoardState[i, j] == "b")
                        sprite = _blackBishop;
                    else if (_board.BoardState[i, j] == "q")
                        sprite = _blackQueen;
                    else if (_board.BoardState[i, j] == "k")
                        sprite = _blackKing;
                    else if (_board.BoardState[i, j] == "p")
                        sprite = _blackPawn;
                    else if (_board.BoardState[i, j] == "R")
                        sprite = _whiteRook;
                    else if (_board.BoardState[i, j] == "N")
                        sprite = _whiteKnight;
                    else if (_board.BoardState[i, j] == "B")
                        sprite = _whiteBishop;
                    else if (_board.BoardState[i, j] == "Q")
                        sprite = _whiteQueen;
                    else if (_board.BoardState[i, j] == "K")
                        sprite = _whiteKing;
                    else
                        sprite = _whitePawn;

                    int height = (int)(0.8f * _tileSize);
                    if (_board.BoardState[i, j] == "p" || _board.BoardState[i, j] == "P")
                        height = (int)(0.7f * _tileSize);

                    float scale = (float)height / sprite.Height;

                    Vector2 pos = new Vector2(_x0 + (j + 0.5f) * _tileSize, _y0 + (7 - i + 0.5f) * _tileSize);
                    _sprites.Draw(sprite, null, new Vector2(sprite.Width / 2, sprite.Height / 2), pos, 0f, new Vector2(scale, scale), Color.White);
                }
            }
        }
            
        public void ChangeBoardScheme(BoardScheme newScheme)
        {
            if (_scheme == newScheme)
                return;

            _scheme = newScheme;
            LoadTileContent();
        }

        private void LoadTileContent()
        {
            if (_scheme == BoardScheme.Brown)
            {
                _lightTexture = Content.Load<Texture2D>("Board/brown_light");
                _darkTexture = Content.Load<Texture2D>("Board/brown_dark");
            }
            else if (_scheme == BoardScheme.Gray)
            {
                _lightTexture = Content.Load<Texture2D>("Board/gray_light");
                _darkTexture = Content.Load<Texture2D>("Board/gray_dark");
            }
        }
    }
}
