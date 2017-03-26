using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Web.Http;
using System;

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
            var messageContent = new HttpStringContent(OriginalText.Text);
            var message = new HttpRequestMessage(HttpMethod.Post, new System.Uri("127.0.0.1"))
            {
                Content = messageContent
            };
            await client.SendRequestAsync(message);

            SummarizedText.IsEnabled = true;
            ProgressBar.Visibility = Visibility.Collapsed;
        }
    }
}
