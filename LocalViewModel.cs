using LibVLCSharp;
using System;
using System.ComponentModel;
using System.Diagnostics;

namespace Nyaa_Streamer
{
    public class LocalViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Uri _mediaUri;
        public Uri MediaUri
        {
            get => _mediaUri;
            set => Set(nameof(MediaUri), ref _mediaUri, value);
        }

        public LocalViewModel()
        {
            // Parameterless constructor
        }

        public LocalViewModel(Uri mediaUri) : this() // Calls the parameterless constructor
        {
            MediaUri = mediaUri;
            Initialize(mediaUri);
            Debug.WriteLine($"LocalViewModel initialized with URI: {mediaUri}");
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

        private void Initialize(Uri mediaUri)
        {
            LibVLC = new LibVLCSharp.Shared.LibVLC(enableDebugLogs: true);
            using var media = new LibVLCSharp.Shared.Media(LibVLC, mediaUri);

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
