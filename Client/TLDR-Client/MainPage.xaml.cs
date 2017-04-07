using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TLDR_Client
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            HamburgerMenu.ItemsSource = HamburgerMenuItem.GetMenuItems();
        }

        private void HamburgerMenu_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as HamburgerMenuItem;
            PageFrame.Navigate(item.Page);
        }
    }

    public class HamburgerMenuItem
    {
        public Symbol Icon { get; set; }
        public string Label { get; set; }
        public Type Page { get; set; }

        static public List<HamburgerMenuItem> GetMenuItems()
        {
            var result = new List<HamburgerMenuItem>
            {
                new HamburgerMenuItem() { Icon = Symbol.Camera, Label = "OCR", Page = typeof(CameraPage) },
                new HamburgerMenuItem() { Icon = Symbol.Page, Label = "Text", Page = typeof(TextPage) },
                new HamburgerMenuItem() { Icon = Symbol.BrowsePhotos, Label = "Photo", Page = typeof(ImagePage) }
            };
            return result;
        }
    }
}
