using System;
using System.Collections.Generic;

namespace SystemAudioRecordingSoftware.Domain.Model
{
    public sealed record RecordingDto(Guid Id, string Name, IReadOnlyList<TrackDto> Tracks, TimeSpan Length,
        string FilePath);

    public sealed record Recording(Guid Id, string Name, List<Track> Tracks, TimeSpan Length, string FilePath);
}