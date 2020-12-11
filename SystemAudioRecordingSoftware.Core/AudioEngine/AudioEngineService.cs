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
        private string _currentPlayBackFilePath;

        public AudioEngineService(IRecorderService? recorderService = null, IPlaybackService? playbackService = null,
            IAudioFileLoaderService? audioFileLoaderService = null)
        {
            _recorderService = recorderService ?? Locator.Current.GetService<IRecorderService>();
            _playbackService = playbackService ?? Locator.Current.GetService<IPlaybackService>();
            _audioFileLoaderService = audioFileLoaderService ?? Locator.Current.GetService<IAudioFileLoaderService>();

            _audioDataAvailable = new Subject<AudioDataDto>();
            _recordings = new SourceCache<Recording, Guid>(r => r.Id);
            _currentPlayBackFilePath = string.Empty;

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

        public void StartRecording()
        {
            _recorderService.StartRecording();
        }

        public void StopRecording()
        {
            _recorderService.StopRecording();
        }

        public TimeSpan SnipRecording()
        {
            return _recorderService.SnipRecording();
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

        // public void Play(string filePath)
        // {
        //     if (_currentPlayBackFilePath != filePath || _playbackService == null)
        //     {
        //         _currentPlayBackFilePath = filePath;
        //         _playbackService = new PlaybackService(filePath);
        //         _playbackService
        //             .PlaybackDataAvailable
        //             .Subscribe(x => _audioDataAvailable.OnNext(x));
        //         _playbackService
        //             .PlaybackStateChanged
        //             .Subscribe(x => _playBackStateChanged.OnNext(x));
        //     }
        //
        //     _playbackService.Play();
        // }

        public IObservable<IChangeSet<RecordingDto, Guid>> RecordingsChanged() =>
            _recordings.Connect().Transform(r => new RecordingDto(
                r.Id,
                $"Recording {_recordings.Count}",
                r.Tracks.Select(t => new TrackDto(t.RecordingId, t.Name, t.FilePath, t.Length)).ToList(),
                r.FilePath,
                r.Length));

        public void RemoveRecording(Guid id)
        {
            _recordings.Remove(id);
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
            _recordings.AddOrUpdate(recording);
        }

        private void OnRecordingStopped()
        {
            //_playbackService.Initialize(_filePathProvider.CurrentRecordingFile);
        }
    }
}