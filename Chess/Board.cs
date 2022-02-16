using System;
using System.Collections.Generic;
using System.Data;

namespace Chess
{
    public sealed class Board
    {
        private string _name;
        private string[,] _boardState;
        private PieceColor _activeColor;
        private AvailableCastle _whiteAvailableCastle;
        private AvailableCastle _blackAvailableCastle;
        private string _enPassantTarget;
        private int _halfMoveCounter;
        private int _moveNumber;

        private List<Piece> _pieces;
        private List<string> _possibleMoves;

        private DataTable _movesMade;

        private string _FEN;
        private List<string> _FENList;

        public string[,] BoardState { get { return _boardState; } set { _boardState = value; } }
        public PieceColor ActiveColor { get { return _activeColor; } }
        public AvailableCastle AvailableCastles { get { return _activeColor == PieceColor.White ? _whiteAvailableCastle : _blackAvailableCastle; } }
        public AvailableCastle WhiteAvailableCastle { get { return _whiteAvailableCastle; } }
        public AvailableCastle BlackAvailableCastle { get { return _blackAvailableCastle; } }
        public string EnPassantTarget { get { return _enPassantTarget; } }
        public int HalfMoveCounter { get { return _halfMoveCounter; } }
        public int MoveNumber { get { return _moveNumber; } }

        public List<Piece> Pieces { get { return _pieces; } }
        public string FEN { get { return _FEN; } }

        public Board()
        {
            _name = "Game";    // TODO: Generate name

            //  Pawn = "P", Knight = "N", Bishop = "B", Rook = "R", Queen = "Q", King = "K"
            //  White -> upper-case ("PNBRQK"), Black -> lowercase ("pnbrqk")
            _boardState = new string[8, 8] {
                { "r", "n", "b", "q", "k", "b", "n", "r" },
                { "p", "p", "p", "p", "p", "p", "p", "p" },
                { "",  "",  "",  "",  "",  "",  "",  ""  },
                { "",  "",  "",  "",  "",  "",  "",  ""  },
                { "",  "",  "",  "",  "",  "",  "",  ""  },
                { "",  "",  "",  "",  "",  "",  "",  ""  },
                { "P", "P", "P", "P", "P", "P", "P", "P" },
                { "R", "N", "B", "Q", "K", "B", "N", "R" },
            };

            _activeColor = PieceColor.White;

            _whiteAvailableCastle = AvailableCastle.Both;
            _blackAvailableCastle = AvailableCastle.Both;

            // If a pawn has just made a two-square move, this is the position "behind" the pawn. Otherwise, this is "-".
            _enPassantTarget = "-";

            // The number of halfmoves since the last capture or pawn advance (used for the fifty-move rule).
            _halfMoveCounter = 0;

            // Starts at 1 and is incremented after Black's move.
            _moveNumber = 1;

            _FENList = new List<string>();
            _FEN = WriteFEN();
            _FENList.Add(_FEN);

            _pieces = Util.PopulatePiecesList(_boardState);

            CalculateAvailableMoves();

            _movesMade = new DataTable("Move list");
            _movesMade.Columns.Add("White", Type.GetType("System.String"));
            _movesMade.Columns.Add("Black", Type.GetType("System.String"));
        }

        public Board(string FEN, List<string> FENList = null, bool forTest = false)
        {
            ReadFEN(FEN);

            _FENList = FENList ?? new List<string>();
            _FEN = WriteFEN();
            _FENList.Add(_FEN);

            _pieces = Util.PopulatePiecesList(_boardState);

            if (!forTest)
            {
                CalculateAvailableMoves();

                _movesMade = new DataTable("Move list");
                _movesMade.Columns.Add("White", Type.GetType("System.String"));
                _movesMade.Columns.Add("Black", Type.GetType("System.String"));
            }
        }

        public void CalculateAvailableMoves()
        {
            _possibleMoves = new List<string>();

            foreach (var piece in _pieces)
            {
                if (piece.color != _activeColor)
                    continue;

                List<string> possibleMoves = Moves.GetAvailableMoves(this, piece);

                piece.possibleMoves = possibleMoves;
                _possibleMoves.AddRange(possibleMoves);
            }
        }

