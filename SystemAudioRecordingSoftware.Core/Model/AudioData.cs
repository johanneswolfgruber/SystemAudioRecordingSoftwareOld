// (c) Johannes Wolfgruber, 2020

using System.Collections.Generic;

namespace SystemAudioRecordingSoftware.Core.Model
{
    public sealed record AudioDataDto(
        IEnumerable<float> Buffer,
        int TotalNumberOfSingleChannelSamples,
        int SampleRate);
    
    public sealed class AudioData
    {
        public AudioData(IEnumerable<float> buffer, int totalNumberOfSingleChannelSamples, int sampleRate)
        {
            Buffer = buffer;
            TotalNumberOfSingleChannelSamples = totalNumberOfSingleChannelSamples;
            SampleRate = sampleRate;
        }

        public IEnumerable<float> Buffer { get; }
        public int TotalNumberOfSingleChannelSamples { get; }
        public int SampleRate { get; }
    }
}