using System;
using System.Collections.Generic;
using System.Text;

namespace Chess
{
    internal static class Moves
    {
        public static List<string> GetAvailableMoves(Board board, Piece piece)
        {
            List<string> possibleMoves = new List<string>();

            if (piece.type == PieceType.Pawn)
                possibleMoves.AddRange(GetPawnMoves(piece, board));
            else if (piece.type == PieceType.Rook)
                possibleMoves.AddRange(GetRookMoves(piece, board));
            else if (piece.type == PieceType.Knight)
                possibleMoves.AddRange(GetKnightMoves(piece, board));
            else if (piece.type == PieceType.Bishop)
                possibleMoves.AddRange(GetBishopMoves(piece, board));
            else if (piece.type == PieceType.Queen)
                possibleMoves.AddRange(GetQueenMoves(piece, board));
            else if (piece.type == PieceType.King)
                possibleMoves.AddRange(GetKingMoves(piece, board));

            // If a move puts the king in check -> the move is not possible (remove it from the list)
            Board testBoard;
            for (int i = possibleMoves.Count - 1; i >= 0; i--)
            {
                testBoard = new Board(board.FEN, forTest: true);
                testBoard.TestMove(piece, possibleMoves[i]);
                if (IsKingInCheck(testBoard))
                    possibleMoves.RemoveAt(i);               
            }

            return possibleMoves;
        }

        private static List<string> GetPawnMoves(Piece piece, Board board)
        {
            int[] arrayIndex = Util.GetArrayIndex(piece.square);
            int rowID = arrayIndex[0];
            int colID = arrayIndex[1];

            List<string> pawnMoves = new List<string>();

            int sign = piece.color == PieceColor.White ? -1 : 1;

            // "Two-square" move
            if (rowID == (int)(3.5f - 2.5f * sign) && board.BoardState[rowID + 2 * sign, colID] == "")
                pawnMoves.Add(Util.GetSquare(new int[] { rowID + 2 * sign, colID }));

            // Regular move forward
            if (board.BoardState[rowID + 1 * sign, colID] == "")
                pawnMoves.Add(Util.GetSquare(new int[] { rowID + 1 * sign, colID }));

            // Take "left"
            if (colID - 1 >= 0 && board.BoardState[rowID + 1 * sign, colID - 1] != "" && IsOppositeColor(piece.color, char.Parse(board.BoardState[rowID + 1 * sign, colID - 1])))
                pawnMoves.Add(Util.GetSquare(new int[] { rowID + 1 * sign, colID - 1 }));

            // Take "right"
            if (colID + 1 <= 7 && board.BoardState[rowID + 1 * sign, colID + 1] != "" && IsOppositeColor(piece.color, char.Parse(board.BoardState[rowID + 1 * sign, colID + 1])))
                pawnMoves.Add(Util.GetSquare(new int[] { rowID + 1 * sign, colID + 1 }));

            // En passant "left"
            if (Util.GetSquare(new int[] { rowID + 1 * sign, colID - 1 }) == board.EnPassantTarget)
                pawnMoves.Add(Util.GetSquare(new int[] { rowID + 1 * sign, colID - 1 }));

            // En passant "right"
            if (Util.GetSquare(new int[] { rowID + 1 * sign, colID + 1 }) == board.EnPassantTarget)
                pawnMoves.Add(Util.GetSquare(new int[] { rowID + 1 * sign, colID + 1 }));

            return pawnMoves;
        }

        private static List<string> GetRookMoves(Piece piece, Board board)
        {
            int[] arrayIndex = Util.GetArrayIndex(piece.square);
            int rowID = arrayIndex[0];
            int colID = arrayIndex[1];

            List<string> rookMoves = new List<string>();

            // Vertical "down"
            for (int i = rowID + 1; i < 8; i++)
                if (CheckIteratedMove(board, piece, ref rookMoves, i, colID))
                    break;

            // Vertical "up"
            for (int i = rowID - 1; i >= 0; i--)
                if (CheckIteratedMove(board, piece, ref rookMoves, i, colID))
                    break;

            // Horizontal "right"
            for (int i = colID + 1; i < 8; i++)
                if (CheckIteratedMove(board, piece, ref rookMoves, rowID, i))
                    break;

            // Horizontal "left"
            for (int i = colID - 1; i >= 0; i--)
                if (CheckIteratedMove(board, piece, ref rookMoves, rowID, i))
                    break;

            return rookMoves;
        }

