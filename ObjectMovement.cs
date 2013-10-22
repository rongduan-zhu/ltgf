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

    public class ObjectMovement
    {
        public Vector3 velocity { get; private set; }

        private Project2Game game;

        public ObjectMovement(Project2Game game)
        {
            this.game = game;
            velocity = Vector3.Zero;
        }

        public void InitializeV(float v0, float AngleV, float AngleH)
        {
            Vector3 v = new Vector3();

            v.X = (float)(v0 * Math.Cos(AngleV) * Math.Sin(-AngleH));
            v.Z = (float)(v0 * Math.Cos(AngleV) * Math.Cos(-AngleH));
            v.Y = - (float)(v0 * Math.Sin(-AngleV));

            velocity = v;
        }

        private void inAir(ref Vector3 position, float[,] heights)
        {
            float accelerate = -0.007f;
            Vector3 v = velocity;
            v.Y += accelerate;
            velocity = v;

            position.Y += velocity.Y;
            position.X += velocity.X;
            position.Z += velocity.Z;

            if (!game.landscape.isInside(position.X, position.Z))
            {
                game.main.lose();
                return;
            }
        }

        // control the ball movement on ground when velocity on Y-axis is 0.
        public void onGround(ref Vector3 position, float[,] heights)
        {
            // temps
            Vector3 v = velocity;
            v.Y = 0;

            if (game.landscape.isWater((int)position.X, (int)position.Z))
            {
                game.main.lose();
                return;
            }

            if (Math.Abs(game.ball.position.X - game.landscape.objectivePos.X) < 0.1
                || Math.Abs(game.ball.position.Z - game.landscape.objectivePos.Z) < 0.1)
            {
                game.main.win();
                return;
            }
            
            // control the velocity of ball movement on XZ-axis.
            if (Math.Abs(v.X) > 0.1f)
            {
                //v.X += accelerate * cos;
                v.X *= 0.98f;
            }
            else
            {
                v.X = 0;
            }

            if (Math.Abs(v.Z) > 0.1f)
            {
                //v.Z += accelerate * sin;
                v.Z *= 0.98f;
            }
            else
            {
                v.Z = 0.0f;
            }

            position.X += v.X;
            position.Z += v.Z;

            if (!game.landscape.isInside(position.X, position.Z))
            {
                game.main.lose();
                return;
            }

            Vector3 fpos, bpos, lpos, rpos;
            fpos = new Vector3(position.X, 0, (int)Math.Ceiling(position.Z));
            bpos = new Vector3(position.X, 0, (int)Math.Floor(position.Z));
            lpos = new Vector3((int)Math.Floor(position.X), 0, position.Z);
            rpos = new Vector3((int)Math.Ceiling(position.X), 0, position.Z);

            // Calculate the height of next position. 
            float hOnX = (heights[(int)rpos.X, (int)rpos.Z] - heights[(int)lpos.X, (int)lpos.Z])
                * (position.X - lpos.X) / (rpos.X - lpos.X)
                + heights[(int)lpos.X, (int)lpos.Z];

            float hOnZ = (heights[(int)fpos.X, (int)fpos.Z] - heights[(int)bpos.X, (int)bpos.Z])
                * (position.Z - (float)bpos.Z) * (fpos.Z - bpos.Z)
                + heights[(int)bpos.X, (int)bpos.Z];

            // update position Y.
            float nextH = (hOnX + hOnZ) / 2 + game.ball.RADIUS;
            if (nextH > position.Y)
            {
                v *= 0.8f;
            }
            position.Y = nextH;
            velocity = v;
        }

        // check if ball hit ground.
        private bool hitGround(ref Vector3 position, float[,] heights)
        {
            // Comparing the height of current position of ball to 
            // the height of position in map. If height of ball is less or equal
            // to height of position in map, then set hitGround to true.
            //return position.Y <= (heights[(int)position.X, (int)position.Z] + r);
            float heightLRavg = heights[(int)position.X - 1, (int)position.Z] + heights[(int)position.X + 1, (int)position.Z];
            heightLRavg /= 2;
            float heightFBavg = heights[(int)position.X, (int)position.Z - 1] + heights[(int)position.X, (int)position.Z + 1];
            heightFBavg /= 2;

            float height = (heightFBavg + heightLRavg) / 2;

            return position.Y <= (height);
       }

        // control ball movement after ball hit ground and bounce back to sky.
        private void ballBounce(ref Vector3 position, float[,] heights)
        {
            // heights difference between left and right
            float heightLRdiff = heights[(int)position.X - 1, (int)position.Z] - heights[(int)position.X + 1, (int)position.Z];
            // heights difference between front and back
            float heightFBdiff = heights[(int)position.X, (int)position.Z - 1] - heights[(int)position.X, (int)position.Z + 1];

            float c = (float)Math.Sqrt(heightLRdiff * heightLRdiff + heightFBdiff * heightFBdiff);
            // proportion on X-axis is cos, proportion on Z-axis is sin.
            float cos = heightLRdiff / c;
            float sin = heightFBdiff / c;
            float accelerate = 0.01f;

            Vector3 v = velocity;
            v.Y = -0.3f * v.Y;
            v.X += accelerate * cos;
            v.Z += accelerate * sin;
            velocity = v;

            Vector3 fpos, bpos, lpos, rpos;
            fpos = new Vector3(position.X, 0, (int)Math.Ceiling(position.Z));
            bpos = new Vector3(position.X, 0, (int)Math.Floor(position.Z));
            lpos = new Vector3((int)Math.Floor(position.X), 0, position.Z);
            rpos = new Vector3((int)Math.Ceiling(position.X), 0, position.Z);

            // Calculate the height of next position. 
            float hOnX = (heights[(int)rpos.X, (int)rpos.Z] - heights[(int)lpos.X, (int)lpos.Z])
                * (position.X - lpos.X) / (rpos.X - lpos.X)
                + heights[(int)lpos.X, (int)lpos.Z];

            float hOnZ = (heights[(int)fpos.X, (int)fpos.Z] - heights[(int)bpos.X, (int)bpos.Z])
                * (position.Z - (float)bpos.Z) * (fpos.Z - bpos.Z)
                + heights[(int)bpos.X, (int)bpos.Z];

            // update position Y.
            float nextH = (hOnX + hOnZ) / 2 + game.ball.RADIUS;
            position.Y = nextH;

            position.Y = nextH + velocity.Y;
            position.X += velocity.X;
            position.Z += velocity.Z;

            if (!game.landscape.isInside(position.X, position.Z))
            {
                game.gameState = Project2Game.GameState.Lose;
                return;
            }

        }

        public void Update(GameTime gameTime)
        {
            Vector3 position = game.ball.position;

            if (!game.landscape.isInside(position.X - 1, position.Z - 1)
                || !game.landscape.isInside(position.X + 1, position.Z + 1 ))
            {
                game.main.lose();
                return;
            }

            if (hitGround(ref position, game.landscape.pHeights))
            {
                if (Math.Abs(velocity.Y) >  0.005f)
                {
                    ballBounce(ref position, game.landscape.pHeights);
                }
                else
                {
                    onGround(ref position, game.landscape.pHeights);
                }
            }
            else
            {
                inAir(ref position, game.landscape.pHeights);
            }

            game.ball.position = position;
        }
    }
}
