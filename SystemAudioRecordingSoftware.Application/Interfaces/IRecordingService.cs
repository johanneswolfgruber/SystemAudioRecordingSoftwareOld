// (c) Johannes Wolfgruber, 2020

using NAudio.Wave;
using System;
using SystemAudioRecordingSoftware.Domain.Events;

namespace SystemAudioRecordingSoftware.Application.Interfaces
{
    public interface IRecordingService
    {
        IDisplayDataProvider DisplayDataProvider { get; }

        bool IsRecording { get; }

        event EventHandler<CaptureStateChangedEventArgs> CaptureStateChanged;

        event EventHandler<WaveInEventArgs> DataAvailable;
        event EventHandler<EventArgs> RecordingStarted;

        event EventHandler<StoppedEventArgs> RecordingStopped;

        void StartRecording();

        void StopRecording();
    }
}