        private static List<string> GetKnightMoves(Piece piece, Board board)
        {
            int[] arrayIndex = Util.GetArrayIndex(piece.square);
            int rowID = arrayIndex[0];
            int colID = arrayIndex[1];

            List<string> knightMoves = new List<string>();

            for (int i = -2; i <= 2; i++)
                for (int j = -2; j <= 2; j++)
                {
                    if (Math.Abs(i) + Math.Abs(j) != 3)
                        continue;
                    if (rowID + i >= 0 && rowID + i < 8 && colID + j >= 0 && colID + j < 8)
                        if (board.BoardState[rowID + i, colID + j] == "" || IsOppositeColor(piece.color, char.Parse(board.BoardState[rowID + i, colID + j])))
                            knightMoves.Add(Util.GetSquare(new int[] { rowID + i, colID + j }));
                }

            return knightMoves;
        }

        private static List<string> GetBishopMoves(Piece piece, Board board)
        {
            int[] arrayIndex = Util.GetArrayIndex(piece.square);
            int rowID = arrayIndex[0];
            int colID = arrayIndex[1];

            List<string> bishopMoves = new List<string>();

            // "right-down"
            int row = rowID + 1;
            int col = colID + 1;
            while (row < 8 && col < 8)
            {
                if (CheckIteratedMove(board, piece, ref bishopMoves, row, col))
                    break;
                row++;
                col++;
            }

            // "right-up"
            row = rowID - 1;
            col = colID + 1;
            while (row >= 0 && col < 8)
            {
                if (CheckIteratedMove(board, piece, ref bishopMoves, row, col))
                    break;
                row--;
                col++;
            }

            // "left-up"
            row = rowID - 1;
            col = colID - 1;
            while (row >= 0 && col >= 0)
            {
                if (CheckIteratedMove(board, piece, ref bishopMoves, row, col))
                    break;
                row--;
                col--;
            }

            // "left-down"
            row = rowID + 1;
            col = colID - 1;
            while (row < 7 && col >= 0)
            {
                if (CheckIteratedMove(board, piece, ref bishopMoves, row, col))
                    break;
                row++;
                col--;
            }

            return bishopMoves;
        }

        private static List<string> GetQueenMoves(Piece piece, Board board)
        {
            int[] arrayIndex = Util.GetArrayIndex(piece.square);
            int rowID = arrayIndex[0];
            int colID = arrayIndex[1];

            List<string> queenMoves = new List<string>();

            queenMoves.AddRange(GetRookMoves(piece, board));
            queenMoves.AddRange(GetBishopMoves(piece, board));

            return queenMoves;
        }

        private static List<string> GetKingMoves(Piece piece, Board board)
        {
            int[] arrayIndex = Util.GetArrayIndex(piece.square);
            int rowID = arrayIndex[0];
            int colID = arrayIndex[1];

            List<string> kingMoves = new List<string>();

            for (int i = -1; i <= 1; i++)
                if (rowID + i >= 0 && rowID + i < 8)
                    for (int j = -1; j <= 1; j++)
                    {
                        if (i == 0 && j == 0)
                            continue;
                        if (colID + j >= 0 && colID + j < 8)
                            if (board.BoardState[rowID + i, colID + j] == "" || IsOppositeColor(piece.color, char.Parse(board.BoardState[rowID + i, colID + j])))
                                kingMoves.Add(Util.GetSquare(new int[] { rowID + i, colID + j }));
                    }

            return kingMoves;
        }

