using System;
using System.Collections.Generic;
using System.Text;

namespace Chess
{
    public class Piece
    {
        public PieceColor color;
        public PieceType type;
        public string shortName;
        public string square;
        public List<String> possibleMoves;

        public Piece(PieceColor color, PieceType type, string shortName, string square)
        {
            this.color = color;
            this.type = type;
            this.shortName = shortName;
            this.square = square;
        }
    }
}
