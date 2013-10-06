using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;
using SharpDX.Toolkit;

using Windows.UI.Input;
using Windows.UI.Core;

namespace Project2
{
    using SharpDX.Toolkit.Input;

    public class Camera
    {
        public Matrix View;
        public Matrix Projection;
        public Game game;

        public Vector3 distance;

        private float AngleH = 0;
        private float AngleV = 0;

        // Ensures that all objects are being rendered from a consistent viewpoint
        public Camera(Game game) {
            distance = new Vector3(0, 30, -10);

            View = Matrix.LookAtLH(distance, Vector3.Zero, Vector3.UnitY);
            Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4.0f, (float)game.GraphicsDevice.BackBuffer.Width / game.GraphicsDevice.BackBuffer.Height, 0.1f, 100.0f);

            this.game = game;
        }

        public void OnManipulationUpdated(GestureRecognizer sender, ManipulationUpdatedEventArgs args)
        {
            AngleH -= (float)args.Delta.Translation.X / 100;
            if ((AngleV <= 3 * (float)Math.PI / 9 && (float)args.Delta.Translation.Y > 0) || (AngleV > 0 && (float)args.Delta.Translation.Y < 0))
                AngleV += (float)args.Delta.Translation.Y / 100;
        }

        public void Update(GameTime gameTime)
        {
            View = Matrix.LookAtLH(distance, Vector3.Zero, Vector3.UnitY);
        }
    }
}
