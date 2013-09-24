using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.Toolkit;

namespace Project1
{
    using SharpDX.Toolkit.Graphics;
    using SharpDX.Toolkit.Input;

    class Landscape : ColoredGameObject
    {
        private const float ROTATION = 0.005f, VROTATION = 3 * ROTATION, FRACTION = 0.992f, THRESHOLD = 0.0005f;
        private static int SIZE = 513;
        private static float INIT_DIFF = SIZE / 16, DIFF = SIZE / 6, REDUCING_FACTOR = 0.5f;

        private Random random;
        private float hAcc, hVelocity, vVelocity, hRotation, vRotation;
        private float[,] heights;
        private VertexPositionColor[] vpc;

        public Landscape(Game game)
        {
            hRotation = hVelocity = vVelocity = hAcc = 0;
            vRotation = -(float)Math.PI / 6; // initial overlook angle 30deg
            hVelocity = -0.1f; // initial rotation according to y-axis

            random = new Random();

            heights = new float[SIZE, SIZE];
            initHeights();

            vpc = new VertexPositionColor[(SIZE - 1) * (SIZE -1) * 6];
            initVertices(vpc);

            vertices = Buffer.Vertex.New<VertexPositionColor>(game.GraphicsDevice, vpc);

            basicEffect = new BasicEffect(game.GraphicsDevice)
            {
                VertexColorEnabled = true,
                View = Matrix.LookAtLH(new Vector3(0, 0, -5), new Vector3(0, 0, 0), Vector3.UnitY),
                Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4.0f, (float)game.GraphicsDevice.BackBuffer.Width / game.GraphicsDevice.BackBuffer.Height, 0.1f, 100.0f),
                World = Matrix.Identity
            };

            inputLayout = VertexInputLayout.FromBuffer<VertexPositionColor>(0, (Buffer<VertexPositionColor>)vertices);
            this.game = game;
        }

        // init corner points
        private void initHeights()
        {
            int edge = SIZE - 1;
            heights[0, 0] = random.NextFloat(-INIT_DIFF, INIT_DIFF);
            heights[0, edge] = random.NextFloat(-INIT_DIFF, INIT_DIFF);
            heights[edge, 0] = random.NextFloat(-INIT_DIFF, INIT_DIFF);
            heights[edge, edge] = random.NextFloat(-INIT_DIFF, INIT_DIFF);

            initHeight(0, 0, edge, edge, DIFF);
        }

        // init square
        // (row1, col1) is the top left corner
        // (row2, col2) is the bottom right corner
        private void initHeight(int row1, int col1, int row2, int col2, float diff)
        {
            int interRow = (row1 + row2) / 2;
            int interCol = (col1 + col2) / 2;
            // init edge points
            heights[row1, interCol] = (heights[row1, col1] + heights[row1, col2]) / 2;
            heights[interRow, col1] = (heights[row1, col1] + heights[row2, col1]) / 2;
            heights[interRow, col2] = (heights[row1, col2] + heights[row2, col2]) / 2;
            heights[row2, interCol] = (heights[row2, col1] + heights[row2, col2]) / 2;

            // init mid point
            heights[interRow, interCol] =
                (heights[row1, interCol] + heights[interRow, col1] + heights[interRow, col2] + heights[interRow, interCol]) / 4
                    + random.NextFloat(-diff, diff);

            if(row2 - row1 > 1){
                // if row2 - row1 > 0, then there are more sub squres
                // otherwise, job finished
                initHeight(row1, col1, interRow, interCol, REDUCING_FACTOR * diff);
                initHeight(row1, interCol, interRow, col2, REDUCING_FACTOR * diff);
                initHeight(interRow, col1, row2, interCol, REDUCING_FACTOR * diff);
                initHeight(interRow, interCol, row2, col2, REDUCING_FACTOR * diff);
            }
        }

