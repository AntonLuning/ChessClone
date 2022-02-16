using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace ChessGame
{
    public static class TempLib
    {
        public static void SetRelativeBackBufferSize_Temp(GraphicsDeviceManager graphics, float ratio, float aspectRatio)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");

            ratio = MGL.Util.Clamp(ratio, 0.25f, 1f);

            DisplayMode dm = graphics.GraphicsDevice.DisplayMode;

            float dmAR = dm.AspectRatio;
            int height = (int)MathF.Round(dm.Height * ratio);
            int width = (int)MathF.Round(height * aspectRatio);

            if (aspectRatio < 1f)
            {
                width = (int)MathF.Round(dm.Width / ratio);
                height = (int)MathF.Round(width / aspectRatio);
            }

            graphics.PreferredBackBufferWidth = width;
            graphics.PreferredBackBufferHeight = height;
            graphics.ApplyChanges();
        }
    }
}
