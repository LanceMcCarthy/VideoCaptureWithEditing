﻿//  ---------------------------------------------------------------------------------
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace VideoCaptureWithEditing.Views
{
    public sealed partial class MainPage : Page
    {
        private readonly ObservableCollection<StorageFile> videoFiles;
        private StorageFile selectedVideoFile;

        public MainPage()
        {
            InitializeComponent();

            // Set up ListView
            videoFiles = new ObservableCollection<StorageFile>();
            VideosListView.ItemsSource = videoFiles;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await CheckForVideosAsync();

            base.OnNavigatedTo(e);
        }

        private async Task CheckForVideosAsync()
        {
            try
            {
                // Checking for .mp4 files in LocalFolder
                var localFolderFiles = await ApplicationData.Current.LocalFolder.GetFilesAsync();

                // Create a list for any results
                videoFiles.Clear();

                // If there are any mp4 videos add them to the videoFiles list
                foreach (var file in localFolderFiles)
                {
                    if (file.FileType == ".mp4")
                    {
                        videoFiles.Add(file);
                    }
                }

                // If there were any videos, preselect the first one inthe ListView
                if (videoFiles.Count > 0)
                    VideosListView.SelectedIndex = 0;
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
            }
        }

        private void VideosListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                // If a video was selected enable the buttons
                if (e.AddedItems.Count > 0)
                {
                    var videoFile = e.AddedItems.FirstOrDefault() as StorageFile;

                    if (videoFile != null)
                    {
                        selectedVideoFile = videoFile;
                        PlayVideoButton.IsEnabled = true;
                        EditVideoButton.IsEnabled = true;
                        DeleteVideoButton.IsEnabled = true;
                        return;
                    }
                }

                // Disable the buttons if a video was not selected
                selectedVideoFile = null;
                PlayVideoButton.IsEnabled = false;
                EditVideoButton.IsEnabled = false;
                DeleteVideoButton.IsEnabled = false;
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
            }
        }

        private void RecordVideoButton_OnClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(RecordingPage));
        }

        private void EditVideoButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (selectedVideoFile == null)
                return;

            Frame.Navigate(typeof(EditingPage), selectedVideoFile);
        }

        private void PlayVideoButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (selectedVideoFile == null)
                return;

            Frame.Navigate(typeof(PlaybackPage), selectedVideoFile);
        }

        private async void DeleteVideoButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                await selectedVideoFile.DeleteAsync(StorageDeleteOption.PermanentDelete);

                selectedVideoFile = null;

                await CheckForVideosAsync();
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
            }
        }
    }
}
