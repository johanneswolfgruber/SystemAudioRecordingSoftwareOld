// (c) Johannes Wolfgruber, 2020
using NAudio.Wave;
using System;

namespace SystemAudioRecordingSoftware.Core.Audio
{
    public interface IRecorderService
    {
        event EventHandler<CaptureStateChangedEventArgs>? CaptureStateChanged;

        event EventHandler<StoppedEventArgs>? RecordingStopped;

        event EventHandler<MinMaxValuesEventArgs>? SampleAvailable;

        bool IsRecording { get; }

        void StartRecording();

        void StopRecording();
    }
}
