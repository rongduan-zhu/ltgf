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

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using SharpDX;

namespace Project2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>

    using Windows.UI.Xaml.Controls.Primitives;


    public sealed partial class MainPage
    {
        private  Project2Game game;
        private float force  = 0;
        public bool focussld = false;
        //which mode choosen, 0 is unlimit time,
        public int mode = 0;

        public MainPage()
        {
            InitializeComponent();
            game = new Project2Game(this);
            game.Run(this);
        }

        public void StartGame(object sender, RoutedEventArgs e)
        {
            mode = 1000;
            modebox.IsOpen = true;
            txb_mode.Text = "You are under Real play mode, in this mode, you hit will be counted as score!";
            sgrid.Visibility = Visibility.Collapsed;
            sldforce.Visibility = Visibility.Visible;
            btnhit.Visibility = Visibility.Visible;
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
            if (mode > 0)
            {
                // hit the ball + UI disappear + watch movie
                game.gameState = Project2Game.GameState.Movie;
                game.objectmove.InitializeV(force, game.camera.AngleV, game.camera.AngleH);

                sldforce.Visibility = Visibility.Collapsed;
                btnhit.Visibility = Visibility.Collapsed;
                mode--;
            }
            else
            {
                modebox.IsOpen = true;
                txb_mode.Text = "GameOver";
            }
        }

        private void sldforce_GotFocus(object sender, RoutedEventArgs e)
        {
            focussld = true;
        }


        private void sldforce_LostFocus(object sender, RoutedEventArgs e)
        {
            focussld = false;
        }

        private void bstart_practise_Click(object sender, RoutedEventArgs e)
        {
            mode = 3;
            sgrid.Visibility = Visibility.Collapsed;
            sldforce.Visibility = Visibility.Visible;
            btnhit.Visibility = Visibility.Visible;
            modebox.IsOpen = true;
            txb_mode.Text = "You are under practise mode, in this mode, you are allowed unlimited times to hit the ball!";
        }
        private void close_popup_Click(object sender, RoutedEventArgs e)
        {
            modebox.IsOpen = false;
            if (mode == 0)
            {
                game = new Project2Game(this);
                game.Run(this);
            }
        }
    }
}
