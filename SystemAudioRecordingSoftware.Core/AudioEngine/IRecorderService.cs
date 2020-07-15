// (c) Johannes Wolfgruber, 2020
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using SystemAudioRecordingSoftware.Core.Audio;
using SystemAudioRecordingSoftware.Core.Model;

namespace SystemAudioRecordingSoftware.Core.AudioEngine
{
    public interface IRecorderService
    {
        bool IsRecording { get; }
        IObservable<CaptureState> CaptureStateChanged { get; }
        IObservable<StoppedEventArgs> RecordingStopped { get; }
        IObservable<MinMaxValuesEventArgs> SampleAvailable { get; }
        IObservable<Recording> NewRecordingCreated { get; }

        void StartRecording();

        void StopRecording();
    }
}
