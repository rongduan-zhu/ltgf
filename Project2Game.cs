// Copyright (c) 2010-2013 SharpDX - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;

using SharpDX;
using SharpDX.Toolkit;

namespace Project2
{
    // Use this namespace here in case we need to use Direct3D11 namespace as well, as this
    // namespace will override the Direct3D11.
    using SharpDX.Toolkit.Graphics;
    using SharpDX.Toolkit.Input;

    public class Project2Game : Game
    {
        private GraphicsDeviceManager graphicsDeviceManager;
        private GameObject landscape;
        private Model ball, pin, arrow;
        private Stack<Model> models;
        private Camera camera;
        private KeyboardManager km;
        private KeyboardState ks;

        /// <summary>
        /// Initializes a new instance of the <see cref="Project2Game" /> class.
        /// </summary>
        public Project2Game()
        {
            // Creates a graphics manager. This is mandatory.
            graphicsDeviceManager = new GraphicsDeviceManager(this);

            // Setup the relative directory to the executable directory
            // for loading contents with the ContentManager
            Content.RootDirectory = "Content";
            models = new Stack<Model>(3);

            // Creates a keyboard manager
            km = new KeyboardManager(this);
        }

        protected override void LoadContent()
        {
            landscape = new Landscape2(this);
            arrow = Content.Load<Model>("Arrow");
            models.Push(arrow);
            ball = Content.Load<Model>("Ball");
            models.Push(ball);
            pin = Content.Load<Model>("Pin");
            models.Push(pin);

            foreach (var m in models)
            {
                BasicEffect.EnableDefaultLighting(m, true);
            }

            // Create an input layout from the vertices

            base.LoadContent();
        }

        protected override void Initialize()
        {
            Window.Title = "Project 2";

            base.Initialize();

            camera = new Camera(this);
        }

        protected override void Update(GameTime gameTime)
        {
            ks = km.GetState();
            landscape.Update(gameTime);
            landscape.Control(ks);

            // Handle base.Update
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // Clears the screen with the Color.CornflowerBlue
            GraphicsDevice.Clear(Color.Black);

            landscape.Draw(gameTime);
            foreach(var m in models){
                m.Draw(GraphicsDevice, Matrix.Identity, camera.View, camera.Projection);
            }

            // Handle base.Draw
            base.Draw(gameTime);
        }
    }
}
