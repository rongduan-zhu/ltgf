// Copyright (c) 2010-2013 SharpDX - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rightsD
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

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using SharpDX;
using Windows.UI.Xaml.Media.Animation;
using System.Diagnostics;

namespace Project2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>

    using Windows.UI.Xaml.Controls.Primitives;


    public sealed partial class MainPage
    {
        private Project2Game game;
        private float force  = 0;
        private int mode = 0;
        private int hitCount = 0;
        DispatcherTimer timer = new DispatcherTimer();
        public bool focussld { get; private set; }

        public MainPage()
        {
            
            InitializeComponent();
            focussld = false;
            game = new Project2Game(this);
            game.Run(this);
            timer.Start();

            //initialize positions
            startScreen.Width = getScreenWidth();
            startScreen.Height = getScreenHeight();
            bstart.Margin = new Thickness(startScreen.Width - 400, 180, 0, 0);
            bstart_practise.Margin = new Thickness(startScreen.Width - 400, 300, 0, 0);
            bcontrol.Margin = new Thickness(startScreen.Width - 400, 420, 0, 0);
            babout.Margin = new Thickness(startScreen.Width - 400, 540, 0, 0);
            sldforce.Margin = new Thickness(20, startScreen.Height - 100, 0, 0);
            btnhit.Margin = new Thickness(startScreen.Width - 280, startScreen.Height - 280, 0, 0);
            menuBar.Margin = new Thickness(startScreen.Width - 400, 0, 0, 0);
            displayText.Margin = new Thickness(10, 180, 0, 0);
            displayText.Visibility = Visibility.Collapsed;
        }
        private void startAnimation()
        {
            timer.Tick += timer_Tick;
        }

        void timer_Tick(object sender, object e)
        {
            throw new System.NotImplementedException();
        }

        public int getScreenWidth() {
            return (int)Window.Current.Bounds.Width;
        }

        public int getScreenHeight() {
            return (int)Window.Current.Bounds.Height;
        }

        public void startGame(object sender, RoutedEventArgs e)

        {
            btnMenu.Visibility = Windows.UI.Xaml.Visibility.Visible;
            game.gameState = Project2Game.GameState.Start;
            game.started = true;
            mode = 5;
            startScreen.Visibility = Visibility.Collapsed;
            sldforce.Visibility = Visibility.Visible;
            btnhit.Visibility = Visibility.Visible;
            popupBox.IsOpen = true;
            popupText.Text = "5 shots allowed";
            game.initializePositions();
        }

        private void practiseClick(object sender, RoutedEventArgs e)
        {
            btnMenu.Visibility = Windows.UI.Xaml.Visibility.Visible;
            game.gameState = Project2Game.GameState.Start;
            game.started = true;
            mode = 1000;
            startScreen.Visibility = Visibility.Collapsed;
            sldforce.Visibility = Visibility.Visible;
            btnhit.Visibility = Visibility.Visible;
            popupBox.IsOpen = true;
            popupText.Text = "Unlimited shots";
            game.initializePositions();
            //game.Run(this);
        }

        private void About(object sender, RoutedEventArgs e)
        {
            //abutton.Visibility = Visibility.Visible;
            string aboutText = "Production of:\n\tPo Chen, Litao Shen, Weiqian Wang, Rongduan Zhu.\n\tAll Rights Reserved";
            dummyText.Text = aboutText;
            if (displayText.Visibility == Visibility.Visible && displayText.Text.Equals(dummyText.Text))
            {
                displayText.Visibility = Visibility.Collapsed;
            }
            else
            {
                displayText.Text = aboutText;
                displayText.Visibility = Visibility.Visible;
            }
            
        }
        private void Control(object sender, RoutedEventArgs e)
        {
            //cbutton.Visibility = Visibility.Visible;
            string controlText = "Objective:\n\tHit the golf ball into the hole under the floating arrow.\n\t" +
                "Direction of camera is the direction the ball will be hit.\n\t" +
                "Use slider to control how hard you want to hit the ball,\n\tuse the Hit Button to hit.\n";
            dummyText.Text = controlText;
            if (displayText.Visibility == Visibility.Visible && displayText.Text.Equals(dummyText.Text))
            {
                displayText.Visibility = Visibility.Collapsed;
            }
            else
            {
                displayText.Text = controlText;
                Debug.WriteLine(displayText.Text.ToString().Length);
                Debug.WriteLine(controlText.Length);
                displayText.Visibility = Visibility.Visible;
            }
        }

        public void hideAbout(object sender, RoutedEventArgs e)
        {
            abutton.Visibility = Visibility.Collapsed;
            cbutton.Visibility = Visibility.Collapsed;
        }

        private void setForce(object sender, RangeBaseValueChangedEventArgs e)
        {
            force = (float)e.NewValue;
        }

        public void showHitUI()
        {
            sldforce.Visibility = Visibility.Visible;
            btnhit.Visibility = Visibility.Visible;
        }

        private void hit(object sender, RoutedEventArgs e)
        {
            if (game.gameState == Project2Game.GameState.Start)
            {
                game.fire();
            }
            // hit the ball + UI disappear + watch movie
            game.gameState = Project2Game.GameState.Movie;
            game.movement.Hit(force);

            sldforce.Visibility = Visibility.Collapsed;
            btnhit.Visibility = Visibility.Collapsed;
            mode--;
            hitCount++;
        }

        private void closePopupClick(object sender, RoutedEventArgs e)
        {
            popupBox.IsOpen = false;
            if (game.gameState == Project2Game.GameState.Win ||
                game.gameState == Project2Game.GameState.Lose)
            {
                game.started = false;
                focussld = false;
                startScreen.Visibility = Visibility.Visible;
                sldforce.Visibility = Visibility.Collapsed;
                btnhit.Visibility = Visibility.Collapsed;
            }
        }

        internal void win()
        {
            game.gameState = Project2Game.GameState.Win;
            popupBox.IsOpen = true;
            popupText.Text = "You Finished the game with " + hitCount + " shot(s)";
            //game.started = false;
        }

        internal void lose()
        {
            game.gameState = Project2Game.GameState.Lose;
            popupBox.IsOpen = true;
            popupText.Text = "GameOver";
            //game.started = false;
        }

        private void btnback_Click(object sender, RoutedEventArgs e)
        {
            btnMenu.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            focussld = false;
            startScreen.Visibility = Visibility.Visible;
            sldforce.Visibility = Visibility.Collapsed;
            btnhit.Visibility = Visibility.Collapsed;
            menuBar.IsOpen = false;
        }

        private void btnQuit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();   
        }

        private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch sound1 = sender as ToggleSwitch;
            if (sound1 != null){
           if (sound1.IsOn == true) {
               if (bgm != null)
               {
                   bgm.IsMuted = false;
               }
            }
           else
           {
               if (bgm != null)
               {
                   bgm.IsMuted = true;
               }
           }
            }
        }

        private void sldforce_PointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            focussld = false;
        }

        private void sldforce_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            focussld = true;
        }

        private void btnMenu_Click(object sender, RoutedEventArgs e)
        {
            menuBar.IsOpen = true;
        }

        private void btnHideMenu_Click(object sender, RoutedEventArgs e) {
            menuBar.IsOpen = false;
        }
    }
}