        public void TestMove(Piece piece, string square)
        {
            int[] oldArrayID = Util.GetArrayIndex(piece.square);
            int[] newArrayID = Util.GetArrayIndex(square);

            // TODO: If move is castle -> check that the king, or its castle path, is not in check

            UpdateBoardState(oldArrayID[0], oldArrayID[1], newArrayID[0], newArrayID[1], piece, square);

            _pieces = Util.PopulatePiecesList(_boardState);
        }

        public GameState MakeMove(Piece piece, int row, int col)
        {
            string square = Util.GetSquare(new int[] { row, col });
            return MakeMove(piece, square);
        }

        public GameState MakeMove(Piece piece, string square)
        {
            int[] oldArrayID = Util.GetArrayIndex(piece.square);
            int[] newArrayID = Util.GetArrayIndex(square);

            // Promote
            if (piece.type == PieceType.Pawn && (newArrayID[0] == 0 || newArrayID[0] == 7))
                return GameState.Castle;
            // TODO: Implement how the user can choose between pieces
            
                //if (_activeColor == PieceColor.White)
                //    piece = new Piece(_activeColor, PieceType.Queen, "Q", square);
                //else
                //    piece = new Piece(_activeColor, PieceType.Queen, "q", square);

            UpdateBoardState(oldArrayID[0], oldArrayID[1], newArrayID[0], newArrayID[1], piece, square);

            RegisterMove(piece, square);

            _activeColor = _activeColor == PieceColor.White ? PieceColor.Black : PieceColor.White;
            _enPassantTarget = "-";
            _halfMoveCounter++;

            if (piece.type == PieceType.Pawn)
            {
                if (Math.Abs(newArrayID[0] - oldArrayID[0]) == 2)
                {
                    if (_activeColor == PieceColor.White)
                        _enPassantTarget = Util.GetSquare(new int[] { newArrayID[0] - 1, newArrayID[1] });
                    else
                        _enPassantTarget = Util.GetSquare(new int[] { newArrayID[0] + 1, newArrayID[1] });
                }
                _halfMoveCounter = 0;
            } 

            if (_activeColor == PieceColor.White)
                _moveNumber++;

            _FEN = WriteFEN();
            _FENList.Add(_FEN);

            _pieces = Util.PopulatePiecesList(_boardState);

            CalculateAvailableMoves();

            return CheckGameState();
        }

        public void UpdateBoardState(int oldRow, int oldCol, int newRow, int newCol, Piece piece, string square)
        {
            _boardState[oldRow, oldCol] = "";
            _boardState[newRow, newCol] = piece.shortName;

            // En passant
            if (piece.type == PieceType.Pawn && square == _enPassantTarget)
            {
                if (_activeColor == PieceColor.White)
                    _boardState[oldRow + 1, oldCol] = "";
                else
                    _boardState[oldRow - 1, oldCol] = "";
            }
            // Castle
            else if (piece.type == PieceType.King && Math.Abs(oldCol - newCol) > 1)
            {
                if (_activeColor == PieceColor.White)
                {
                    _whiteAvailableCastle = AvailableCastle.None;

                    if (oldCol - newCol > 0)
                    {
                        _boardState[7, 7] = "";
                        _boardState[7, 5] = "R";
                    }
                    else
                    {
                        _boardState[7, 0] = "";
                        _boardState[7, 3] = "R";
                    }
                }
                else
                {
                    _blackAvailableCastle = AvailableCastle.None;

                    if (oldCol - newCol > 0)
                    {
                        _boardState[0, 7] = "";
                        _boardState[0, 5] = "r";
                    }
                    else
                    {
                        _boardState[0, 0] = "";
                        _boardState[0, 3] = "r";
                    }
                }
            }
        }

        public GameState CheckGameState()
        {
            if (_possibleMoves.Count == 0)
            {
                if (Moves.IsKingInCheck(this))
                    return GameState.Checkmate;

                return GameState.Patt;
            }

            return GameState.Active;
        }

