using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Media.Ocr;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
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
        Dictionary<string, KeyPhrase> keyPhrases;

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
            try
            {

                CapturedPhoto frame = await CapturePreviewFrame();

                await mediaCapture.StopPreviewAsync();

                EnableResultPane();

                var ocrEngine = OcrEngine.TryCreateFromUserProfileLanguages();
                var ocrResult = await ocrEngine.RecognizeAsync(frame.Frame.SoftwareBitmap);

                await ShowResultImage(frame);
                var client = new HttpClient();
                var requestObject = new RequestObject() { content = ocrResult.Text };

                var requestObjectString = JsonConvert.SerializeObject(requestObject);

                var dataWriter = new DataWriter();
                dataWriter.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                dataWriter.WriteString(requestObjectString);
                var buffer = dataWriter.DetachBuffer();

                var messageContent = new HttpBufferContent(buffer);

                messageContent.Headers.ContentType = new Windows.Web.Http.Headers.HttpMediaTypeHeaderValue("application/json");
                var message = new HttpRequestMessage(HttpMethod.Post, ConfigConstants.EndPoint)
                {
                    Content = messageContent
                };

                var response = await client.SendRequestAsync(message);
                var responseBuffer = await response.Content.ReadAsBufferAsync();
                var dataReader = DataReader.FromBuffer(responseBuffer);
                dataReader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                var responseString = dataReader.ReadString(responseBuffer.Length);
                var responseObject = JsonConvert.DeserializeObject<ResponseObject>(responseString);

                var summaryString = responseObject.summary;
                var phrases = responseObject.key_phrases.Select(phrase => phrase.phrase).ToArray();
                var stringParts = summaryString.Split(phrases, StringSplitOptions.RemoveEmptyEntries);
                keyPhrases = responseObject.key_phrases.ToDictionary(phrase => phrase.phrase);

                var keyPhraseIndices = phrases.Select(phrase => new Tuple<int, String>(summaryString.IndexOf(phrase), phrase)).OrderBy(pair => pair.Item1).ToArray();

                var paragraph = new Paragraph();

                var normalStringStartIndex = 0;
                foreach (var pair in keyPhraseIndices)
                {
                    if (normalStringStartIndex < pair.Item1)
                    {
                        paragraph.Inlines.Add(new Run() { Text = summaryString.Substring(normalStringStartIndex, pair.Item1 - normalStringStartIndex) });
                    }

                    var uiContainter = new InlineUIContainer();
                    var button = new Button() { Content = pair.Item2, Background = new SolidColorBrush(Colors.Yellow) };
                    button.Margin = new Thickness(0, 0, 0, 0);
                    button.Padding = new Thickness(0, 0, 0, 0);

                    button.Click += KeyPhraseClick;
                    uiContainter.Child = button;
                    paragraph.Inlines.Add(uiContainter);

                    normalStringStartIndex = pair.Item1 + pair.Item2.Length;
                }


                if (normalStringStartIndex < summaryString.Length - 1)
                {
                    paragraph.Inlines.Add(new Run() { Text = summaryString.Substring(normalStringStartIndex) });
                }

                SummarizedText.Blocks.Add(paragraph);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                ProgressBar.Visibility = Visibility.Collapsed;
            }

        }

        private async void KeyPhraseClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var phrase = button.Content as string;
            var keyPhrase = keyPhrases[phrase];
            var dialog = new Flyout()
            {
                Content = new WebView() { Source = keyPhrase.url, MaxHeight = RootGrid.ActualHeight, MaxWidth = RootGrid.ActualWidth },
                Placement = Windows.UI.Xaml.Controls.Primitives.FlyoutPlacementMode.Full
            };
            button.Flyout = dialog;
            button.Flyout.ShowAt(button);
        }

        private async System.Threading.Tasks.Task ShowResultImage(CapturedPhoto frame)
        {
            SoftwareBitmapSource imageSource = await GetImageSourceFromCapturedFrame(frame);

            ResultImage.Source = imageSource;
            ResultImage.Visibility = Visibility.Visible;
            PreviewElement.Visibility = Visibility.Collapsed;
        }

        private static async System.Threading.Tasks.Task<SoftwareBitmapSource> GetImageSourceFromCapturedFrame(CapturedPhoto frame)
        {
            var imageSource = new SoftwareBitmapSource();
            var bitmap = SoftwareBitmap.Convert(frame.Frame.SoftwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            await imageSource.SetBitmapAsync(bitmap);
            return imageSource;
        }

        private async System.Threading.Tasks.Task<CapturedPhoto> CapturePreviewFrame()
        {
            var lowLagCapture = await mediaCapture.PrepareLowLagPhotoCaptureAsync(ImageEncodingProperties.CreateUncompressed(MediaPixelFormat.Bgra8));
            var frame = await lowLagCapture.CaptureAsync();
            await lowLagCapture.FinishAsync();
            return frame;
        }

        private void EnableResultPane()
        {
            ProgressBar.Visibility = Visibility.Visible;
            SummarizedText.Visibility = Visibility.Visible;
            SummarizedTextBlock.Visibility = Visibility.Visible;
        }
    }
}
