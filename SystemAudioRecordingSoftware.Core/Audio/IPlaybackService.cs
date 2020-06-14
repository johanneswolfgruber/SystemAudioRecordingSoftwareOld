// (c) Johannes Wolfgruber, 2020
using NAudio.Wave;
using System;

namespace SystemAudioRecordingSoftware.Core.Audio
{
    public interface IPlaybackService : IDisposable
    {
        bool IsPlaying { get; }
        IObservable<PlaybackState> PlaybackStateChanged { get; }
        IObservable<MinMaxValuesEventArgs> SampleAvailable { get; }

        void Initialize(string fileName);

        void Pause();

        void Play();

        void Stop();
    }
}
