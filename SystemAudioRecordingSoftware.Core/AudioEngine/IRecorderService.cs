// (c) Johannes Wolfgruber, 2020

using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using SystemAudioRecordingSoftware.Core.Model;

namespace SystemAudioRecordingSoftware.Core.AudioEngine
{
    public interface IRecorderService
    {
        bool IsRecording { get; }
        IObservable<CaptureState> CaptureStateChanged { get; }
        IObservable<StoppedEventArgs> RecordingStopped { get; }
        IObservable<AudioDataDto> AudioDataAvailable { get; }
        IObservable<Recording> NewRecordingCreated { get; }

        void StartRecording();

        void StopRecording();
        
        void SnipRecording();
    }
}
