using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SharpDX;
using SharpDX.Toolkit;

namespace Project2
{
    using SharpDX.Toolkit.Graphics;

    public class Landscape2 : ColoredGameObject
    {
        /*board properties*/
        private static int BOARD_SIZE = 513;                            //Work best at 513x513
        private float MAX_HEIGHT;                                       //Setting the maximum height, a random value between INIT_MIN_HEIGHT and INIT_MAX_HEIGHT
        public float[,] pHeights;                                       //2D array storing the heights for corresponding (x,z)
        private VertexPositionColor[] vpc;                              //Vertices list generated from the 2D array

        /*landscape properties*/
        private float INIT_MIN_HEIGHT = BOARD_SIZE / 100;
        private float INIT_MAX_HEIGHT = BOARD_SIZE / 20;
        private float ROUGHNESS = BOARD_SIZE / 50;                      //How rough the terrain is, 1 is super flat, 20 is rocky mountain range. Default = 10
        private float GBIGSIZE = 2 * BOARD_SIZE;                        //Normalizing factor for displacement
        private float HIGHEST_POINT = 0;                                //Calculating the highest point
        private float COLOUR_SCALE = BOARD_SIZE / 4;                    //A colour scale for calculating colours
        private float smoothingFactor = 3;                              //Determines how smooth the landscape is
        private int flatOffset = BOARD_SIZE / 100;                      //Value determines how smooth the landscape is
        public int minPlayable = BOARD_SIZE / 10;                       //Minimum x or z value that any GameObject could be placed
        public int maxPlayable = 9 * BOARD_SIZE / 10;                   //Maximum x or z value that any GameObject could be placed
        private int minimumDistance = 2 * BOARD_SIZE / 10;              //Minimum distance between golf ball and hole
        private const float MIN_PROBABILITY = 0.1f;
        private const float MAX_PROBABILITY = 1f;

        /*auxiliary members*/
        Random rnd = new Random();                                      //Initialize a Random object
        public Vector3 startPos { get; private set; }                   //Position where the golf ball should start
        public Vector3 objectivePos { get; private set; }               //Position where the golf ball should land 
        

        public Landscape2(Project2Game game)
        {
            MAX_HEIGHT = rnd.NextFloat(INIT_MIN_HEIGHT, INIT_MAX_HEIGHT);      //Randomize the height

            //initlize the world
            vpc = InitializeGrid();
            vertices = Buffer.Vertex.New<VertexPositionColor>(game.GraphicsDevice, vpc);

            basicEffect = new BasicEffect(game.GraphicsDevice)
            {
                VertexColorEnabled = true,
                World = Matrix.Identity,
                View = game.camera.View,
                Projection = game.camera.Projection
            };

            inputLayout = VertexInputLayout.FromBuffer<VertexPositionColor>(0, (Buffer<VertexPositionColor>) vertices);
            this.game = game;
        }