        private void initVertices(VertexPositionColor[] vpc)
        {
            int max = SIZE / 2;
            int min = -max;
            int counter = 0;
            float height;
            Color color;

            for (int i = 0; i < SIZE - 1; ++i)
            {
                for (int j = 0; j < SIZE - 1; ++j)
                {
                    height = heights[i + 1, j] + heights[i, j] + heights[i, j + 1];
                    color = colorHeight(height);
                    //color = colorHeight(heights[i + 1, j]);
                    vpc[counter++] = new VertexPositionColor(new Vector3((float)min + i + 1, heights[i + 1, j], (float)min + j), color);
                    //color = colorHeight(heights[i, j]);
                    vpc[counter++] = new VertexPositionColor(new Vector3((float)min + i, heights[i, j], (float)min + j), color);
                    //color = colorHeight(heights[i, j + 1]);
                    vpc[counter++] = new VertexPositionColor(new Vector3((float)min + i, heights[i, j + 1], (float)min + j + 1), color);
                    height = heights[i + 1, j] + heights[i, j + 1] + heights[i + 1, j + 1];
                    color = colorHeight(height);
                    //color = colorHeight(heights[i + 1, j]);
                    vpc[counter++] = new VertexPositionColor(new Vector3((float)min + i + 1, heights[i + 1, j], (float)min + j), color);
                    //color = colorHeight(heights[i, j + 1]);
                    vpc[counter++] = new VertexPositionColor(new Vector3((float)min + i, heights[i, j + 1], (float)min + j + 1), color);
                    //color = colorHeight(heights[i + 1, j + 1]);
                    vpc[counter++] = new VertexPositionColor(new Vector3((float)min + i + 1, heights[i + 1, j + 1], (float)min + j + 1), color);
                }
            }
        }

        private Color colorHeight(float height)
        {
            if (height < 0.1 * DIFF && height > 0.1 * -DIFF)
            {
                return Color.Orange;
            }
            else if (height <= 0.1 * -DIFF && height > 0.25 * -DIFF)
            {
                return Color.DeepSkyBlue;
            }
            else if (height <= 0.25 * -DIFF && height > 0.35 * -DIFF)
            {
                return Color.Blue;
            }
            else if (height <= 0.35 * -DIFF)
            {
                return Color.DarkBlue;
            }
            else if (height >= 0.1 * DIFF && height < 0.35 * DIFF)
            {
                return Color.Green;
            }
            else if (height >= 0.35 * DIFF && height < 0.6 * DIFF)
            {
                return Color.SaddleBrown;
            }
            else
            {
                return Color.White;
            }
        }

        public override void Update(GameTime gameTime)
        {
            hVelocity += hAcc;

            hRotation += hVelocity;
            float temp = vRotation + vVelocity;
            if (temp < -Math.PI / 12 && temp > -Math.PI / 2)
            {
                vRotation += vVelocity;
            }

            if (Math.Abs(hVelocity) > THRESHOLD)
            {
                hVelocity *= FRACTION;
            }
            else
            {
                hVelocity = 0;
            }

            basicEffect.World = Matrix.RotationY(hRotation) * Matrix.RotationX(vRotation) * Matrix.Translation(0, 0, SIZE);
            // Rotate the cube.
            // var time = (float)gameTime.TotalGameTime.TotalSeconds;
            // basicEffect.World = Matrix.RotationX(time) * Matrix.RotationY(time * 2.0f) * Matrix.RotationZ(time * .7f);
            basicEffect.Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4.0f, (float)game.GraphicsDevice.BackBuffer.Width / game.GraphicsDevice.BackBuffer.Height, 0.1f, SIZE * 2);
        }

        public override void Control(KeyboardState ks)
        {
            if (ks.IsKeyDown(Keys.Right))
            {
                hAcc = -ROTATION;
            }
            else if (ks.IsKeyDown(Keys.Left))
            {
                hAcc = ROTATION;
            }
            else
            {
                hAcc = 0;
            }

            if (ks.IsKeyDown(Keys.Up))
            {
                vVelocity = -VROTATION;
            }
            else if (ks.IsKeyDown(Keys.Down))
            {
                vVelocity = VROTATION;
            }
            else
            {
                vVelocity = 0;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            // Setup the vertices
            game.GraphicsDevice.SetVertexBuffer<VertexPositionColor>((Buffer<VertexPositionColor>)vertices);
            game.GraphicsDevice.SetVertexInputLayout(inputLayout);

            // Apply the basic effect technique and draw the rotating cube
            basicEffect.CurrentTechnique.Passes[0].Apply();
            game.GraphicsDevice.Draw(PrimitiveType.TriangleList, vertices.ElementCount);
        }
    }
}
