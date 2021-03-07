using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using SystemAudioRecordingSoftware.Application.Interfaces;
using SystemAudioRecordingSoftware.Domain.Events;
using SystemAudioRecordingSoftware.Domain.Model;
using SystemAudioRecordingSoftware.Domain.Types;
using SystemAudioRecordingSoftware.Infrastructure.Interfaces;

namespace SystemAudioRecordingSoftware.Infrastructure.Services
{
    public class RecordsService : IRecordsService
    {
        private readonly IMapper _mapper;
        private readonly IRecordsRepository _repository;

        public RecordsService(IRecordsRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public event EventHandler<RecordingChangedEventArgs>? RecordingChanged;
        public event EventHandler<RecordingsCollectionChangedEventArgs>? RecordingsCollectionChanged;

        public RecordingDto CreateRecording()
        {
            return _mapper.Map<RecordingDto>(_repository.CreateRecording());
        }

        public Result<RecordingDto> GetById(Guid id)
        {
            var result = _repository.GetById(id);
            return result.Succeeded
                ? Result.Success(_mapper.Map<RecordingDto>(result.Value))
                : Result.Error<RecordingDto>(result.ErrorText);
        }

        public IReadOnlyList<RecordingDto> GetAllRecordings()
        {
            return _repository.GetAllRecordings().Select(r => _mapper.Map<RecordingDto>(r)).ToList();
        }

        public Result UpdateRecording(RecordingDto recording)
        {
            return _repository.UpdateRecording(_mapper.Map<Recording>(recording));
        }

        public Result RemoveRecording(Guid id)
        {
            return _repository.RemoveRecording(id);
        }

        public Result RemoveTrack(Guid recordingId, Guid trackId)
        {
            return _repository.RemoveTrack(recordingId, trackId);
        }
    }
}