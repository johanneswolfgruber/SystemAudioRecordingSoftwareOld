// (c) Johannes Wolfgruber, 2020

using DynamicData;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using SystemAudioRecordingSoftware.Core.File;
using SystemAudioRecordingSoftware.Core.Model;

namespace SystemAudioRecordingSoftware.Core.AudioEngine
{
    public sealed class AudioEngineService : IAudioEngineService
    {
        private readonly Subject<AudioDataDto> _audioDataAvailable;
        private readonly IDisposable _audioDataAvailableDisposable;
        private readonly IAudioFileLoaderService _audioFileLoaderService;
        private readonly IDisposable _newRecordingCreatedDisposable;
        private readonly IPlaybackService _playbackService;
        private readonly IRecorderService _recorderService;
        private readonly SourceCache<Recording, Guid> _recordings;
        private readonly IDisposable _recordingStoppedDisposable;
        private Recording? _currentRecording;

        public AudioEngineService(
            IRecorderService? recorderService = null,
            IPlaybackService? playbackService = null,
            IAudioFileLoaderService? audioFileLoaderService = null)
        {
            _recorderService = recorderService ?? Locator.Current.GetService<IRecorderService>();
            _playbackService = playbackService ?? Locator.Current.GetService<IPlaybackService>();
            _audioFileLoaderService = audioFileLoaderService ?? Locator.Current.GetService<IAudioFileLoaderService>();

            _audioDataAvailable = new Subject<AudioDataDto>();
            _recordings = new SourceCache<Recording, Guid>(r => r.Id);

            _audioDataAvailableDisposable = RecorderDataAvailable
                .Concat(PlaybackDataAvailable)
                .Subscribe(x => _audioDataAvailable.OnNext(x));

            _recordingStoppedDisposable = RecordingStopped
                .Subscribe(_ => OnRecordingStopped());

            _newRecordingCreatedDisposable = NewRecordingCreated
                .Subscribe(OnNewRecordingCreated);
        }

        public bool IsPlaying => _playbackService.IsPlaying;

        public bool IsRecording => _recorderService.IsRecording;

        public IObservable<CaptureState> CaptureStateChanged => _recorderService.CaptureStateChanged;

        public IObservable<PlaybackState> PlaybackStateChanged => _playbackService.PlaybackStateChanged;

        public IObservable<StoppedEventArgs> RecordingStopped => _recorderService.RecordingStopped;

        public IObservable<AudioDataDto> RecorderDataAvailable => _recorderService.RecorderDataAvailable;

        public IObservable<AudioDataDto> PlaybackDataAvailable => _playbackService.PlaybackDataAvailable;

        public IObservable<AudioDataDto> AudioDataAvailable => _audioDataAvailable.AsObservable();

        public IObservable<Recording> NewRecordingCreated => _recorderService.NewRecordingCreated;

        public Guid StartRecording()
        {
            return _recorderService.StartRecording();
        }

        public void StopRecording()
        {
            _recorderService.StopRecording();
        }

        public TimeSpan SnipRecording()
        {
            return _recorderService.SnipRecording();
        }

        public void SnipRecording(Guid? recordingId, TimeSpan timeStamp)
        {
            if (IsRecording)
            {
                _recorderService.SnipRecording(null, timeStamp);
                return;
            }

            if (recordingId == null)
            {
                throw new ArgumentException("RecordingId cannot be null");
            }

            var recordingResult = _recordings.Lookup(recordingId.Value);
            if (!recordingResult.HasValue)
            {
                throw new InvalidOperationException("Recording does not exist");
            }

            var recording = recordingResult.Value;
            recording.Tracks.Add(new Track(Guid.NewGuid(), recording.Id, string.Empty, string.Empty, timeStamp,
                TimeSpan.Zero));
            var previousTimeStamp = TimeSpan.Zero;
            var tracks = recording.Tracks
                .OrderBy(t => t.Start)
                .Select((t, i) =>
                {
                    var length = t.Start - previousTimeStamp;
                    previousTimeStamp = t.Start;
                    return t with {Name = $"Track {i + 1}", Start = previousTimeStamp, Length = length};
                })
                .ToList();

            var newRecording = recording with {Tracks = tracks};

            _recordings.Edit(x => x.AddOrUpdate(newRecording, recording.Id));
        }

        public void Initialize(string filePath)
        {
            _playbackService.Initialize(filePath);
        }

        public void PausePlayback()
        {
            _playbackService.PausePlayback();
        }

        public void Play()
        {
            _playbackService.Play();
        }

        public void StopPlayback()
        {
            _playbackService.StopPlayback();
        }

        public IObservable<IChangeSet<RecordingDto, Guid>> RecordingsChanged() =>
            _recordings.Connect().Transform(r => new RecordingDto(
                r.Id,
                $"Recording {_recordings.Count}",
                r.Tracks.Select(t => new TrackDto(t.Id, t.RecordingId, t.Name, t.FilePath, t.Start, t.Length)).ToList(),
                r.Length,
                r.FilePath));

        public void RemoveRecording(Guid id)
        {
            _recordings.Remove(id);
        }

        public void RemoveTrack(Guid recordingId, Guid trackId)
        {
            var recordingResult = _recordings.Lookup(recordingId);
            if (!recordingResult.HasValue)
            {
                throw new InvalidOperationException("Recording does not exist");
            }

            var recording = recordingResult.Value;
            var track = recording.Tracks.FirstOrDefault(t => t.Id == trackId);

            if (track == null)
            {
                throw new InvalidOperationException("Recording does not contain this track");
            }

            recording.Tracks.Remove(track);
            _recordings.Edit(x => x.AddOrUpdate(recording, recording.Id));
        }

        public IEnumerable<float> GetAudioData(string filePath)
        {
            throw new NotImplementedException();
        }

        public AudioDataDto GetAudioDisplayData(string filePath)
        {
            var data = _audioFileLoaderService.GetAudioDisplayData(filePath);
            return new AudioDataDto(data.Buffer, data.TotalTime);
        }

        public void Dispose()
        {
            _audioDataAvailableDisposable.Dispose();
            _recordingStoppedDisposable.Dispose();
            _newRecordingCreatedDisposable.Dispose();
            _playbackService.Dispose();
        }

        private void OnNewRecordingCreated(Recording recording)
        {
            _currentRecording = recording;
            _recordings.AddOrUpdate(recording);
            _playbackService.Initialize(_currentRecording.FilePath);
        }

        private void OnRecordingStopped()
        {
            if (_currentRecording == null)
            {
                return;
            }

            _playbackService.Initialize(_currentRecording.FilePath);
        }
    }
}