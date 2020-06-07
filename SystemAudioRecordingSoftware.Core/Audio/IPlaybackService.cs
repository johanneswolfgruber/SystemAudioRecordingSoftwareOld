// (c) Johannes Wolfgruber, 2020
using System;
using System.Collections.Generic;
using System.Text;

namespace SystemAudioRecordingSoftware.Core.Audio
{
    public interface IPlaybackService : IDisposable
    {
        event EventHandler<MinMaxValuesEventArgs>? SampleAvailable;

        bool IsPlaying { get; }

        void Initialize(string fileName);

        void Pause();

        void Play();

        void Stop();
    }
}
