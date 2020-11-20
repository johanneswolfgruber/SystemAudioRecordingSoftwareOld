// (c) Johannes Wolfgruber, 2020

using System;
using System.Collections.Generic;

namespace SystemAudioRecordingSoftware.Core.Model
{
    public sealed record RecordingDto(Guid Id, string Name, IReadOnlyList<TrackDto> Tracks, string FilePath, TimeSpan Length);
    
    public sealed class Recording
    {
        public Recording(Guid id, string name, List<Track> tracks, string filePath, TimeSpan length)
        {
            Id = id;
            Name = name;
            Tracks = tracks;
            FilePath = filePath;
            Length = length;
        }

        public Guid Id { get; }
        
        public string Name { get; }
        
        public List<Track> Tracks { get; }

        public string FilePath { get; }

        public TimeSpan Length { get; }
    }
}
