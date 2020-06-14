// (c) Johannes Wolfgruber, 2020
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;

namespace SystemAudioRecordingSoftware.Core.Audio
{
    public interface IAudioEngineService
    {
        bool IsPlaying { get; }
        bool IsRecording { get; }
        IObservable<CaptureState> CaptureStateChanged { get; }
        IObservable<PlaybackState> PlaybackStateChanged { get; }
        IObservable<MinMaxValuesEventArgs> SampleAvailable { get; }

        void Pause();

        void Play();

        void Record();

        void Save(string filePath);

        void Stop();
    }
}
