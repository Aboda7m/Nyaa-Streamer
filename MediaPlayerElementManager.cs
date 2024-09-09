using LibVLCSharp.Shared;
using System;

namespace Nyaa_Streamer
{
    public class MediaPlayerElementManager : IDisposable
    {
        public MediaPlayerElementManager(LibVLCSharp.Shared.MediaPlayer mediaPlayer)
        {
            MediaPlayer = mediaPlayer;
            // Initialize your sub-managers here, e.g.:
            // AspectRatioManager = new AspectRatioManager(mediaPlayer);
            // VolumeManager = new VolumeManager(mediaPlayer);
            // etc.
        }

        public LibVLCSharp.Shared.MediaPlayer MediaPlayer { get; private set; }

        public void Dispose()
        {
            // Dispose of sub-managers and other resources
            MediaPlayer?.Dispose();
        }
    }
}
