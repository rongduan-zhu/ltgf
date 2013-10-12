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
    public sealed partial class MainPage
    {
        private readonly Project2Game game;

        public MainPage()
        {
            InitializeComponent();
            game = new Project2Game();
            game.Run(this);
        }
        private void StartGame(object sender, RoutedEventArgs e)
        {
            //game.started = true;
            sgrid.Visibility = Visibility.Collapsed;
            sldforce.Visibility = Visibility.Visible;
            btnhit.Visibility = Visibility.Visible;
            //game.Run(this);
        }
        private void About(object sender, RoutedEventArgs e)
        {
            //game.started = true;
            //sgrid.Visibility = Visibility.Collapsed;
            //game.Run(this);
            abutton.Visibility = Visibility.Visible;
        }

        private void abutton_Click(object sender, RoutedEventArgs e)
        {
            abutton.Visibility = Visibility.Collapsed;
        }

        private void btnhit_Click(object sender, RoutedEventArgs e)
        {
            //ball.move = true;
        }

        private void changeDifficulty(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            //if (game != null) { game.force = (float)e.NewValue; }
        }
    }
}
