using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Web.Http;
using System;
using Newtonsoft.Json;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace TLDR_Client
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TextPage : Page
    {
        public TextPage()
        {
            this.InitializeComponent();
        }

        private async void Button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ProgressBar.Visibility = Visibility.Visible;
            SummarizedText.IsEnabled = false;

            // Do the thing
            var client = new HttpClient();
            var requestObject = new RequestObject() { content = OriginalText.Text };
            var requestObjectString = JsonConvert.SerializeObject(requestObject);
            var messageContent = new HttpStringContent(requestObjectString);
            var message = new HttpRequestMessage(HttpMethod.Post, new System.Uri("127.0.0.1"))
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
