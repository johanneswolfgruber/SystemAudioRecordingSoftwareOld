// (c) Johannes Wolfgruber, 2020

using System;

namespace SystemAudioRecordingSoftware.Core.Model
{
    public sealed record TrackDto(Guid RecordingId, string Name, string FilePath, TimeSpan Length);
    
    public sealed class Track
    {
        public Track(Guid recordingId, string name, string filePath, TimeSpan length)
        {
            RecordingId = recordingId;
            Name = name;
            FilePath = filePath;
            Length = length;
        }

        public Guid RecordingId { get; }
        
        public string Name { get; }

        public string FilePath { get; }

        public TimeSpan Length { get; }
    }
}
