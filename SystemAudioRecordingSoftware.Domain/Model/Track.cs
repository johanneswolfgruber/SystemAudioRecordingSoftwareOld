using System;

namespace SystemAudioRecordingSoftware.Domain.Model
{
    public sealed record TrackDto(Guid Id, Guid RecordingId, string Name, string FilePath, TimeSpan Start,
        TimeSpan Length);

    public sealed record Track(Guid Id, Guid RecordingId, string Name, string FilePath, TimeSpan Start,
        TimeSpan Length);
}