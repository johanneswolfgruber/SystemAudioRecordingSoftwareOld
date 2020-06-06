// (c) Johannes Wolfgruber, 2020
using NAudio.Wave;
using System;

namespace SystemAudioRecordingSoftware.Core.Audio
{
    internal interface IWaveInSampleProvider
    {
        event EventHandler<MinMaxValuesEventArgs>? SampleAvailable;

        WaveFormat WaveFormat { get; }

        void Add(byte[] buffer, int offset, int count);
    }
}
