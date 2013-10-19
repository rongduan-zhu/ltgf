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
        private const int BOARD_SIZE = 513;                      //Work best at 513x513
        private float MAX_HEIGHT;                                       //Setting the maximum height, a random value between INIT_MIN_HEIGHT and INIT_MAX_HEIGHT
        public float[,] pHeights;                                       //2D array storing the heights for corresponding (x,z)
        private VertexPositionNormalColor[] vpc;                              //Vertices list generated from the 2D array

        /*landscape properties*/
        private float INIT_MIN_HEIGHT = BOARD_SIZE / 50;
        private float INIT_MAX_HEIGHT = BOARD_SIZE / 20;
        private float ROUGHNESS = BOARD_SIZE / 40;                      //How rough the terrain is, 1 is super flat, 20 is rocky mountain range. Default = 10
        private float GBIGSIZE = 2 * BOARD_SIZE;                        //Normalizing factor for displacement
        private float HIGHEST_POINT = 0;                                //Calculating the highest point
        private float COLOUR_SCALE = BOARD_SIZE / 4;                    //A colour scale for calculating colours
        private float smoothingFactor = 6;                              //Determines how smooth the landscape is
        private int flatOffset = BOARD_SIZE / 100;                      //Value determines how smooth the landscape is
        public int minPlayable = BOARD_SIZE / 10;                       //Minimum x or z value that any GameObject could be placed
        public int maxPlayable = 9 * BOARD_SIZE / 10;                   //Maximum x or z value that any GameObject could be placed
        private int minimumDistance = 2 * BOARD_SIZE / 10;              //Minimum distance between golf ball and hole
        private float min_probability = 0.1f;                           //Minimum percentage of max height value of the range of the height generator
        private float max_probability = 1.2f;                           //Maximum percentage of max height value of the range of the height generator
        private const float BACK_FACE_ALPHA = 0.5f;                     //Back face transparency value

        private float totalLand;                                        //Total number of points which is land
        private float totalPoints;                                      //Total number of initialized points
        private float landRatio;                                        //Portion of land
        private const float OPTIMAL_RATIO = 0.7f;                       //Optimal portion of land
        private const float UPPERBOUND_RATIO = 0.9f;                    //Upper bound portion of land
        private const float LOWERBOUND_RATIO = 0.5f;                    //Lower bound portion of land

        /*auxiliary members*/
        Random rnd = new Random();                                      //Initialize a Random object
        public Vector3 startPos { get; private set; }                   //Position where the golf ball should start
        public Vector3 objectivePos { get; private set; }               //Position where the golf ball should land 
        

        public Landscape2(Project2Game game)
        {
            MAX_HEIGHT = rnd.NextFloat(INIT_MIN_HEIGHT, INIT_MAX_HEIGHT);      //Randomize the height
            
            //initlize the world
            vpc = InitializeGrid();
            vertices = Buffer.Vertex.New<VertexPositionNormalColor>(game.GraphicsDevice, vpc);

            effect = game.Content.Load<Effect>("Phong");

            effect.Parameters["World"].SetValue(Matrix.Identity);
            effect.Parameters["Projection"].SetValue(game.camera.Projection);
            effect.Parameters["worldInvTrp"].SetValue(Matrix.Transpose(Matrix.Invert(Matrix.Identity)));
            effect.Parameters["maxHeight"].SetValue(COLOUR_SCALE);

            inputLayout = VertexInputLayout.FromBuffer<VertexPositionNormalColor>(0, (Buffer<VertexPositionNormalColor>) vertices);
            this.game = game;
        }

        public override void Update(GameTime gameTime)
        {
            effect.Parameters["View"].SetValue(game.camera.View);
            effect.Parameters["cameraPos"].SetValue(game.camera.RealPosition);
        }
        
        public override void Draw(GameTime gameTime)
        {
            // Setup the vertices
            game.GraphicsDevice.SetVertexBuffer<VertexPositionNormalColor>((Buffer<VertexPositionNormalColor>)vertices);
            game.GraphicsDevice.SetVertexInputLayout(inputLayout);

            // Apply the basic effect technique and draw the rotating cube
            //basicEffect.CurrentTechnique.Passes[0].Apply();
            effect.CurrentTechnique.Passes[0].Apply();
            game.GraphicsDevice.Draw(PrimitiveType.TriangleList, vertices.ElementCount);
        }

        /**
         Initialization function which starts the random landscape algorithm
         */
        public VertexPositionNormalColor[] InitializeGrid()
        {
            float h1, h2, h3, h4;
            pHeights = new float[BOARD_SIZE, BOARD_SIZE];
            VertexPositionNormalColor[] vertices = new VertexPositionNormalColor[BOARD_SIZE * BOARD_SIZE * 6 * 2];
            //Initialize the four starting corners
            h1 = rnd.NextFloat(-MAX_HEIGHT, MAX_HEIGHT);
            h2 = rnd.NextFloat(-MAX_HEIGHT, MAX_HEIGHT);
            h3 = rnd.NextFloat(-MAX_HEIGHT, MAX_HEIGHT);
            h4 = rnd.NextFloat(-MAX_HEIGHT, MAX_HEIGHT);

            landscapeShapeGuard(h1, h2, h3, h4);

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

            //average the landscape to remove sharp drop
            for (int z = 0; z < smoothingFactor; z++)
            {
                for (int i = 1; i < BOARD_SIZE - 1; i++)
                {
                    for (int j = 1; j < BOARD_SIZE - 1; j++)
                    {
                        pHeights[i, j] = (pHeights[i, j - 1] + pHeights[i - 1, j] + pHeights[i, j + 1] + pHeights[i + 1, j]) / 4f;
                    }
                }
            }

            //Now convert the array into vertices
            int k = 0;
            for (int i = 0; i < BOARD_SIZE - 1; i++)
            {
                for (int j = 0; j < BOARD_SIZE - 1; j++)
                {
                    Vector3 normal = vertexNormal(i, flatOcean(pHeights[i, j]), j);
                    vertices[k++] = new VertexPositionNormalColor(new Vector3(i, flatOcean(pHeights[i, j]), j), 
                        normal, GetColor(pHeights[i, j]));

                    normal = vertexNormal(i + 1, flatOcean(pHeights[i + 1, j + 1]), j + 1);
                    vertices[k++] = new VertexPositionNormalColor(new Vector3((i + 1), flatOcean(pHeights[i + 1, j + 1]), (j + 1)), 
                        normal, GetColor(pHeights[i + 1, j + 1]));
                    
                    normal = vertexNormal(i + 1, flatOcean(pHeights[i + 1, j]), j);
                    vertices[k++] = new VertexPositionNormalColor(new Vector3((i + 1), flatOcean(pHeights[i + 1, j]), j), 
                        normal, GetColor(pHeights[i + 1, j]));

                    normal = vertexNormal(i, flatOcean(pHeights[i, j]), j);
                    vertices[k++] = new VertexPositionNormalColor(new Vector3(i, flatOcean(pHeights[i, j]), j), 
                        normal, GetColor(pHeights[i, j]));

                    normal = vertexNormal(i, flatOcean(pHeights[i, j + 1]), j + 1);
                    vertices[k++] = new VertexPositionNormalColor(new Vector3(i, flatOcean(pHeights[i, j + 1]), (j + 1)), 
                        normal, GetColor(pHeights[i, j + 1]));

                    normal = vertexNormal(i + 1, flatOcean(pHeights[i + 1, j + 1]), j + 1);
                    vertices[k++] = new VertexPositionNormalColor(new Vector3((i + 1), flatOcean(pHeights[i + 1, j + 1]), (j + 1)), 
                        normal, GetColor(pHeights[i + 1, j + 1]));

                    //backface
                    vertices[k++] = new VertexPositionNormalColor(new Vector3(i, flatOcean(pHeights[i, j]), j),
                        normal, GetColor(pHeights[i, j], BACK_FACE_ALPHA));

                    normal = vertexNormal(i + 1, flatOcean(pHeights[i + 1, j]), j);
                    vertices[k++] = new VertexPositionNormalColor(new Vector3((i + 1), flatOcean(pHeights[i + 1, j]), j),
                        normal, GetColor(pHeights[i + 1, j], BACK_FACE_ALPHA));

                    normal = vertexNormal(i + 1, flatOcean(pHeights[i + 1, j + 1]), j + 1);
                    vertices[k++] = new VertexPositionNormalColor(new Vector3((i + 1), flatOcean(pHeights[i + 1, j + 1]), (j + 1)),
                        normal, GetColor(pHeights[i + 1, j + 1], BACK_FACE_ALPHA));

                    normal = vertexNormal(i, flatOcean(pHeights[i, j]), j);
                    vertices[k++] = new VertexPositionNormalColor(new Vector3(i, flatOcean(pHeights[i, j]), j),
                        normal, GetColor(pHeights[i, j], BACK_FACE_ALPHA));

                    normal = vertexNormal(i + 1, flatOcean(pHeights[i + 1, j + 1]), j + 1);
                    vertices[k++] = new VertexPositionNormalColor(new Vector3((i + 1), flatOcean(pHeights[i + 1, j + 1]), (j + 1)),
                        normal, GetColor(pHeights[i + 1, j + 1], BACK_FACE_ALPHA));

                    normal = vertexNormal(i, flatOcean(pHeights[i, j + 1]), j + 1);
                    vertices[k++] = new VertexPositionNormalColor(new Vector3(i, flatOcean(pHeights[i, j + 1]), (j + 1)),
                        normal, GetColor(pHeights[i, j + 1], BACK_FACE_ALPHA));
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

            landscapeShapeGuard(newp1, newp2, newp3, newp4, ncen);

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
        public Color GetColor(float height, float alpha = 1) {
            if (height >= COLOUR_SCALE) {
                return new Color(new Vector4(255f / 255, 255f / 255, 255f / 255, alpha));
            }
            if (height < COLOUR_SCALE && height >= COLOUR_SCALE * 0.97)
                return new Color(new Vector4(255f/255, 250f/255, 250f/255, alpha));
            if (height < COLOUR_SCALE * 0.97 && height >= COLOUR_SCALE * 0.92)
                return new Color(new Vector4(211f/255, 211f/255, 211f/255, alpha));
            if (height < COLOUR_SCALE * 0.92 && height >= COLOUR_SCALE * 0.85)
                return new Color(new Vector4(192f/255, 192f/255, 192f/255, alpha));
            if (height < COLOUR_SCALE * 0.85 && height >= COLOUR_SCALE * 0.8)
                return new Color(new Vector4(192f/255,192f/255,192f/255, alpha));
            if (height < COLOUR_SCALE * 0.8 && height >= COLOUR_SCALE * 0.75)
                return new Color(new Vector4(211f/255,211f/255,211f/255, alpha));
            if (height < COLOUR_SCALE * 0.75 && height >= COLOUR_SCALE * 0.7)
                return new Color(new Vector4(128f/255, 128f/255, 128f/255, alpha));
            if (height < COLOUR_SCALE * 0.7 && height >= COLOUR_SCALE * 0.68)
                return new Color(new Vector4(139f / 255, 69f / 255, 19f / 255, alpha));
            if (height < COLOUR_SCALE * 0.68 && height >= COLOUR_SCALE * 0.6)
                return new Color(new Vector4(0 / 255, 100f / 255, 0 / 255, alpha));
            if (height < COLOUR_SCALE * 0.6 && height >= COLOUR_SCALE * 0.5)
                return new Color(new Vector4(34f / 255, 139f / 255, 34f / 255, alpha));
            if (height < COLOUR_SCALE * 0.5 && height >= COLOUR_SCALE * 0.45)
                return new Color(new Vector4(0 / 255, 100f / 255, 0 / 255, alpha));
            if (height < COLOUR_SCALE * 0.45 && height >= COLOUR_SCALE * 0.43)
                return new Color(new Vector4(34f / 255, 139f / 255, 34f / 255, alpha));
            if (height < COLOUR_SCALE * 0.43 && height >= COLOUR_SCALE * 0.4)
                return new Color(new Vector4(0 / 255, 128f / 255, 0 / 255, alpha));
            if (height < COLOUR_SCALE * 0.4 && height >= COLOUR_SCALE * 0.35)
                return new Color(new Vector4(34f / 255, 139f / 255, 34f / 255, alpha));
            if (height < COLOUR_SCALE * 0.35 && height >= COLOUR_SCALE * 0.31)
                return new Color(new Vector4(85f / 255, 107f / 255, 47f / 255, alpha));
            if (height < COLOUR_SCALE * 0.31 && height >= COLOUR_SCALE * 0.24)
                return new Color(new Vector4(107f / 255, 142f / 255, 35f / 255, alpha));
            if (height < COLOUR_SCALE * 0.24 && height >= COLOUR_SCALE * 0.18)
                return new Color(new Vector4(50f / 255, 205f / 255, 50f / 255, alpha));
            if (height < COLOUR_SCALE * 0.18 && height >= COLOUR_SCALE * 0.16)
                return new Color(new Vector4(154f / 255, 205f / 255, 50f / 255, alpha));
            if (height < COLOUR_SCALE * 0.16 && height >= COLOUR_SCALE * 0.14)
                return new Color(new Vector4(173f / 255, 255 / 255, 47f / 255, alpha));
            if (height < COLOUR_SCALE * 0.14 && height >= COLOUR_SCALE * 0.12)
                return new Color(new Vector4(255 / 255, 255 / 255, 34f / 255, alpha));
            if (height < COLOUR_SCALE * 0.12 && height >= COLOUR_SCALE * 0.1)
                return new Color(new Vector4(255 / 255, 215f / 255, 0f / 255, alpha));
            //Water below this point
            if (height < COLOUR_SCALE * 0.1 && height >= COLOUR_SCALE * 0.09)
                return new Color(new Vector4(127f / 255, 255f / 255, 212f / 255, alpha));
            if (height < COLOUR_SCALE * 0.09 && height >= COLOUR_SCALE * 0.05)
                return new Color(new Vector4(70f / 255, 130f / 255, 180f / 255, alpha));
            if (height < COLOUR_SCALE * 0.05)
                return new Color(new Vector4(0 / 255, 0 / 255, 139f / 255, alpha));
            return new Color(new Vector4(0 / 255, 0 / 255, 255f / 255, alpha));

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
        public bool isInside(float x, float y) {
            return x <= BOARD_SIZE - 1 && x >= 0 && y <= BOARD_SIZE - 1 && y >= 0;
        }

        /**
         taken from internet, but modified how the Random value is calculated, this seems to generate more
         "interesting" landscape.
         */
        private float displace(float smallsize) {
            float max = smallsize * ROUGHNESS / GBIGSIZE;
            return rnd.NextFloat(-MAX_HEIGHT * min_probability, MAX_HEIGHT * max_probability) * max;
        }

        /**
         * Generates a random golf ball position and hole position
         */
        private void generateRandomStartObjectivePos() {
            //Get starting pos
            bool unsuccessful = true;
            int tempX1, tempZ1, tempX2, tempZ2;
            tempX1 = tempX2 = tempZ1 = tempZ2 = 0;
            //while (unsuccessful) {
                tempX1 = rnd.Next(minPlayable, maxPlayable);
                tempZ1 = rnd.Next(minPlayable, maxPlayable);
                if (isSafePosition(tempX1, tempZ1))
                {
                    unsuccessful = false;
                }
            //}

            //Get objective pos
            unsuccessful = true;
            //while (unsuccessful) {
                tempX2 = rnd.Next(minPlayable, maxPlayable);
                tempZ2 = rnd.Next(minPlayable, maxPlayable);
                if ( isSafePosition(tempX2, tempZ2) && 
                    (Math.Abs(tempX2 - tempX1) > minimumDistance || Math.Abs(tempZ2 - tempZ1) > minimumDistance)) {
                    unsuccessful = false;
                }
            //}
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
            return pHeights[x, z] < COLOUR_SCALE * 0.1;
        }

        /**
         *  Checks if height is water level
         */
        private bool isWater(float height)
        {
            return height < COLOUR_SCALE * 0.1;
        }

        /**
         *  Checks if (x,z) is at least some distance away from water
         */
        private Boolean isSafePosition(int x, int z) {
            return pHeights[x, z] > COLOUR_SCALE * 0.12;
        }

        /**
         *  Calculates percentage of land
         */
        private float calcLandRatio() { 
            return totalLand / totalPoints;
        }

        /**
         *  Given these new heights, updates the total number of land points
         */
        private void updateLand(float h1, float h2, float h3, float h4, float h5 = 0) {
            if (!isWater(h1)) {
                totalLand++;
            }
            if (!isWater(h2))
            {
                totalLand++;
            }
            if (!isWater(h3))
            {
                totalLand++;
            }
            if (!isWater(h4))
            {
                totalLand++;
            }
            if (!isWater(h5)) 
            {
                totalLand++;
            }
        }

        /**
         *  If proportion of land is below threshold value, increase probability of getting land
         *  otherwise make probability of generating land and water equivalent
         */
        private void landscapeShapeGuard(float h1, float h2, float h3, float h4, float h5 = -float.MinValue) {
            updateLand(h1, h2, h3, h4, h5);
            if (h5 != -float.MinValue)
            {
                totalPoints += 5;
            }
            else {
                totalPoints += 4;
            }
            landRatio = totalLand / totalPoints;

            //If its lower than lower bound, make the probability of land extremely high
            if (landRatio < LOWERBOUND_RATIO)
            {
                max_probability = 1.5f;
                min_probability = -1f;
            }
            //If its higher than upper bound, make probability of land very low
            else if (landRatio > UPPERBOUND_RATIO) {
                max_probability = 0.1f;
                min_probability = 1f;
            }
            //If its less than optimal_ratio, just have land slightly higher than water
            else if (landRatio < OPTIMAL_RATIO) {
                max_probability = 1.5f;
                min_probability = 0.5f;
            }
            //We are safe, so make probability of water and land the same
            else
            {
                max_probability = 1f;
                min_probability = 1f;
            }
        }

        Vector3 vertexNormal(int x, float y, int z) {
            Vector3 n1, n2, n3, n4, n5, n6, center;
            n1 = new Vector3(0, 0, 0);
            n2 = new Vector3(0, 0, 0);
            n3 = new Vector3(0, 0, 0);
            n4 = new Vector3(0, 0, 0);
            n5 = new Vector3(0, 0, 0);
            n6 = new Vector3(0, 0, 0);
            center = new Vector3(x, pHeights[x, z], z);
            float counter = 0;
            //top right
            if (isInside(x - 1, z) && isInside(x, z + 1)) { 
                Vector3 top = new Vector3(x - 1, flatOcean(pHeights[x - 1, z]), z);
                Vector3 right = new Vector3(x, flatOcean(pHeights[x, z + 1]), z + 1);
                n1 = Vector3.Cross(top - center, right - center);
                counter++;
            }
            //right bottom
            if (isInside(x, z + 1) && isInside(x + 1, z + 1)) {
                Vector3 right = new Vector3(x, flatOcean(pHeights[x, z + 1]), z + 1);
                Vector3 bottom = new Vector3(x + 1, flatOcean(pHeights[x + 1, z + 1]), z + 1);
                n2 = Vector3.Cross(right - center, bottom - center);
                counter++;
            }
            //bottom right
            if (isInside(x + 1, z + 1) && isInside(x + 1, z))
            {
                Vector3 right = new Vector3(x + 1, flatOcean(pHeights[x + 1, z + 1]), z + 1);
                Vector3 bottom = new Vector3(x + 1, flatOcean(pHeights[x + 1, z]), z);
                n3 = Vector3.Cross(right - center, bottom - center);
                counter++;
            }
            //bottom left
            if (isInside(x + 1, z) && isInside(x, z - 1)) {
                Vector3 bottom = new Vector3(x, flatOcean(pHeights[x + 1, z]), z);
                Vector3 left = new Vector3(x, flatOcean(pHeights[x, z - 1]), z - 1);
                n4 = Vector3.Cross(bottom - center, left - center);
                counter++;
            }
            //left top
            if (isInside(x, z - 1) && isInside(x - 1, z - 1)) {
                Vector3 left = new Vector3(x, flatOcean(pHeights[x, z - 1]), z - 1);
                Vector3 top = new Vector3(x - 1, flatOcean(pHeights[x - 1, z - 1]), z - 1);
                n5 = Vector3.Cross(left - center, top - center);
                counter++;
            }
            //top left
            if (isInside(x - 1, z - 1) && isInside(x - 1, z))
            {
                Vector3 left = new Vector3(x - 1, flatOcean(pHeights[x - 1, z - 1]), z - 1);
                Vector3 top = new Vector3(x - 1, flatOcean(pHeights[x - 1, z]), z);
                n6 = Vector3.Cross(left - center, top - center);
                counter++;
            }

            if (counter < 1) {
                counter = 1;
            }
            return (n1 + n2 + n3 + n4 + n5 + n6) / counter;
        }

    }
}
