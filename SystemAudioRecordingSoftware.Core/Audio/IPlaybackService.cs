// (c) Johannes Wolfgruber, 2020
using System;

namespace SystemAudioRecordingSoftware.Core.Audio
{
    public interface IPlaybackService : IDisposable
    {
        event EventHandler<PlaybackStateChangedEventArgs>? PlaybackStateChanged;

        event EventHandler<MinMaxValuesEventArgs>? SampleAvailable;

        bool IsPlaying { get; }

        void Initialize(string fileName);

        void Pause();

        void Play();

        void Stop();
    }
}
