//  ---------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// 
//  The MIT License (MIT)
// 
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
// 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
//  ---------------------------------------------------------------------------------

using System;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace VideoCaptureWithEditing.Views
{
    public sealed partial class PlaybackPage : Page
    {
        private bool isPlaying;

        public PlaybackPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var videoFile = e.Parameter as StorageFile;

            if (videoFile != null)
            {
                PlaybackMediaElement.Source = new Uri(videoFile.Path);
                HeaderTextBlock.Text = videoFile.DisplayName;
            }
        }

        private void PlayPauseButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (isPlaying)
                PlaybackMediaElement.Pause();
            else
                PlaybackMediaElement.Play();
        }

        private void PlaybackMediaElement_OnCurrentStateChanged(object sender, RoutedEventArgs e)
        {
            switch (PlaybackMediaElement.CurrentState)
            {
                case MediaElementState.Playing:
                    isPlaying = true;
                    PlayPauseButton.Label = "pause";
                    PlayPauseButton.Icon = new SymbolIcon(Symbol.Pause);
                    break;
                case MediaElementState.Paused:
                    isPlaying = false;
                    PlayPauseButton.Label = "play";
                    PlayPauseButton.Icon = new SymbolIcon(Symbol.Play);
                    break;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            PlaybackMediaElement.Stop();
            base.OnNavigatedFrom(e);
        }
    }
}
