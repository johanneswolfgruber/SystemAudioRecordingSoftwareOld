using AutoMapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SystemAudioRecordingSoftware.Application.Interfaces;
using SystemAudioRecordingSoftware.Domain.Events;
using SystemAudioRecordingSoftware.Domain.Model;
using SystemAudioRecordingSoftware.Domain.Types;

namespace SystemAudioRecordingSoftware.Infrastructure.Services
{
    internal class RecordsRepository : IRecordsRepository
    {
        private readonly IMapper _mapper;
        private readonly Dictionary<Guid, Recording> _recordings = new();

        public RecordsRepository(IMapper mapper)
        {
            _mapper = mapper;
        }

        public event EventHandler<RecordingChangedEventArgs>? RecordingChanged;
        public event EventHandler<RecordingsCollectionChangedEventArgs>? RecordingsCollectionChanged;

        public Recording CreateRecording()
        {
            var id = Guid.NewGuid();
            var dirPath = Path.Combine(Path.GetTempPath(), "SystemAudioRecordingSoftware");
            Directory.CreateDirectory(dirPath);
            var path = Path.Combine(dirPath, id + ".wav");
            var recording = new Recording(
                id,
                string.Empty,
                new List<Track> {new(Guid.NewGuid(), id, "Track 1", path, TimeSpan.Zero, TimeSpan.Zero)},
                TimeSpan.Zero,
                path);

            _recordings.Add(recording.Id, recording);

            RecordingsCollectionChanged?.Invoke(this,
                new RecordingsCollectionChangedEventArgs(
                    _recordings.Values.Select(x => _mapper.Map<RecordingDto>(x)).ToList()));

            return recording;
        }

        public Result<Recording> GetById(Guid id)
        {
            if (!_recordings.TryGetValue(id, out Recording? recording))
            {
                return Result.Error<Recording>($"Recording with Id {id} does not exist");
            }

            return Result.Success(recording);
        }

        public IReadOnlyList<Recording> GetAllRecordings()
        {
            return _recordings.Values.ToList();
        }

        public Result UpdateRecording(Recording r)
        {
            if (!_recordings.TryGetValue(r.Id, out Recording? recording))
            {
                return Result.Error($"Recording with Id {r.Id} does not exist");
            }

            _recordings[r.Id] = recording with
            {
                Name = r.Name,
                Tracks = r.Tracks.Select(t => _mapper.Map<Track>(t)).ToList(),
                Length = r.Length,
                FilePath = r.FilePath
            };

            RecordingChanged?.Invoke(this, new RecordingChangedEventArgs(_mapper.Map<RecordingDto>(_recordings[r.Id])));

            return Result.Success();
        }

        public Result RemoveRecording(Guid id)
        {
            if (!_recordings.ContainsKey(id))
            {
                return Result.Error<RecordingDto>($"Recording with Id {id} does not exist");
            }

            _recordings.Remove(id);

            RecordingsCollectionChanged?.Invoke(this,
                new RecordingsCollectionChangedEventArgs(
                    _recordings.Values.Select(x => _mapper.Map<RecordingDto>(x)).ToList()));

            return Result.Success();
        }

        public Result RemoveTrack(Guid recordingId, Guid trackId)
        {
            if (!_recordings.TryGetValue(recordingId, out Recording? recording))
            {
                return Result.Error<RecordingDto>($"Recording with Id {recordingId} does not exist");
            }

            recording.Tracks.RemoveAll(x => x.Id == trackId);

            RecordingChanged?.Invoke(this, new RecordingChangedEventArgs(_mapper.Map<RecordingDto>(recording)));

            return Result.Success();
        }
    }
}