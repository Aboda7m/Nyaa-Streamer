#if WINDOWS
#else
using LibVLCSharp.Shared;
using LibVLCSharp.MAUI;
using System.Diagnostics;

namespace Nyaa_Streamer
{
    public partial class LibVLCSharpPage : ContentPage
    {
        public LibVLCSharpPage()
        {
            InitializeComponent();
            Debug.WriteLine("public LibVLCSharpPage()\r\n        {\r\n            InitializeComponent();");
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ((MainViewModel)BindingContext).OnAppearing();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            ((MainViewModel)BindingContext).OnDisappearing();
        }

        private void VideoView_MediaPlayerChanged(object sender, MediaPlayerChangedEventArgs e)
        {
            ((MainViewModel)BindingContext).OnVideoViewInitialized();
        }
    }
}
#endif
