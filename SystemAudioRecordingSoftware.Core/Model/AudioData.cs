// (c) Johannes Wolfgruber, 2020

using System;
using System.Collections.Generic;

namespace SystemAudioRecordingSoftware.Core.Model
{
    public sealed record AudioDataDto(IEnumerable<float> Buffer, TimeSpan TotalTime);

    public sealed record AudioData(IEnumerable<float> Buffer, TimeSpan TotalTime);
}