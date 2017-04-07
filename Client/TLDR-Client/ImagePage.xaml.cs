using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TLDR;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace TLDR_Client
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ImagePage : Page
    {
        private FindMainParagraph findMainParagraph;

        public ImagePage()
        {
            this.InitializeComponent();
        }

        private async void BrowseButtonClick(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker()
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".png");
            picker.ViewMode = PickerViewMode.Thumbnail;
            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                SoftwareBitmap originalBitmap;
                using (var stream = await file.OpenReadAsync())
                {
                    var decoder = await BitmapDecoder.CreateAsync(stream);
                    originalBitmap = await decoder.GetSoftwareBitmapAsync();
                }

                findMainParagraph = new FindMainParagraph(originalBitmap);
                var finalBitmap = findMainParagraph.FinalImage();
                var erodedBitmap = findMainParagraph.ErodedImage();
                var contoursBitmap = findMainParagraph.AllContoursImage();

                var originalSource = await GetBitmapSourceAsync(originalBitmap);
                Original.Source = originalSource;
                BrowseButton.Visibility = Visibility.Collapsed;

                var finalSource = await GetBitmapSourceAsync(finalBitmap);
                Final.Source = finalSource;

                var erodedSource = await GetBitmapSourceAsync(erodedBitmap);
                Eroded.Source = erodedSource;

                var contoursSource = await GetBitmapSourceAsync(contoursBitmap);
                Contours.Source = contoursSource;
            }
        }

        private async Task<SoftwareBitmapSource> GetBitmapSourceAsync(SoftwareBitmap bitmap)
        {
            var imageSource = new SoftwareBitmapSource();
            var newbitmap = SoftwareBitmap.Convert(bitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            await imageSource.SetBitmapAsync(newbitmap);
            return imageSource;
        }
    }
}
