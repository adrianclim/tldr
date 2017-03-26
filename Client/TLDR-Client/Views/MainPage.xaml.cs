using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

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
            var result = new List<HamburgerMenuItem>();
            result.Add(new HamburgerMenuItem() { Icon = Symbol.Camera, Label = "OCR", Page = typeof(CameraPage) });

            return result;
        }
    }
}
