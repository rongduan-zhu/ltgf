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
        public Effect effect;
        public Vector3 position { get; set; }

         Vector3[] lightPointPositions;

        public GameModel (Model model, Game game, float x, float y, float z)
        {
            this.game = (Project2Game) game;
            this.model = model;
            effect = game.Content.Load<Effect>("ObjectShader");
            this.position = new Vector3(x, y, z);
            World = Matrix.Identity;

            lightPointPositions = new [] {
                new Vector3(0, 180, 0),
                new Vector3(-this.game.landscape.baord_size_public , 180, 0),
                new Vector3(this.game.landscape.baord_size_public, 180, this.game.landscape.baord_size_public)
            };
        }

        public GameModel () {}

        public virtual void Update(GameTime gameTime)
        {
            World = Matrix.Translation(position.X, position.Y, position.Z);
        }

        public virtual void Draw()
        {
            //BasicEffect.EnableDefaultLighting(model, true);

            effect.Parameters["World"].SetValue(World);
            effect.Parameters["Projection"].SetValue(game.camera.Projection);
            effect.Parameters["worldInvTrp"].SetValue(Matrix.Transpose(Matrix.Invert(World)));
            effect.Parameters["maxHeight"].SetValue(0);
            effect.Parameters["View"].SetValue(game.camera.View);
            effect.Parameters["cameraPos"].SetValue(game.camera.RealPosition);
            effect.Parameters["mainLightPos"].SetValue(lightPointPositions[0]);
            effect.Parameters["supportLightPos"].SetValue(lightPointPositions[1]);
            effect.Parameters["oppMainLightPos"].SetValue(lightPointPositions[2]);
            model.Draw(game.GraphicsDevice, World, game.camera.View, game.camera.Projection, effect);
        }
    }
}
