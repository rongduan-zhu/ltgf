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
        public Project2Game game;


        public float AngleH { get; private set; }
        public float AngleV { get; private set; }
        private float scaleFactor = 1;

        private Vector3 distance, position;

        public Camera(Project2Game game) {
            distance = new Vector3(0, 55, -15);
            position = new Vector3(0, 0, 0);
            View = Matrix.LookAtLH(distance, Vector3.Zero, Vector3.UnitY);
            Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4.0f,
                (float)game.GraphicsDevice.BackBuffer.Width / game.GraphicsDevice.BackBuffer.Height, 0.1f, 500.0f);

            AngleH = AngleV = 0;

            this.game = game;
        }

        public void OnManipulationUpdated(GestureRecognizer sender, ManipulationUpdatedEventArgs args)
        {
            scaleFactor *= (float) args.Delta.Scale;
            AngleH -= (float)args.Delta.Translation.X / 100;
            AngleV -= (float)args.Delta.Translation.Y / 100;
        }

        public void Update(GameTime gameTime)
        {
            position = game.ball.position + distance * scaleFactor;
            View = Matrix.Translation(-game.ball.position.X, -game.ball.position.Y, -game.ball.position.Z)
                * Matrix.RotationY(AngleH)
                * Matrix.RotationX(AngleV)
                * Matrix.Translation(game.ball.position.X, game.ball.position.Y, game.ball.position.Z)
                * Matrix.LookAtLH(position, game.ball.position, Vector3.UnitY);
        }
    }
}
