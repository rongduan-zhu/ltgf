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
    using SharpDX.Toolkit.Input;
    using SharpDX.Toolkit.Content;

    class ObjectMovement
    {
        public ObjectMovement()
        {
        }

        public Vector3 BallMove(float v0, Vector3 position, Vector3 dir, float[,] heights, string landtype)
        {
            Vector3 fPosition = position;
            float a = 0;

            if (this.hitGround(fPosition, heights))
            {

                if (landtype.Equals("water"))
                {

                        //TODO: allow ball sink into water with static velocity
                        // unitil reaching river bed.
                }
                else if (landtype.Equals("moutain"))
                {
                    // heights difference between left and right
                    float heightLRdiff = heights[(int)fPosition.X - 1, (int)fPosition.Z] - heights[(int)fPosition.X + 1, (int)fPosition.Z];
                    // heights difference between front and back
                    float heightFBdiff = heights[(int)fPosition.X, (int)fPosition.Z - 1] - heights[(int)fPosition.X, (int)fPosition.Z + 1];
                }
                else if (landtype.Equals("sand"))
                {
                    fPosition.Y = heights[(int)fPosition.X, (int)fPosition.Z];
                    a = -0.01f;
                }
                else
                {
                    dir.Y = -1;
                    a = -0.008f;
                }

                if (v0 > 0)
                {
                    v0 += a;
                }
                else
                {
                    v0 = 0;
                }
                fPosition.X += dir.X * v0;
                fPosition.Z += dir.Y * v0;
            }

            return fPosition;

        }

        private bool hitGround(Vector3 position, float[,] heights)
        {
            bool hitGround = false;

            // Comparing the height of current position of ball to 
            // the height of position in map. If height of ball is less or equal
            // to height of position in map, then set hitGround to true.
            if (position.Y <= heights[(int)position.X, (int)position.Z])
            {
                hitGround = true;
            }

            return hitGround;
        }

        private float ballBounce(float a, float v0)
        {
            float h = 0;
            float time = 0;

            time = v0 / a;
            h = v0 * time + (1 / 2) * a * (float) Math.Pow(time, 2);

            return h;
        }
    }
}