        private void RegisterMove(Piece piece, string square)
        {
            // TODO: Implement
        }

        public void SaveMovesMade(string savePath)
        {
            // TODO: Implement
        }

        // -------------------------------------------- FEN notation --------------------------------------------
        // https://en.wikipedia.org/wiki/Forsyth%E2%80%93Edwards_Notation
        private string WriteFEN()
        {
            string FEN = "";

            for (int i = 0; i < 8; i++)
            {
                int emptySquares = 0;
                for (int j = 0; j < 8; j++)
                {
                    if (_boardState[i, j] != "")
                    {
                        if (emptySquares > 0)
                            FEN += emptySquares;
                        FEN += _boardState[i, j];
                        emptySquares = 0;
                    }
                    else
                        emptySquares++;

                    if (j == 7 && emptySquares > 0)
                        FEN += emptySquares;
                }

                FEN += i == 7 ? " " : "/";
            }

            FEN += _activeColor == PieceColor.White ? "w " : "b ";

            if (_whiteAvailableCastle == AvailableCastle.None && _blackAvailableCastle == AvailableCastle.None)
                FEN += "-";
            else
            {
                if (_whiteAvailableCastle == AvailableCastle.Both || _whiteAvailableCastle == AvailableCastle.Kingside)
                    FEN += "K";
                if (_whiteAvailableCastle == AvailableCastle.Both || _whiteAvailableCastle == AvailableCastle.QueenSide)
                    FEN += "Q";
                if (_blackAvailableCastle == AvailableCastle.Both || _blackAvailableCastle == AvailableCastle.Kingside)
                    FEN += "k";
                if (_blackAvailableCastle == AvailableCastle.Both || _blackAvailableCastle == AvailableCastle.QueenSide)
                    FEN += "q";
            }
            FEN += " ";

            FEN += _enPassantTarget + " ";

            FEN += _halfMoveCounter + " ";

            FEN += _moveNumber;

            return FEN;
        }

        public void ReadFEN(string FEN)
        {
            string[] splitFEN = FEN.Split(' ');    // \x0020

            _boardState = new string[8, 8];
            string[] boardFEN = splitFEN[0].Split('/');   // \x002F
            for (int i = 0; i < 8; i++)
            {
                int rowPos = 0;
                for (int j = 0; j < boardFEN[i].Length; j++)
                {
                    if (char.IsNumber(boardFEN[i], j))
                        for (int k = 0; k < int.Parse(boardFEN[i].Substring(j, 1)); k++)
                        {
                            _boardState[i, rowPos] = "";
                            rowPos++;
                        }                 
                    else
                    {
                        _boardState[i, rowPos] = boardFEN[i].Substring(j, 1);
                        rowPos++;
                    }                    
                }   
            }

            _activeColor = splitFEN[1] == "w" ? PieceColor.White : PieceColor.Black;
            
            char[] castleFEN = splitFEN[2].ToCharArray();
            _whiteAvailableCastle = AvailableCastle.None;
            _blackAvailableCastle = AvailableCastle.None;
            if (castleFEN[0] != '-')    // \x002D
                for (int i = 0; i < castleFEN.Length; i++)
                {
                    if (castleFEN[i] == 'K')
                        _whiteAvailableCastle = AvailableCastle.Kingside;
                    else if (castleFEN[i] == 'Q')
                        _whiteAvailableCastle = _whiteAvailableCastle == AvailableCastle.Kingside ? AvailableCastle.Both : AvailableCastle.QueenSide;
                    else if (castleFEN[i] == 'k')
                        _blackAvailableCastle = AvailableCastle.Kingside;
                    else if (castleFEN[i] == 'q')
                        _blackAvailableCastle = _blackAvailableCastle == AvailableCastle.Kingside ? AvailableCastle.Both : AvailableCastle.QueenSide;
                }

            _enPassantTarget = splitFEN[3];

            _halfMoveCounter = int.Parse(splitFEN[4]);

            _moveNumber = int.Parse(splitFEN[5]);
        }

        public void SaveGameFENList(string savePath)
        {
            // TODO: Implement
        }
    }
}
