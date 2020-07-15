// (c) Johannes Wolfgruber, 2020
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using SystemAudioRecordingSoftware.Core.Audio;
using SystemAudioRecordingSoftware.Core.Model;

namespace SystemAudioRecordingSoftware.Core.AudioEngine
{
    public interface IAudioEngineService
    {
        bool IsPlaying { get; }
        bool IsRecording { get; }
        IObservable<CaptureState> CaptureStateChanged { get; }
        IObservable<PlaybackState> PlaybackStateChanged { get; }
        IObservable<MinMaxValuesEventArgs> SampleAvailable { get; }
        IObservable<IReadOnlyList<Recording>> RecordingsChanged { get; }

        void Pause();

        void Play(string filePath);

        void Record();

        void Save(string filePath);

        void Stop();
    }
}
