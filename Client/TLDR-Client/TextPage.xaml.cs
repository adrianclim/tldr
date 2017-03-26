using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Web.Http;
using System;
using Newtonsoft.Json;
using Windows.Storage.Streams;
using System.Linq;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;
using Windows.UI;
using System.Collections.Generic;
using System.Diagnostics;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace TLDR_Client
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TextPage : Page
    {
        Dictionary<string, KeyPhrase> keyPhrases;
        public TextPage()
        {
            this.InitializeComponent();
        }

        private async void Button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                ProgressBar.Visibility = Visibility.Visible;

                var client = new HttpClient();
                var requestObject = new RequestObject() { content = OriginalText.Text };

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

                var keyPhraseIndices = phrases.Select(phrase => summaryString.IndexOf(phrase)).ToArray();

                var paragraph = new Paragraph();

                var normalStringStartIndex = 0;
                for (var i = 0; i < phrases.Length; ++i)
                {
                    if (normalStringStartIndex < keyPhraseIndices[i])
                    {
                        paragraph.Inlines.Add(new Run() { Text = summaryString.Substring(normalStringStartIndex, keyPhraseIndices[i] - normalStringStartIndex) });
                    }

                    var uiContainter = new InlineUIContainer();
                    var button = new Button() { Content = phrases[i], Background = new SolidColorBrush(Colors.Yellow) };
                    button.Margin = new Thickness(0, 0, 0, 0);
                    button.Padding = new Thickness(0, 0, 0, 0);

                    button.Click += KeyPhraseClick;
                    uiContainter.Child = button;
                    paragraph.Inlines.Add(uiContainter);

                    normalStringStartIndex = keyPhraseIndices[i] + phrases[i].Length;
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


        private void KeyPhraseClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var phrase = button.Content as string;
            var keyPhrase = keyPhrases[phrase];
            var dialog = new Flyout()
            {
                Content = new WebView() { Source = keyPhrase.url, MaxHeight=RootGrid.ActualHeight, MaxWidth=RootGrid.ActualWidth },
                Placement = Windows.UI.Xaml.Controls.Primitives.FlyoutPlacementMode.Full
            };
            button.Flyout = dialog;
            button.Flyout.ShowAt(button);
        }

    }
}