        public override void Update(GameTime gameTime)
        {
            basicEffect.View = game.camera.View;
            basicEffect.Projection = game.camera.Projection;
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

        /**
         Initialization function which starts the random landscape algorithm
         */
        public VertexPositionColor[] InitializeGrid()
        {
            float h1, h2, h3, h4;
            pHeights = new float[BOARD_SIZE, BOARD_SIZE];
            VertexPositionColor[] vertices = new VertexPositionColor[BOARD_SIZE * BOARD_SIZE * 6];
            //Initialize the four starting corners
            h1 = rnd.NextFloat(0, MAX_HEIGHT);
            h2 = rnd.NextFloat(0, MAX_HEIGHT);
            h3 = rnd.NextFloat(0, MAX_HEIGHT);
            h4 = rnd.NextFloat(0, MAX_HEIGHT);

            //Start populating the array using a hybrid midpoint displacement and diamond square algorithm
            DivideVertices(ref pHeights, 0, 0, BOARD_SIZE - 1, h1, h2, h3, h4);

            //average the landscape to remove sharp drop
            for (int z = 0; z < smoothingFactor; z++)
            {
                for (int i = flatOffset; i < BOARD_SIZE - flatOffset; i++)
                {
                    for (int j = flatOffset; j < BOARD_SIZE - flatOffset; j++)
                    {
                        pHeights[i, j] = (pHeights[i, j - flatOffset] + pHeights[i - flatOffset, j] + pHeights[i, j + flatOffset] + pHeights[i + flatOffset, j]) / 4f;
                    }
                }
            }

            //Now convert the array into vertices
            int k = 0;
            for (int i = 0; i < BOARD_SIZE - 1; i++)
            {
                for (int j = 0; j < BOARD_SIZE - 1; j++)
                {
                    vertices[k++] = new VertexPositionColor(new Vector3(i, flatOcean(pHeights[i, j]), j), GetColor(pHeights[i, j]));
                    vertices[k++] = new VertexPositionColor(new Vector3((i + 1), flatOcean(pHeights[i + 1, j + 1]), (j + 1)), GetColor(pHeights[i + 1, j + 1]));
                    vertices[k++] = new VertexPositionColor(new Vector3((i + 1), flatOcean(pHeights[i + 1, j]), j), GetColor(pHeights[i + 1, j]));
                    vertices[k++] = new VertexPositionColor(new Vector3(i, flatOcean(pHeights[i, j]), j), GetColor(pHeights[i, j]));
                    vertices[k++] = new VertexPositionColor(new Vector3(i, flatOcean(pHeights[i, j + 1]), (j + 1)), GetColor(pHeights[i, j + 1]));
                    vertices[k++] = new VertexPositionColor(new Vector3((i + 1), flatOcean(pHeights[i + 1, j + 1]), (j + 1)), GetColor(pHeights[i + 1, j + 1]));
                }
            }

            //generates the position of the ball and the hole
            generateRandomStartObjectivePos();
            return vertices;
        }

        /**
         *  Generats a random height map
         */
        public void DivideVertices(ref float[,] points, int x, int y, int width, float h1, float h2, float h3, float h4)
        {
            //Base case, if the width between two points is less than 1, stop
            if (width < 1)
                return;
            float newp1, newp2, newp3, newp4, ncen = 0, tempDisplacement;
            int newWidth = width / 2;
            points[x, y] = h1;
            points[x, y + width] = h2;
            points[x + width, y + width] = h3;
            points[x + width, y] = h4;

            //calculate the random displacement value of the center point
            tempDisplacement = displace(width);
            //update highest point
            if ((h1 + h2 + h3 + h4) / 4 + tempDisplacement > HIGHEST_POINT)
                HIGHEST_POINT = (h1 + h2 + h3 + h4) / 4 + tempDisplacement;
            ncen = (h1 + h2 + h3 + h4) / 4 + tempDisplacement;

            points[x + newWidth, y + newWidth] = ncen;

            //Calculate the average using the hybrid center calculation. Since I've used recursion, I've
            //ran into the trouble that one of the four points around the point I want to average has not
            //yet been initialized, so the hybrid version make sure that I don't include the uninitalized one
            newp1 = getDiamondAverage(ref points, x, y + newWidth, newWidth);
            newp2 = getDiamondAverage(ref points, x + newWidth, y + width, newWidth);
            newp3 = getDiamondAverage(ref points, x + width, y + newWidth, newWidth);
            newp4 = getDiamondAverage(ref points, x + newWidth, y, newWidth);
            points[x, y + newWidth] = newp1;
            points[x + newWidth, y + width] = newp2;
            points[x + width, y + newWidth] = newp3;
            points[x + newWidth, y] = newp4;

            //Recursively call itself
            DivideVertices(ref points, x, y, newWidth, h1, newp1, ncen, newp4);
            DivideVertices(ref points, x, y + newWidth, newWidth, newp1, h2, newp2, ncen);
            DivideVertices(ref points, x + newWidth, y + newWidth, newWidth, ncen, newp2, h3, newp3);
            DivideVertices(ref points, x + newWidth, y, newWidth, newp4, ncen, newp3, h4);
        }

        /**
         * This function flats the ocean, there will only be colour effect for different
           depth, but they are physically the same height. Just so it looks more realistic.
           It also flatten the beach near the ocean
         */
        private float flatOcean(float height) {
            if (height <= COLOUR_SCALE * 0.1)
            {
                return COLOUR_SCALE * 0.1f;
            }
            return height;
        }

        /**
         *  Calculates the colour of landscape based on height
         */
        public Color GetColor(float height) {
            if (height >= COLOUR_SCALE) {
                return Color.White;
            }
            if (height < COLOUR_SCALE && height >= COLOUR_SCALE * 0.97)
                return new Color(new Vector4(255f/255, 250f/255, 250f/255,1));
            if (height < COLOUR_SCALE * 0.97 && height >= COLOUR_SCALE * 0.92)
                return new Color(new Vector4(211f/255, 211f/255, 211f/255,1));
            if (height < COLOUR_SCALE * 0.92 && height >= COLOUR_SCALE * 0.85)
                return new Color(new Vector4(192f/255, 192f/255, 192f/255,1));
            if (height < COLOUR_SCALE * 0.85 && height >= COLOUR_SCALE * 0.8)
                return new Color(new Vector4(192f/255,192f/255,192f/255,1));
            if (height < COLOUR_SCALE * 0.8 && height >= COLOUR_SCALE * 0.75)
                return new Color(new Vector4(211f/255,211f/255,211f/255,1));
            if (height < COLOUR_SCALE * 0.75 && height >= COLOUR_SCALE * 0.7)
                return new Color(new Vector4(128f/255, 128f/255, 128f/255,1));
            if (height < COLOUR_SCALE * 0.7 && height >= COLOUR_SCALE * 0.68)
                return new Color(new Vector4(139f / 255, 69f / 255, 19f / 255, 1));
            if (height < COLOUR_SCALE * 0.68 && height >= COLOUR_SCALE * 0.6)
                return new Color(new Vector4(0 / 255, 100f / 255, 0 / 255, 1));
            if (height < COLOUR_SCALE * 0.6 && height >= COLOUR_SCALE * 0.5)
                return new Color(new Vector4(34f / 255, 139f / 255, 34f / 255, 1));
            if (height < COLOUR_SCALE * 0.5 && height >= COLOUR_SCALE * 0.45)
                return new Color(new Vector4(0 / 255, 100f / 255, 0 / 255, 1));
            if (height < COLOUR_SCALE * 0.45 && height >= COLOUR_SCALE * 0.43)
                return new Color(new Vector4(34f / 255, 139f / 255, 34f / 255, 1));
            if (height < COLOUR_SCALE * 0.43 && height >= COLOUR_SCALE * 0.4)
                return new Color(new Vector4(0 / 255, 128f / 255, 0 / 255, 1));
            if (height < COLOUR_SCALE * 0.4 && height >= COLOUR_SCALE * 0.35)
                return new Color(new Vector4(34f / 255, 139f / 255, 34f / 255, 1));
            if (height < COLOUR_SCALE * 0.35 && height >= COLOUR_SCALE * 0.31)
                return new Color(new Vector4(85f / 255, 107f / 255, 47f / 255, 1));
            if (height < COLOUR_SCALE * 0.31 && height >= COLOUR_SCALE * 0.24)
                return new Color(new Vector4(107f / 255, 142f / 255, 35f / 255, 1));
            if (height < COLOUR_SCALE * 0.24 && height >= COLOUR_SCALE * 0.18)
                return new Color(new Vector4(50f / 255, 205f / 255, 50f / 255, 1));
            if (height < COLOUR_SCALE * 0.18 && height >= COLOUR_SCALE * 0.16)
                return new Color(new Vector4(154f / 255, 205f / 255, 50f / 255, 1));
            if (height < COLOUR_SCALE * 0.16 && height >= COLOUR_SCALE * 0.14)
                return new Color(new Vector4(173f / 255, 255 / 255, 47f / 255, 1));
            if (height < COLOUR_SCALE * 0.14 && height >= COLOUR_SCALE * 0.12)
                return new Color(new Vector4(255 / 255, 255 / 255, 34f / 255, 1));
            if (height < COLOUR_SCALE * 0.12 && height >= COLOUR_SCALE * 0.1)
                return new Color(new Vector4(255 / 255, 215f / 255, 0f / 255, 1));
            //Water below this point
            if (height < COLOUR_SCALE * 0.1 && height >= COLOUR_SCALE * 0.09)
                return new Color(new Vector4(127f / 255, 255f / 255, 212f / 255, 1));
            if (height < COLOUR_SCALE * 0.09 && height >= COLOUR_SCALE * 0.05)
                return new Color(new Vector4(70f / 255, 130f / 255, 180f / 255, 1));
            if (height < COLOUR_SCALE * 0.05)
                return new Color(new Vector4(0 / 255, 0 / 255, 139f / 255, 1));
            return Color.DarkBlue;

        }

        /**
         * Auxiliary average function for the diamond-square algorithm
         */
        private float getDiamondAverage(ref float[,] points, int x, int y, int width) {
            int counter = 0;
            float totalHeight = 0;
            if (isInside(x - width, y) && points[x - width, y] != 0f)
            {
                totalHeight += points[x - width,y];
                counter++;
            }
            if (isInside(x, y + width) && points[x, y + width] != 0f)
            {
                totalHeight += points[x, y + width];
                counter++;
            }
            if (isInside(x + width, y) && points[x + width, y] != 0f)
            {
                totalHeight += points[x + width, y];
                counter++;
            }
            if (isInside(x, y - width) && points[x, y - width] != 0f)
            {
                totalHeight += points[x, y - width];
                counter++;
            }
            if (counter == 0)
                counter = 1;
            return totalHeight/counter;
        }

        /**
         *  Checks if x,y is inside the board
         */
        private bool isInside(int x, int y) {
            return x <= BOARD_SIZE - 1 && x >= 0 && y <= BOARD_SIZE - 1 && y >= 0;
        }

        /**
         taken from internet, but modified how the Random value is calculated, this seems to generate more
         "interesting" landscape.
         */
        private float displace(float smallsize) {
            float max = smallsize * ROUGHNESS / GBIGSIZE;
            return rnd.NextFloat(-MAX_HEIGHT * MIN_PROBABILITY, MAX_HEIGHT * MAX_PROBABILITY) * max;
        }

        /**
         * Generates a random golf ball position and hole position
         */
        private void generateRandomStartObjectivePos() {
            //Get starting pos
            bool unsuccessful = true;
            int tempX1, tempZ1, tempX2, tempZ2;
            tempX1 = tempX2 = tempZ1 = tempZ2 = 0;
            while (unsuccessful) {
                tempX1 = rnd.Next(minPlayable, maxPlayable);
                tempZ1 = rnd.Next(minPlayable, maxPlayable);
                if (isSafePosition(tempX1, tempZ1))
                {
                    unsuccessful = false;
                }
            }

            //Get objective pos
            unsuccessful = true;
            while (unsuccessful) {
                tempX2 = rnd.Next(minPlayable, maxPlayable);
                tempZ2 = rnd.Next(minPlayable, maxPlayable);
                if ( isSafePosition(tempX2, tempZ2) && 
                    (Math.Abs(tempX2 - tempX1) > minimumDistance || Math.Abs(tempZ2 - tempZ1) > minimumDistance)) {
                    unsuccessful = false;
                }
            }
            startPos = new Vector3(tempX1, flatOcean(pHeights[tempX1, tempZ1]), tempZ1);
            objectivePos = new Vector3(tempX2, flatOcean(pHeights[tempX2, tempZ2]), tempZ2);
        }

        /**
         *  Checks if (x,z) is water
         */
        public Boolean isWater(int x, int z) {
            if (!isInside(x, z)) {
                return false;
            }
            return pHeights[x, z] <= COLOUR_SCALE * 0.1;
        }

        /**
         *  Checks if (x,z) is at least some distance away from water
         */
        private Boolean isSafePosition(int x, int z) {
            return pHeights[x, z] > COLOUR_SCALE * 0.12;
        }
    }
}
