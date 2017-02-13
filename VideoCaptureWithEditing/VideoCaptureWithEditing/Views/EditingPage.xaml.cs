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

// Code reused from https://github.com/Microsoft/Windows-universal-samples/tree/master/Samples/MediaEditing

using System;
using Windows.Foundation;
using Windows.Media.Core;
using Windows.Media.Editing;
using Windows.Media.Transcoding;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace VideoCaptureWithEditing.Views
{
    public sealed partial class EditingPage : Page
    {
        private MediaComposition composition;
        private MediaStreamSource mediaStreamSource;

        public EditingPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            var videoFile = e.Parameter as StorageFile;

            if (videoFile != null)
            {
                StatusTextBlock.Text = videoFile.DisplayName;

                // Create a MediaClip from the file
                var clip = await MediaClip.CreateFromFileAsync(videoFile);

                // Set the End Trim slider's maximum value so that the user can trim from the end
                // You can also do this from the start
                EndTrimSlider.Maximum = clip.OriginalDuration.Milliseconds;

                // Create a MediaComposition containing the clip and set it on the MediaElement.
                composition = new MediaComposition();
                composition.Clips.Add(clip);

                // start the MediaElement at the beginning
                EditorMediaElement.Position = TimeSpan.Zero;

                // Create the media source and assign it to the media player
                mediaStreamSource = composition.GeneratePreviewMediaStreamSource((int)EditorMediaElement.ActualWidth, (int)EditorMediaElement.ActualHeight);
                EditorMediaElement.SetMediaStreamSource(mediaStreamSource);

                TrimClipButton.IsEnabled = true;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            EditorMediaElement.Source = null;
            mediaStreamSource = null;
            base.OnNavigatedFrom(e);
        }
        

        private void TrimClip_Click(object sender, RoutedEventArgs e)
        {
            // Get the first clip in the MediaComposition
            // We know this beforehand because it's the only clip in the composition
            // that we created from the passed video file
            var clip = composition.Clips[0];

            // Trim the end of the clip (you can use TrimTimeFromStart to trim from the beginning)
            clip.TrimTimeFromEnd = TimeSpan.FromMilliseconds((long)EndTrimSlider.Value);

            // Rewind the MediaElement
            EditorMediaElement.Position = TimeSpan.Zero;

            // Update the video source with the trimmed clip
            mediaStreamSource = composition.GeneratePreviewMediaStreamSource((int)EditorMediaElement.ActualWidth, (int)EditorMediaElement.ActualHeight);
            EditorMediaElement.SetMediaStreamSource(mediaStreamSource);

            // Update the UI
            EndTrimSlider.Value = 0;
            StatusTextBlock.Text = "Clip trimmed! Trim again or click Save.";
            StatusTextBlock.Foreground = new SolidColorBrush(Colors.LawnGreen);
            SaveButton.IsEnabled = true;
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            EnableButtons(false);
            StatusTextBlock.Text = "Creating new file...";

            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync($"Edited Video {DateTime.Now:D}.mp4", CreationCollisionOption.ReplaceExisting);
            
            if (file != null)
            {
                var saveOperation = composition.RenderToFileAsync(file, MediaTrimmingPreference.Precise);

                // This will show progress as video is rendered and saved
                saveOperation.Progress = async (info, progress) =>
                {
                    await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        StatusTextBlock.Text = string.Format("Saving file... Progress: {0:F0}%", progress);
                    });
                };

                // when the operation is complete
                saveOperation.Completed = async (info, status) =>
                {
                    await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        try
                        {
                            var results = info.GetResults();

                            if (results != TranscodeFailureReason.None || status != AsyncStatus.Completed)
                            {
                                StatusTextBlock.Foreground = new SolidColorBrush(Colors.Red);
                                StatusTextBlock.Text = "Saving was unsuccessful";
                            }
                            else
                            {
                                // Successful save, go back to main page.
                                if (Frame.CanGoBack)
                                    Frame.GoBack();
                            }
                        }
                        finally
                        {
                            // Remember to re-enable controls on both success and failure
                            EnableButtons(true);
                        }
                    });
                };
            }
            else
            {
                EnableButtons(true);
            }
        }

        private void EnableButtons(bool isEnabled)
        {
            SaveButton.IsEnabled = isEnabled;
            TrimClipButton.IsEnabled = isEnabled;
        }
        
    }
}
