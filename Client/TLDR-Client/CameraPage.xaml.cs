﻿using System;
using Windows.Media.Capture;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

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
            await mediaCapture.StopPreviewAsync();
        }
    }
}