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

                }
                else if (landtype.Equals("moutain"))
                {

                }
                else if (landtype.Equals("sand"))
                {
                    fPosition.Y = heights[(int)fPosition.X, (int)fPosition.Z];
                    a = -0.01f;
                }
                else
                {
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
    }
}
