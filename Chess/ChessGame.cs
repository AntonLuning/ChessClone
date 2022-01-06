using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MGL;
using MGL.Graphics;
using MGL.Input;
using System;

namespace Chess
{
    public class ChessGame : Game
    {
        private GraphicsDeviceManager _graphics;
        private Screen _screen;
        private Camera _camera;
        private Sprites _sprites;
        private Shapes _shapes;

        public ChessGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.SynchronizeWithVerticalRetrace = true;

            Content.RootDirectory = "Content";

            IsMouseVisible = true;
            IsFixedTimeStep = true;
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = 1400;
            _graphics.PreferredBackBufferHeight = 1000;
            _graphics.ApplyChanges();

            _screen = new Screen(this, 800, 800);
            _camera = new Camera(_screen);
            _sprites = new Sprites(this);
            _shapes = new Shapes(this);

            base.Initialize();
        }

        protected override void LoadContent()
        {

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
                Util.ToggleFullScreen(_graphics);



            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            _screen.Set();
            GraphicsDevice.Clear(Color.Black);

            // Draw everything here...

            _screen.UnSet();
            _screen.Present(_sprites);

            base.Draw(gameTime);

            base.Draw(gameTime);
        }
    }
}
