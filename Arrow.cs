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

    public class Arrow : GameModel
    {
        public Arrow(Model model, Game game, float x, float y, float z)
            : base(model, game, x, y, z) {} 
        
        public override void Update(GameTime gameTime)
        {
            World = Matrix.Scaling(9)
                * Matrix.RotationY((float)(gameTime.TotalGameTime.TotalMilliseconds * Math.PI / 2000))
                * Matrix.Translation(position.X, position.Y, position.Z);
        }
    }
}
