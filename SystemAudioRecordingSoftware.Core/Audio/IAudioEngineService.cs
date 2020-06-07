// (c) Johannes Wolfgruber, 2020
using System;
using System.Collections.Generic;
using System.Text;

namespace SystemAudioRecordingSoftware.Core.Audio
{
    public interface IAudioEngineService
    {
        event EventHandler? CaptureStateChanged;

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
