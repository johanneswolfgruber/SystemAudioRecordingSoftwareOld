// (c) Johannes Wolfgruber, 2020

using NAudio.Wave;
using System;
using SystemAudioRecordingSoftware.Core.Model;

namespace SystemAudioRecordingSoftware.Core.AudioEngine
{
    public interface IPlaybackService : IDisposable
    {
        bool IsPlaying { get; }
        IObservable<PlaybackState> PlaybackStateChanged { get; }
        IObservable<AudioDataDto> AudioDataAvailable { get; }

        void Pause();

        void Play();

        void Stop();
    }
}
