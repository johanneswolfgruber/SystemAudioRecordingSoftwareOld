// (c) Johannes Wolfgruber, 2020

using System;
using System.Collections.Generic;

namespace SystemAudioRecordingSoftware.Core.Model
{
    public sealed record AudioDataDto(
        IEnumerable<float> Buffer,
        TimeSpan TotalTime);

    public sealed class AudioData
    {
        public AudioData(IEnumerable<float> buffer, TimeSpan totalTime)
        {
            Buffer = buffer;
            TotalTime = totalTime;
        }

        public IEnumerable<float> Buffer { get; }
        public TimeSpan TotalTime { get; }
    }
}