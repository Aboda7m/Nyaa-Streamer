using LibVLCSharp;
using System.ComponentModel;

namespace Nyaa_Streamer
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public MainViewModel()
        {
            Initialize();
        }

        private LibVLCSharp.Shared.LibVLC LibVLC { get; set; }

        private LibVLCSharp.Shared.MediaPlayer _mediaPlayer;
        public LibVLCSharp.Shared.MediaPlayer MediaPlayer
        {
            get => _mediaPlayer;
            private set => Set(nameof(MediaPlayer), ref _mediaPlayer, value);
        }

        private bool IsLoaded { get; set; }
        private bool IsVideoViewInitialized { get; set; }

        private void Set<T>(string propertyName, ref T field, T value)
        {
            if (field == null && value != null || field != null && !field.Equals(value))
            {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void Initialize()
        {
            LibVLC = new LibVLCSharp.Shared.LibVLC(enableDebugLogs: true);
            using var media = new LibVLCSharp.Shared.Media(LibVLC, new Uri("https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4"));

            MediaPlayer = new LibVLCSharp.Shared.MediaPlayer(LibVLC)
            {
                Media = media
            };
        }

        public void OnAppearing()
        {
            IsLoaded = true;
            Play();
        }

        internal void OnDisappearing()
        {
            MediaPlayer.Dispose();
            LibVLC.Dispose();
        }

        public void OnVideoViewInitialized()
        {
            IsVideoViewInitialized = true;
            Play();
        }

        private void Play()
        {
            if (IsLoaded && IsVideoViewInitialized)
            {
                MediaPlayer.Play();
            }
        }
    }
}
