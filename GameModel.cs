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

    class GameModel
    {
        protected Game game;
        protected Camera camera;
        protected Matrix world;
        protected Model model;

        public GameModel (Model model, Game game, Camera camera) {
            this.game = game;
            this.camera = camera;
            this.model = model;
            world = Matrix.Identity;
        }

        public Model getModel()
        {
            return model;
        }

        public void Draw()
        {
            model.Draw(game.GraphicsDevice, world, camera.View, camera.Projection);
        }
    }
}
