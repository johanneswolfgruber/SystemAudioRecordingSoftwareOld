using System;
using System.Collections.Generic;
using System.Linq;
using SystemAudioRecordingSoftware.Application.Interfaces;
using SystemAudioRecordingSoftware.Domain.Model;
using SystemAudioRecordingSoftware.Domain.Types;

namespace SystemAudioRecordingSoftware.Application.Services
{
    public class SnippingService : ISnippingService
    {
        private readonly IRecordsRepository _recordsRepository;

        public SnippingService(IRecordsRepository recordsRepository)
        {
            _recordsRepository = recordsRepository;
        }

        public Result<TimeSpan> SnipCurrentRecording()
        {
            // TODO: implement
            return Result.Success(TimeSpan.FromSeconds(1.5));
        }

        public Result SnipRecording(Guid recordingId, TimeSpan timeStamp)
        {
            var recordingResult = _recordsRepository.GetById(recordingId);
            if (recordingResult.Failed)
            {
                return Result.Error(recordingResult.ErrorText);
            }

            var recording = recordingResult.Value;

            recording!.Tracks.Add(
                new Track(Guid.NewGuid(), recording.Id, string.Empty, string.Empty, timeStamp, TimeSpan.Zero));

            var nextStartTimeStamp = recording.Tracks.Count > 1
                ? recording.Tracks[1].Start
                : recording.Length;
            var tracks = recording.Tracks
                .OrderBy(t => t.Start)
                .ToList();

            var newTracks = new List<Track>();
            for (int i = 0; i < tracks.Count - 1; i++)
            {
                var length = nextStartTimeStamp - tracks[i].Start;
                nextStartTimeStamp = tracks[i + 1].Start;
                newTracks.Add(tracks[i] with {Name = $"Track {i + 1}", Length = length});
            }

            // var previousTimeStamp = TimeSpan.Zero;
            // var tracks = recording.Tracks
            //     .OrderBy(t => t.Start)
            //     .Select((t, i) =>
            //     {
            //         var length = t.Start - previousTimeStamp;
            //         previousTimeStamp = t.Start;
            //         return t with {Name = $"Track {i + 1}", Start = previousTimeStamp, Length = length};
            //     })
            //     .ToList();

            var newRecording = recording with {Tracks = newTracks};

            var updateResult = _recordsRepository.UpdateRecording(newRecording);
            if (updateResult.Failed)
            {
                return Result.Error(updateResult.ErrorText);
            }

            return Result.Success();
        }
    }
}