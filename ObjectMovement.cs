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

    public class ObjectMovement
    {
        Project2Game game;

        // initial velocity
        public Vector3 V;
        Vector3 direction;
        // accelerate 
        float accelerate;
        float cos;
        float sin;
        // the longest side
        float c;
        // radius
        float r = 0.03f;

        public ObjectMovement(Project2Game game)
        {
            this.game = game;
            direction = new Vector3(1,1,1);
            V = new Vector3(0,0,0);
        }

        public Vector3 InitializeV(float v0, float AngleV, float AngleH)
        {
            Vector3 v = new Vector3();
            //* Math.PI / 180 change degree into radian
            v.X = (v0 * (float)Math.Sin(AngleV * Math.PI / 180) * (float)Math.Cos(AngleH * Math.PI / 180)); //+(float)(v0 * (float)Math.Cos(AngleH * Math.PI / 180));
            v.Z = (v0 * (float)Math.Sin(AngleV * Math.PI / 180) * (float)Math.Sin(AngleH * Math.PI / 180)); //+(float)(v0 * (float)Math.Sin(AngleH * Math.PI / 180));
            v.Y = (v0 * (float)Math.Cos(AngleV * Math.PI / 180)) * 0.01f;

            return v;
        }

        public Vector3 BallOnGround(Vector3 v, Vector3 position, Vector3 dir, float[,] heights, string landtype)
        {

            // if ball is inside map.
            //if (position.X > 0 && position.Z > 0 && position.X < 512 && position.Z < 512)
            //{
                // heights difference between left and right
                float heightLRdiff = heights[(int)position.X - 1, (int)position.Z] - heights[(int)position.X + 1, (int)position.Z];
                // heights difference between front and back
                float heightFBdiff = heights[(int)position.X, (int)position.Z - 1] - heights[(int)position.X, (int)position.Z + 1];

                c = (float)Math.Sqrt(Math.Pow(heightLRdiff, 2) + Math.Pow(heightFBdiff, 2));
                // proportion on X-axis is cos, proportion on Z-axis is sin.
                cos = heightLRdiff / c;
                sin = heightFBdiff / c;
            

            if (landtype.Equals("water"))
            {

                //TODO: allow ball sink into water with static velocity
                // unitil reaching river bed.
            }
            else if (landtype.Equals("sand"))
            {
                accelerate = -0.01f;
            }
            // control the velocity of ball movement.
            if (v.X > 0.0f)
            {
                v.X += accelerate * cos;
            }
            else
            {
                v.X = 0;
            }

            if (v.Z > 0.0f)
            {
                v.Z += accelerate * sin;
            }
            else
            {
                v.Z = 0.0f;
            }
            position.X += dir.X * v.X;
            position.Z += dir.Z * v.Z;

            Vector3 fpos, bpos, lpos, rpos;
            fpos = new Vector3(position.X, 0, (int)Math.Ceiling(position.Z));
            bpos = new Vector3(position.X, 0, (int)Math.Floor(position.Z));
            lpos = new Vector3((int)Math.Floor(position.X), 0, position.Z);
            rpos = new Vector3((int)Math.Ceiling(position.X), 0, position.Z);

            float hOnX = (heights[(int)rpos.X, (int)rpos.Z] - heights[(int) lpos.X, (int) lpos.Z]) 
                * (position.X - lpos.X) / ( rpos.X - lpos.X)
                + heights[(int) lpos.X, (int) lpos.Z];

            float hOnZ = (heights[(int) fpos.X, (int) fpos.Z] - heights[(int) bpos.X, (int) bpos.Z]) 
                *  (position.Z - (float) bpos.Z) * (fpos.Z - bpos.Z)
                + heights[(int) bpos.X, (int) bpos.Z];

            float nextH = (hOnX + hOnZ) / 2; 

            //}
            return position;

        }

        // check if ball hit ground.
        private bool hitGround(Vector3 position, float[,] heights)
        {

            // Comparing the height of current position of ball to 
            // the height of position in map. If height of ball is less or equal
            // to height of position in map, then set hitGround to true.
            return position.Y <= (heights[(int)position.X, (int)position.Z] + r);
        
        }

        // control ball movement after ball hit ground and bounce back to sky.
        private float ballBounce(float a, Vector3 v)
        {
            float h = 0;
            float time = 0;

            time = v.Y / a;
            h = v.Y * time + (1 / 2) * a * (float) Math.Pow(time, 2);

            return h;
        }

        public void Update(GameTime gameTime)
        {
            Vector3 position =  game.ball.position;

            if (hitGround(position, game.landscape.pHeights))
            {

                position = this.BallOnGround(V, position, direction, game.landscape.pHeights, "sand");
            }
            else
            {

                if (!V.Y.Equals(0.0f))
                {
                  //V.Y -= 0.0001f;
                }
                else
                {
                    V.Y = 0.0f;
                }

                position.Y += V.Y;
                position.X += V.X;
                position.Z += V.Z;
            }

            game.ball.position = position;
        }
    }
}
