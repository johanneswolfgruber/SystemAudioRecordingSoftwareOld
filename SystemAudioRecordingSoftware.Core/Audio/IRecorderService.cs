// (c) Johannes Wolfgruber, 2020
using NAudio.Wave;
using System;
using System.Threading.Tasks;
using SystemAudioRecordingSoftware.Core.File;

namespace SystemAudioRecordingSoftware.Core.Audio
{
    public interface IRecorderService
    {
        event EventHandler? CaptureStateChanged;

        event EventHandler<StoppedEventArgs>? RecordingStopped;

        event EventHandler<MinMaxValuesEventArgs>? SampleAvailable;

        bool IsRecording { get; }

        void StartRecording();

        void StopRecording();
    }
}
