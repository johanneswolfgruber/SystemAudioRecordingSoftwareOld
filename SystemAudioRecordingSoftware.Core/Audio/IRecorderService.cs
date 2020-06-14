// (c) Johannes Wolfgruber, 2020
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;

namespace SystemAudioRecordingSoftware.Core.Audio
{
    public interface IRecorderService
    {
        bool IsRecording { get; }
        IObservable<CaptureState> CaptureStateChanged { get; }
        IObservable<StoppedEventArgs> RecordingStopped { get; }
        IObservable<MinMaxValuesEventArgs> SampleAvailable { get; }

        void StartRecording();

        void StopRecording();
    }
}
