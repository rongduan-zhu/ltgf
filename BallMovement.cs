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
    using SharpDX.Toolkit.Content;

    class BallMovement
    {
        private Project2Game game;

        public Vector3 velocity { get; private set; }
        public Vector3 accelerate { get; private set; }

        public BallMovement (Project2Game game)
        {
            this.game = game;
            velocity = Vector3.Zero;
        }

        public void Hit(float v0)
        {
            float angleV = game.camera.AngleV;
            float angleH = game.camera.AngleH;
            
            Vector3 v = Vector3.Zero;

            v.X = (float)(v0 * Math.Cos(angleV) * Math.Sin(-angleH));
            v.Z = (float)(v0 * Math.Cos(angleV) * Math.Cos(-angleH));
            v.Y = (float)(v0 * Math.Sin(-angleV));

            velocity = v;
        }

        public void Update (GameTime gameTime)
        {
            Vector3 position = game.ball.position;

            if (!game.landscape.isInside(position.X - 1, position.Z - 1)
                || !game.landscape.isInside(position.X + 1, position.Z + 1 ))
            {
                game.main.lose();
                return;
            }

            position += velocity;

            game.ball.position = position;

        }
    }
}
