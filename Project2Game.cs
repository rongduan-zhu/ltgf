﻿// Copyright (c) 2010-2013 SharpDX - Alexandre Mutel
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
using Windows.UI.Input;
using Windows.UI.Core;


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
        private Model model;
        private Stack<GameModel> models;
        public GameInput input { get; private set; }
        public Camera camera { get; private set; }
        public GameModel ball { get; private set; }
        public GameModel pin { get; private set; }
        public GameModel arrow { get; private set; }

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
            models = new Stack<GameModel>(3);

            // Creates a keyboard manager
            input = new GameInput();
            // Initialise event handling.
            input.gestureRecognizer.ManipulationUpdated += OnManipulationUpdated;
        }

        protected override void LoadContent()
        {
            landscape = new Landscape2(this);

            model = Content.Load<Model>("Arrow");
            arrow = new GameModel(model, this, 5, 10, -5);
            models.Push(arrow);
            model = Content.Load<Model>("Ball");
            ball = new GameModel(model, this, 5, 10, -10);
            models.Push(ball);
            model = Content.Load<Model>("Pin");
            pin = new GameModel(model, this, 5, 10, 0);
            models.Push(pin);

            foreach (var m in models)
            {
                BasicEffect.EnableDefaultLighting(m.model, true);
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
            camera.Update(gameTime);

            foreach (var m in models)
            {
                m.Update(gameTime);
            }

            landscape.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // Clears the screen with the Color.CornflowerBlue
            GraphicsDevice.Clear(Color.SkyBlue);

            foreach(var m in models){
                m.Draw();
            }

            landscape.Draw(gameTime);

            // Handle base.Draw
            base.Draw(gameTime);
        }

        public void OnManipulationUpdated(GestureRecognizer sender, ManipulationUpdatedEventArgs args)
        {
            camera.OnManipulationUpdated(sender, args);
        }
    }
}