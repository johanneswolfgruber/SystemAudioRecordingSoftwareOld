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
        IObservable<AudioDataDto> RecorderDataAvailable { get; }
        IObservable<Recording> NewRecordingCreated { get; }

        TimeSpan SnipRecording();

        void SnipRecording(Guid? recordingId, TimeSpan timeStamp);

        Guid StartRecording();

        void StopRecording();
    }
}