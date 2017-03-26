using Newtonsoft.Json;
using System;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Media.Ocr;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace TLDR_Client
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CameraPage : Page
    {
        MediaCapture mediaCapture;

        public CameraPage()
        {
            this.InitializeComponent();
            Loaded += CameraPage_Loaded;

            ResultGrid.Children.Clear();
        }

        private async void CameraPage_Loaded(object sender, RoutedEventArgs e)
        {
            mediaCapture = new MediaCapture();
            await mediaCapture.InitializeAsync();
            PreviewElement.Source = mediaCapture;
            await mediaCapture.StartPreviewAsync();
        }

        protected override async void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            if (mediaCapture.CameraStreamState == Windows.Media.Devices.CameraStreamState.Streaming)
            {
                await mediaCapture.StopPreviewAsync();
            }
        }

        private async void Button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var lowLagCapture = await mediaCapture.PrepareLowLagPhotoCaptureAsync(ImageEncodingProperties.CreateUncompressed(MediaPixelFormat.Bgra8));
            var frame = await lowLagCapture.CaptureAsync();
            await lowLagCapture.FinishAsync();
            await mediaCapture.StopPreviewAsync();

            ProgressBar.Visibility = Visibility.Visible;
            SummarizedText.Visibility = Visibility.Visible;
            SummarizedTextBlock.Visibility = Visibility.Visible;
            SummarizedText.IsEnabled = false;

            var ocrEngine = OcrEngine.TryCreateFromUserProfileLanguages();
            var ocrResult = await ocrEngine.RecognizeAsync(frame.Frame.SoftwareBitmap);

            var imageSource = new SoftwareBitmapSource();
            var bitmap = SoftwareBitmap.Convert(frame.Frame.SoftwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            await imageSource.SetBitmapAsync(bitmap);

            ResultImage.Source = imageSource;
            ResultImage.Visibility = Visibility.Visible;
            PreviewElement.Visibility = Visibility.Collapsed;
            
            var textAngle = -ocrResult.TextAngle.Value;

            foreach (var line in ocrResult.Lines)
            {
                foreach (var word in line.Words)
                {
                    var textBlock = new TextBlock();

                    var x = word.BoundingRect.X;
                    var y = word.BoundingRect.Y;
                    textBlock.Margin = new Thickness() { Left = x, Top = y};
                    ResultGrid.Children.Add(textBlock);
                }
            }

            ResultGrid.RenderTransform = new RotateTransform() { CenterX = ResultImage.Width / 2, CenterY = ResultImage.Height / 2, Angle = ocrResult.TextAngle.Value };


            ProgressBar.Visibility = Visibility.Visible;
            SummarizedText.IsEnabled = false;

            // Do the thing
            var client = new HttpClient();
            var requestObject = new RequestObject() { content = ocrResult.Text };
            var requestObjectString = JsonConvert.SerializeObject(requestObject);
            var messageContent = new HttpStringContent(requestObjectString);
            messageContent.Headers.ContentType = new Windows.Web.Http.Headers.HttpMediaTypeHeaderValue("application/json");
            var message = new HttpRequestMessage(HttpMethod.Post, new Uri("http://159.203.15.127:8000/summarizer/"))
            {
                Content = messageContent
            };

            var response = await client.SendRequestAsync(message);

            var responseString = await response.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<ResponseObject>(responseString);

            SummarizedText.Text = responseObject.summary;
            SummarizedText.IsEnabled = true;
            ProgressBar.Visibility = Visibility.Collapsed;
        }
    }
}
