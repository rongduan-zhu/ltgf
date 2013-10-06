using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;
using SharpDX.Toolkit;

namespace Project2
{
    using SharpDX.Toolkit.Graphics;
    using SharpDX.Toolkit.Input;

    public class GameModel
    {
        protected Project2Game game;
        public Model model { get; private set; }
        public Matrix World { get; private set; }

        public GameModel (Model model, Game game) {
            this.game = (Project2Game) game;
            this.model = model;
            World = Matrix.Identity;
        }

        public virtual void Update(GameTime gameTime)
        {
            World = Matrix.RotationY((float)(-gameTime.TotalGameTime.Milliseconds * Math.PI / 500)) * Matrix.Translation(0, 10, 0);
        }

        public virtual void Draw()
        {
            model.Draw(game.GraphicsDevice, World, game.camera.View, game.camera.Projection);
        }
    }
}
