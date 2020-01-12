using System;
using System.Diagnostics;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace VideoCaptureWithEditing.Controls
{
    public sealed partial class VidItem : UserControl
    {
        public VidItem()
        {
            this.InitializeComponent();
        }

        private async void VidItem_OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (this.DataContext is StorageFile file)
            {
                try
                {
                    var thumb = await file.GetThumbnailAsync(ThumbnailMode.VideosView, (uint)50);

                    var image = new BitmapImage();
                    image.SetSource(thumb);

                    ThumbImage.Source = image;

                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"GetThumbnailAsync Exception: {ex.Message}");
                }
            }
        }
    }
}
