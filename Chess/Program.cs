﻿using System;

namespace Chess
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new ChessGame())
                game.Run();
        }
    }
}
