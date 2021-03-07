// (c) Johannes Wolfgruber, 2020

using NAudio.Wave;
using System;
using SystemAudioRecordingSoftware.Domain.Events;

namespace SystemAudioRecordingSoftware.Application.Interfaces
{
    public interface IPlaybackService : IDisposable
    {
        bool IsPlaying { get; }

        void PausePlayback();

        void Play(string filePath);
        event EventHandler<EventArgs> PlaybackStarted;

        event EventHandler<PlaybackStateChangedEventArgs> PlaybackStateChanged;

        event EventHandler<StoppedEventArgs> PlaybackStopped;

        void StopPlayback();
    }
}