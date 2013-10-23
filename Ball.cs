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

    public class Ball : GameModel
    {
        public float RADIUS = 0.12f;

        public Ball (Model model, Game game, float x, float y, float z)
            : base(model, game, x, y, z) {}

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }
    }
}
