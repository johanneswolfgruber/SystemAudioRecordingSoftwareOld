using System;
using SystemAudioRecordingSoftware.Application.Interfaces;
using SystemAudioRecordingSoftware.Domain.Extensions;
using SystemAudioRecordingSoftware.Domain.Model;
using SystemAudioRecordingSoftware.Domain.Types;

namespace SystemAudioRecordingSoftware.Application.Services
{
    public class SnippingService : ISnippingService
    {
        private readonly IRecordsRepository _recordsRepository;
        private readonly IRecordingService _recordingService;

        public SnippingService(IRecordsRepository recordsRepository, IRecordingService recordingService)
        {
            _recordsRepository = recordsRepository;
            _recordingService = recordingService;
        }

        public Result<TimeSpan> SnipCurrentRecording()
        {
            var timeStampResult = _recordingService.GetCurrentTimeStamp();
            if (timeStampResult.Failed)
            {
                return Result.Error<TimeSpan>(timeStampResult.ErrorText);
            }
            
            var idResult = _recordingService.GetCurrentRecording();
            if (idResult.Failed)
            {
                return Result.Error<TimeSpan>(idResult.ErrorText);
            }

            var snippingResult = SnipRecording(idResult.Value, timeStampResult.Value);
            if (snippingResult.Failed)
            {
                return Result.Error<TimeSpan>(snippingResult.ErrorText);
            }

            return Result.Success(timeStampResult.Value);
        }

        public Result SnipRecording(Guid recordingId, TimeSpan timeStamp)
        {
            var recordingResult = _recordsRepository.GetById(recordingId);
            if (recordingResult.Failed)
            {
                return Result.Error(recordingResult.ErrorText);
            }

            var recording = recordingResult.Value!;

            recording.Tracks.Add(
                new Track(Guid.NewGuid(), recording.Id, string.Empty, string.Empty, timeStamp, TimeSpan.Zero));

            var newRecording = recording with {Tracks = recording.UpdateTrackTimeStamps(timeStamp)};

            var updateResult = _recordsRepository.UpdateRecording(newRecording);
            if (updateResult.Failed)
            {
                return Result.Error(updateResult.ErrorText);
            }

            return Result.Success();
        }
    }
}