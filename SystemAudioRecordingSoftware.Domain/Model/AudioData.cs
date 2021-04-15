using System;
using System.Collections.Generic;

namespace SystemAudioRecordingSoftware.Domain.Model
{
    public sealed record AudioDataDto(IEnumerable<AudioDataPoint> Buffer);

    public sealed record AudioData(IEnumerable<AudioDataPoint> Buffer);
}