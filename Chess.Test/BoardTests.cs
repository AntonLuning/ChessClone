using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace Chess.Test
{
    public class BoardTests
    {
        [Fact]
        public void FEN_StartingStringShouldBeCorrect()
        {
            Board _board = new Board();

            string expected = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

            string actual = _board.FEN;

            Assert.Equal(expected, actual);
        }

        [Theory]
        [ClassData(typeof(BoardOKMovesData))]
        public void Moves_ShouldBeOk(params string[] moves)
        {
            Board _board = new Board();

            for (int i = 0; i < moves.Length - 1; i++)
                _board.MakeMove(moves[i]);

            int actual = _board.AvailableMoves(moves[moves.Length - 1]).Count;
  
            Assert.True(actual > 0);
        }

        [Theory]
        [ClassData(typeof(BoardOKMovesData))]
        public void FEN_InGameStringShouldBeCorrect(params string[] moves)
        {
            Board _board = new Board();

            string expected = moves[moves.Length - 1];

            for (int i = 0; i < moves.Length - 1; i++)
                _board.MakeMove(moves[i]);

            string actual = _board.FEN;

            Assert.Equal(expected, actual);
        }

        public class BoardOKMovesData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] { "e4" , "rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1" };
                yield return new object[] { "a3", "c5" , "Nf3", "rnbqkbnr/pp1ppppp/8/2p5/4P3/5N2/PPPP1PPP/RNBQKB1R b KQkq - 1 2" };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public class BoardIllegalMovesData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] { "e4", "rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1" };
                yield return new object[] { "a3", "c5", "Nf3", "rnbqkbnr/pp1ppppp/8/2p5/4P3/5N2/PPPP1PPP/RNBQKB1R b KQkq - 1 2" };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
