// (c) Johannes Wolfgruber, 2020

using System;

namespace SystemAudioRecordingSoftware.Core.Model
{
    public sealed record TrackDto(Guid Id, Guid RecordingId, string Name, string FilePath, TimeSpan Start,
        TimeSpan Length);

    public sealed record Track(Guid Id, Guid RecordingId, string Name, string FilePath, TimeSpan Start,
        TimeSpan Length);
}