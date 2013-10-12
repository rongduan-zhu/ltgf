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

    class ObjectMovement
    {
        Project2Game game;
        Landscape2 land;

        // initial velocity
        float v0; 
        Vector3 direction;
        // accelerate 
        float acc;
        float cos;
        float sin;
        // the longest side
        float c;
        // radius
        float r = 0.08f;

        public ObjectMovement(Project2Game game)
        {
            this.game = game;
            land = new Landscape2(game);
            direction = new Vector3(1,1,1);
            v0 = 0.01f;

        }

        public Vector3 BallOnGround(float v0, Vector3 position, Vector3 dir, float[,] heights, string landtype)
        {
            Vector3 fPosition = position;

            if (this.hitGround(fPosition, heights))
            {
                fPosition.Y = heights[(int)fPosition.X, (int)fPosition.Z] + r;

                if (fPosition.X > 0 && fPosition.Z > 0)
                {
                    // heights difference between left and right
                    float heightLRdiff = heights[(int)fPosition.X - 1, (int)fPosition.Z] - heights[(int)fPosition.X + 1, (int)fPosition.Z];
                    // heights difference between front and back
                    float heightFBdiff = heights[(int)fPosition.X, (int)fPosition.Z - 1] - heights[(int)fPosition.X, (int)fPosition.Z + 1];

                    c = (float)Math.Sqrt(Math.Pow(heightLRdiff, 2) + Math.Pow(heightFBdiff, 2));
                    cos = heightLRdiff / c;
                    sin = heightFBdiff / c;

                }

                if (landtype.Equals("water"))
                {

                        //TODO: allow ball sink into water with static velocity
                        // unitil reaching river bed.
                }
                else if (landtype.Equals("sand"))
                {
                    acc = -0.001f;
                }
                else
                {
                    dir.Y = -1;
                    acc = -0.008f;
                }

                // control the velocity of ball movement.
                if (v0 > 0.0f)
                {
                    v0 += acc;
                }
                else
                {
                    v0 = 0;
                }

                fPosition.X += dir.X * v0; 
                fPosition.Z += dir.Z * v0;
                //fPosition.Y += dir.Y * v0;
            }

            return fPosition;

        }

        // check if ball hit ground.
        private bool hitGround(Vector3 position, float[,] heights)
        {
            bool hitGround = false;

            // Comparing the height of current position of ball to 
            // the height of position in map. If height of ball is less or equal
            // to height of position in map, then set hitGround to true.
            //if((int) position.X >= 0 && (int) position.Z >= 0)
            //{
                if (position.Y <= heights[(int)position.X, (int)position.Z] + r)
                {
                    
                    hitGround = true;
                }
            //}

            return hitGround;
        }

        // control ball movement after ball hit ground and bounce back to sky.
        private float ballBounce(float a, float v0)
        {
            float h = 0;
            float time = 0;

            time = v0 / a;
            h = v0 * time + (1 / 2) * a * (float) Math.Pow(time, 2);

            return h;
        }

        public void Update(GameTime gameTime)
        {
            Vector3 position =  game.ball.position;

            if (hitGround(position, land.pHeights))
            {

                position = this.BallOnGround(v0, position, direction, land.pHeights, "sand");
            }
            else
            {
                position.Y -= v0;
                //position.X += v0;
                //position.Z += v0;
            }

            game.ball.position = position;
        }
    }
}
