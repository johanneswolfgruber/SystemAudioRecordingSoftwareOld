// (c) Johannes Wolfgruber, 2020
using System;

namespace SystemAudioRecordingSoftware.Core.Audio
{
    public interface IAudioEngineService
    {
        event EventHandler<CaptureStateChangedEventArgs>? CaptureStateChanged;

        event EventHandler<PlaybackStateChangedEventArgs>? PlaybackStateChanged;

        event EventHandler<MinMaxValuesEventArgs>? SampleAvailable;

        bool IsPlaying { get; }

        bool IsRecording { get; }

        void Pause();

        void Play();

        void Record();

        void Save(string filePath);

        void Stop();
    }
}
