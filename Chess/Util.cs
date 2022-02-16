using System;
using System.Collections.Generic;
using System.Text;

namespace Chess
{
    public static class Util
    {
        public static int[] GetArrayIndex(string square)
        {
            char[] chars = square.ToCharArray();

            char colID = chars[0];
            int rowID = (int)char.GetNumericValue(chars[1]);

            int col = colID - 97;
            int row = 8 - rowID;

            return new int[] { row, col };
        }

        public static string GetSquare(int row, int col)
        {
            int rowID = 8 - row;
            char colID = (char)(col + 97);

            return colID.ToString() + rowID.ToString();
        }

        public static string GetSquare(int[] arrayIndex)
        {
            return GetSquare(arrayIndex[0], arrayIndex[1]);
        }

        public static List<Piece> PopulatePiecesList(string[,] boardState)
        {
            List<Piece> pieces = new List<Piece>();

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (boardState[i, j] == "")
                        continue;

                    PieceType pieceType;
                    if (boardState[i, j].ToLower() == "r")
                        pieceType = PieceType.Rook;
                    else if (boardState[i, j].ToLower() == "n")
                        pieceType = PieceType.Knight;
                    else if (boardState[i, j].ToLower() == "b")
                        pieceType = PieceType.Bishop;
                    else if (boardState[i, j].ToLower() == "q")
                        pieceType = PieceType.Queen;
                    else if (boardState[i, j].ToLower() == "k")
                        pieceType = PieceType.King;
                    else
                        pieceType = PieceType.Pawn;

                    PieceColor pieceColor = GetPieceColor(boardState[i, j]);
                    string shortName = boardState[i, j];
                    string square = GetSquare(new int[] { i, j });

                    pieces.Add(new Piece(pieceColor, pieceType, shortName, square));
                }
            }

            return pieces;
        }

        private static PieceColor GetPieceColor(string shortName)
        {
            if (char.IsUpper(char.Parse(shortName)))
                return PieceColor.White;
            
            return PieceColor.Black;
        }
    }
}
