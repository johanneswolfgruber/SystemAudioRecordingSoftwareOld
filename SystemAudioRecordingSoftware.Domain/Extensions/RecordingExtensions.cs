using System;
using System.Collections.Generic;
using System.Linq;
using SystemAudioRecordingSoftware.Domain.Model;

namespace SystemAudioRecordingSoftware.Domain.Extensions
{
    public static class RecordingExtensions
    {
        public static List<Track> UpdateTrackTimeStamps(this Recording recording, TimeSpan recordingLength)
        {
            var tracks = recording.Tracks
                .OrderBy(t => t.Start)
                .ToList();

            var newTracks = new List<Track>();
            for (int i = 0; i < tracks.Count - 1; i++)
            {
                var nextStartTimeStamp = tracks[i + 1].Start;
                var length = nextStartTimeStamp - tracks[i].Start;
                newTracks.Add(tracks[i] with
                {
                    Name = string.IsNullOrWhiteSpace(tracks[i].Name) ? $"Track {i + 1}" : tracks[i].Name, 
                    Length = length
                });
            }

            var lastTrackLength = tracks.Count > 1 ? recordingLength - tracks[^1].Start : recordingLength;
            newTracks.Add(tracks[^1] with
            {
                Name = string.IsNullOrWhiteSpace(tracks[^1].Name) ? $"Track {tracks.Count}" : tracks[^1].Name,  
                Length = lastTrackLength
            });
            
            return newTracks;
        }
    }
}