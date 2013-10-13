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
        public Model model { get; protected set; }
        public Matrix World { get; protected set; }

        public Vector3 position { get; set; }

        public GameModel (Model model, Game game, float x, float y, float z)
        {
            this.game = (Project2Game) game;
            this.model = model;
            this.position = new Vector3(x, y, z);
            World = Matrix.Identity;
        }

        public GameModel () {}

        public virtual void Update(GameTime gameTime)
        {
            World = Matrix.Translation(position.X, position.Y, position.Z);
        }

        public virtual void Draw()
        {
            model.Draw(game.GraphicsDevice, World, game.camera.View, game.camera.Projection);
        }
    }
}
