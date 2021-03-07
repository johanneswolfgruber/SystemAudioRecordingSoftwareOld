using System;
using System.Collections.Generic;

namespace SystemAudioRecordingSoftware.Domain.Model
{
    public sealed record AudioDataDto(IEnumerable<float> Buffer, TimeSpan TotalTime);

    public sealed record AudioData(IEnumerable<float> Buffer, TimeSpan TotalTime);
}