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
        public MainPage main;

        public float AngleH { get; private set; }
        public float AngleV { get; private set; }
        private float scaleFactor = 1;

        public Vector3 position { get; private set; }
        public Vector3 distance { get; private set; }
        public Vector3 RealDistance { get; private set; }
        public Vector3 RealPosition { get; private set; }

        public MouseManager mouseManager;
        public float prevDelta;

        public Camera(Project2Game game, MainPage main) {
            distance = new Vector3(0, 0, -15);
            position = new Vector3(0, 0, 0);
            View = Matrix.LookAtLH(distance, Vector3.Zero, Vector3.UnitY);
            Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4.0f,
                (float)game.GraphicsDevice.BackBuffer.Width / game.GraphicsDevice.BackBuffer.Height, 0.1f, 500.0f);

            AngleH = 0;
            AngleV = -0.7f;
            this.main = main;
            this.game = game;

            mouseManager = new MouseManager(game);
        }

        public void OnManipulationUpdated(GestureRecognizer sender, ManipulationUpdatedEventArgs args)
        {
            if (main.focussld == false)
            {
                scaleFactor /= (float)args.Delta.Scale;
                AngleH += (float)args.Delta.Translation.X / 500;
                AngleV += (float)args.Delta.Translation.Y / 500;
            }
        }

        public void Update(GameTime gameTime)
        {
            //Mouse update
            MouseState mouseState = mouseManager.GetState();
            scaleFactor -= (mouseState.WheelDelta - prevDelta) / 1000.0f;
            prevDelta = mouseState.WheelDelta;

            if (scaleFactor > 10) {
                scaleFactor = 10;
            }
            if (scaleFactor < 0.1) {
                scaleFactor = 0.1f;
            }

            Vector4 temp = new Vector4(distance, 1);
            temp = Vector4.Transform(temp, Matrix.Scaling(scaleFactor) * Matrix.RotationY(-AngleH) * Matrix.RotationX(-AngleV));
            RealDistance = new Vector3(temp.X, temp.Y, temp.Z);
            RealPosition = game.ball.position + RealDistance;

            position = game.ball.position + distance * scaleFactor;
            View = Matrix.Translation(-game.ball.position.X, -game.ball.position.Y, -game.ball.position.Z)
                * Matrix.RotationY(AngleH)
                * Matrix.RotationX(AngleV)
                * Matrix.Translation(game.ball.position.X, game.ball.position.Y, game.ball.position.Z)
                * Matrix.LookAtLH(position, game.ball.position, Vector3.UnitY);
        }
    }
}
