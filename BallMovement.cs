﻿using System;
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

    public class BallMovement
    {
        private Project2Game game;
        private float[,] heights;

        private const float bounceThreshold = 0.01f;

        public Vector3 accelerate { get; private set; }
        public Vector3 velocity { get; private set; }

        public BallMovement (Project2Game game)
        {
            this.game = game;
            accelerate = new Vector3(0, -0.01f, 0);
            velocity = new Vector3(0, -0.03f, 0);
            heights = game.landscape.pHeights;
        }

        public void Hit(float v0)
        {
            v0 = v0 / 100;
            float angleV = game.camera.AngleV;
            float angleH = game.camera.AngleH;
            
            Vector3 v = Vector3.Zero;

            v.X = (float)(v0 * Math.Cos(angleV) * Math.Sin(-angleH));
            v.Z = (float)(v0 * Math.Cos(angleV) * Math.Cos(-angleH));
            v.Y = (float)(v0 * Math.Sin(angleV));

            velocity = v;
        }

        private void touchGround (ref Vector3 ballPosition, float groundAltitude)
        {
            // heights difference between left and right
            float heightLRdiff = heights[(int)ballPosition.X - 1, (int)ballPosition.Z]
                - heights[(int)ballPosition.X + 1, (int)ballPosition.Z];
            // heights difference between front and back
            float heightFBdiff = heights[(int)ballPosition.X, (int)ballPosition.Z - 1]
                - heights[(int)ballPosition.X, (int)ballPosition.Z + 1];

            float c = (float)Math.Sqrt(heightLRdiff * heightLRdiff + heightFBdiff * heightFBdiff);
            // proportion on X-axis is cos, proportion on Z-axis is sin.
            float cos = heightLRdiff / c;
            float sin = heightFBdiff / c;
            float accelerate = 0.007f;

            Vector3 v = velocity;
            v.X += accelerate * cos;
            v.Z += accelerate * sin;

            float y = (velocity.Y < bounceThreshold) ?
                0 : velocity.Y * (-0.7f);

            ballPosition = new Vector3(ballPosition.X, ballPosition.Y + game.ball.RADIUS, ballPosition.Z);

            v.Y *= y;

            v *= 0.95f;

            v.X = (Math.Abs(v.X) > 0.03f) ? v.X : 0;
            v.Y = (Math.Abs(v.Y) > 0.03f) ? v.Y : 0;
            v.Z = (Math.Abs(v.Z) > 0.03f) ? v.Z : 0;

            velocity = v;
        }

        private void heightFix(ref Vector3 ballPosition, float groundAltitude)
        {
            Vector3 fpos, bpos, lpos, rpos;
            fpos = new Vector3(ballPosition.X, 0, (int)Math.Ceiling(ballPosition.Z));
            bpos = new Vector3(ballPosition.X, 0, (int)Math.Floor(ballPosition.Z));
            lpos = new Vector3((int)Math.Floor(ballPosition.X), 0, ballPosition.Z);
            rpos = new Vector3((int)Math.Ceiling(ballPosition.X), 0, ballPosition.Z);

            // Calculate the height of next ballPosition. 
            float hOnX = (heights[(int)rpos.X, (int)rpos.Z] - heights[(int)lpos.X, (int)lpos.Z])
                * (ballPosition.X - lpos.X) / (rpos.X - lpos.X)
                + heights[(int)lpos.X, (int)lpos.Z];

            float hOnZ = (heights[(int)fpos.X, (int)fpos.Z] - heights[(int)bpos.X, (int)bpos.Z])
                * (ballPosition.Z - (float)bpos.Z) * (fpos.Z - bpos.Z)
                + heights[(int)bpos.X, (int)bpos.Z];

            // update position Y.
            float nextH = (hOnX + hOnZ) / 2 + game.ball.RADIUS;

            ballPosition = new Vector3(ballPosition.X, nextH, ballPosition.Z);
        }

        private void sentinel(Vector3 ballPosition, float groundAltitude)
        {
            if (game.landscape.isWater((int)ballPosition.X, (int)ballPosition.Z))
            {
                game.main.lose();
                return;
            }

            if (Math.Abs(ballPosition.X - game.landscape.objectivePos.X) < 1
                && Math.Abs(ballPosition.Z - game.landscape.objectivePos.Z) < 1)
            {
                game.main.win();
                return;
            }
        }

        public void Update (GameTime gameTime)
        {
            Vector3 position = game.ball.position;

            velocity += accelerate;
            position += velocity;

            if (!game.landscape.isInside(position.X - 1, position.Z - 1)
                || !game.landscape.isInside(position.X + 1, position.Z + 1 ))
            {
                velocity = Vector3.Zero;
                game.main.lose();
                return;
            }

            float groundAltitude =
                (heights[(int)position.X - 1, (int)position.Z]
                + heights[(int)position.X + 1, (int)position.Z]
                + heights[(int)position.X, (int)position.Z + 1]
                + heights[(int)position.X, (int)position.Z - 1])
                / 4;

            if (position.Y - game.ball.RADIUS / 2 <= groundAltitude)
            {
                touchGround(ref position, groundAltitude);
                heightFix(ref position, groundAltitude);
            }

            sentinel(position, groundAltitude);

            game.ball.position = position;
        }
    }
}
