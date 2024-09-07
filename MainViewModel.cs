using LibVLCSharp;
using System;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Nyaa_Streamer
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public MainViewModel()
        {
            Initialize();
            PlayCommand = new Command(OnPlay);
            PauseCommand = new Command(OnPause);
            StopCommand = new Command(OnStop);
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

        public ICommand PlayCommand { get; }
        public ICommand PauseCommand { get; }
        public ICommand StopCommand { get; }

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
            using var media = new LibVLCSharp.Shared.Media(LibVLC, new Uri("http://localhost:8888/"));

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

        private void OnPlay()
        {
            MediaPlayer?.Play();
        }

        private void OnPause()
        {
            MediaPlayer?.Pause();
        }

        private void OnStop()
        {
            MediaPlayer?.Stop();
        }
    }
}
