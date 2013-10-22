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
        
        public bool focussld { get; private set; }

        public MainPage()
        {
            InitializeComponent();
            focussld = false;
            game = new Project2Game(this);
            game.Run(this);
        }

        public void startGame(object sender, RoutedEventArgs e)

        {

            game.gameState = Project2Game.GameState.Start;
            game.started = true;
            mode = 5;
            startScreen.Visibility = Visibility.Collapsed;
            sldforce.Visibility = Visibility.Visible;
            btnhit.Visibility = Visibility.Visible;
            popupBox.IsOpen = true;
            popupText.Text = "5 shots Allowed";
        }

        private void practiseClick(object sender, RoutedEventArgs e)
        {
            game.gameState = Project2Game.GameState.Start;
            game.started = true;
            mode = 1000;
            startScreen.Visibility = Visibility.Collapsed;
            sldforce.Visibility = Visibility.Visible;
            btnhit.Visibility = Visibility.Visible;
            popupBox.IsOpen = true;
            popupText.Text = "Unlimited shots";
            //game.Run(this);
        }

        private void About(object sender, RoutedEventArgs e)
        {
            abutton.Visibility = Visibility.Visible;
        }

        public void hideAbout(object sender, RoutedEventArgs e)
        {
            abutton.Visibility = Visibility.Collapsed;
        }

        private void setForce(object sender, RangeBaseValueChangedEventArgs e)
        {
            force = (float) e.NewValue / 100;
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
            game.objectmove.InitializeV(force, game.camera.AngleV, game.camera.AngleH);

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
                
                game.Exit();
            }
        }

        internal void win()
        {
            game.gameState = Project2Game.GameState.Win;
            popupBox.IsOpen = true;
            popupText.Text = "You Finished the game with " + hitCount + " shot(s)";
        }

        internal void lose()
        {
            game.gameState = Project2Game.GameState.Lose;
            popupBox.IsOpen = true;
            popupText.Text = "GameOver";
            game.started = false;
        }

        private void btnback_Click(object sender, RoutedEventArgs e)
        {
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
    }
}