        public static bool IsKingInCheck(Board board)
        {
            Piece king = null;
            foreach (Piece piece in board.Pieces)
                if (piece.type == PieceType.King && piece.color == board.ActiveColor)
                {
                    king = piece;
                    break;
                }

            int[] arrayIndex = Util.GetArrayIndex(king.square);
            int rowID = arrayIndex[0];
            int colID = arrayIndex[1];

            // Vertical "down"
            for (int i = rowID + 1; i < 8; i++)
            {
                Piece piece = CheckIteratedMove(board, i, colID);
                if (piece != null)
                {
                    if (piece.color != board.ActiveColor && (piece.type == PieceType.Rook || piece.type == PieceType.Queen))
                        return true;
                    break;
                }
            }

            // Vertical "up"
            for (int i = rowID - 1; i >= 0; i--)
            {
                Piece piece = CheckIteratedMove(board, i, colID);
                if (piece != null)
                {
                    if (piece.color != board.ActiveColor && (piece.type == PieceType.Rook || piece.type == PieceType.Queen))
                        return true;
                    break;
                }
            }

            // Horizontal "right"
            for (int i = colID + 1; i < 8; i++)
            {
                Piece piece = CheckIteratedMove(board, rowID, i);
                if (piece != null)
                {
                    if (piece.color != board.ActiveColor && (piece.type == PieceType.Rook || piece.type == PieceType.Queen))
                        return true;
                    break;
                }
            }

            // Horizontal "left"
            for (int i = colID - 1; i >= 0; i--)
            {
                Piece piece = CheckIteratedMove(board, rowID, i);
                if (piece != null)
                {
                    if (piece.color != board.ActiveColor && (piece.type == PieceType.Rook || piece.type == PieceType.Queen))
                        return true;
                    break;
                }
            }

            // "right-down"
            int row = rowID + 1;
            int col = colID + 1;
            while (row < 8 && col < 8)
            {
                Piece piece = CheckIteratedMove(board, row, col);
                if (piece != null)
                {
                    if (piece.color != board.ActiveColor && (piece.type == PieceType.Bishop || piece.type == PieceType.Queen))
                        return true;
                    else if (piece.color != board.ActiveColor && board.ActiveColor == PieceColor.Black && row - rowID == 1 && piece.type == PieceType.Pawn)
                        return true;
                    break;
                }
                row++;
                col++;
            }

            // "right-up"
            row = rowID - 1;
            col = colID + 1;
            while (row >= 0 && col < 8)
            {
                Piece piece = CheckIteratedMove(board, row, col);
                if (piece != null)
                {
                    if (piece.color != board.ActiveColor && (piece.type == PieceType.Bishop || piece.type == PieceType.Queen))
                        return true;
                    else if (piece.color != board.ActiveColor && board.ActiveColor == PieceColor.White && rowID - row == 1 && piece.type == PieceType.Pawn)
                        return true;
                    break;
                }
                row--;
                col++;
            }

            // "left-up"
            row = rowID - 1;
            col = colID - 1;
            while (row >= 0 && col >= 0)
            {
                Piece piece = CheckIteratedMove(board, row, col);
                if (piece != null)
                {
                    if (piece.color != board.ActiveColor && (piece.type == PieceType.Bishop || piece.type == PieceType.Queen))
                        return true;
                    else if (piece.color != board.ActiveColor && board.ActiveColor == PieceColor.White && rowID - row == 1 && piece.type == PieceType.Pawn)
                        return true;
                    break;
                }
                row--;
                col--;
            }

            // "left-down"
            row = rowID + 1;
            col = colID - 1;
            while (row < 7 && col >= 0)
            {
                Piece piece = CheckIteratedMove(board, row, col);
                if (piece != null)
                {
                    if (piece.color != board.ActiveColor && (piece.type == PieceType.Bishop || piece.type == PieceType.Queen))
                        return true;
                    else if (piece.color != board.ActiveColor && board.ActiveColor == PieceColor.Black && row - rowID == 1 && piece.type == PieceType.Pawn)
                        return true;
                    break;
                }
                row++;
                col--;
            }

            // Knights
            for (int i = -2; i <= 2; i++)
                for (int j = -2; j <= 2; j++)
                {
                    if (Math.Abs(i) + Math.Abs(j) != 3)
                        continue;
                    if (rowID + i >= 0 && rowID + i < 8 && colID + j >= 0 && colID + j < 8 && board.BoardState[rowID + i, colID + j] != "")
                    {
                        Piece piece = null;
                        string square = Util.GetSquare(new int[] { rowID + i, colID + j });
                        foreach (Piece p in board.Pieces)
                            if (p.square == square)
                                piece = p;

                        if (piece != null && piece.type == PieceType.Knight && piece.color != board.ActiveColor)
                            return true;
                    }
                }

            return false;
        }

        private static bool IsOppositeColor(PieceColor pieceColor, char piece)
        {
            if (pieceColor == PieceColor.White)
                return char.IsLower(piece);
            else
                return char.IsUpper(piece);
        }

        private static bool CheckIteratedMove(Board board, Piece piece, ref List<string> moves, int row, int col)
        {
            if (board.BoardState[row, col] == "")
            {
                moves.Add(Util.GetSquare(new int[] { row, col }));
                return false;
            }             
            else if (IsOppositeColor(piece.color, char.Parse(board.BoardState[row, col])))
                moves.Add(Util.GetSquare(new int[] { row, col }));

            return true;
        }

        private static Piece CheckIteratedMove(Board board, int row, int col)
        {
            if (board.BoardState[row, col] != "")
            {
                string square = Util.GetSquare(new int[] { row, col });
                foreach (Piece piece in board.Pieces)
                    if (piece.square == square)
                        return piece;

            }

            return null;
        }
    }
}
