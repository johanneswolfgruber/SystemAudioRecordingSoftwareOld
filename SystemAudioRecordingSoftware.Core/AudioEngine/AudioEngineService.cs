// (c) Johannes Wolfgruber, 2020

using DynamicData;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using Splat;
using System;
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
        private readonly IAudioFileLoaderService _audioFileLoaderService;
        private readonly IFilePathProvider _filePathProvider;
        private readonly Subject<PlaybackState> _playBackStateChanged;
        private readonly IRecorderService _recorderService;
        private readonly SourceCache<Recording, Guid> _recordings;
        private string _currentPlayBackFilePath;
        private IPlaybackService? _playbackService;

        public AudioEngineService(IFilePathProvider? filePathProvider = null,
            IRecorderService? recorderService = null,
            IAudioFileLoaderService? audioFileLoaderService = null)
        {
            _filePathProvider = filePathProvider ?? Locator.Current.GetService<IFilePathProvider>();
            _recorderService = recorderService ?? Locator.Current.GetService<IRecorderService>();
            _audioFileLoaderService = audioFileLoaderService ?? Locator.Current.GetService<IAudioFileLoaderService>();

            _audioDataAvailable = new Subject<AudioDataDto>();
            _playBackStateChanged = new Subject<PlaybackState>();
            _recordings = new SourceCache<Recording, Guid>(r => r.Id);
            _currentPlayBackFilePath = string.Empty;

            _recorderService
                .AudioDataAvailable
                .Subscribe(x => _audioDataAvailable.OnNext(x));

            _recorderService
                .RecordingStopped
                .Subscribe(_ => OnRecordingStopped());

            _recorderService
                .NewRecordingCreated
                .Subscribe(OnNewRecordingCreated);
        }

        public bool IsPlaying => _playbackService != null && _playbackService.IsPlaying;

        public bool IsRecording => _recorderService.IsRecording;

        public IObservable<CaptureState> CaptureStateChanged => _recorderService.CaptureStateChanged;

        public IObservable<PlaybackState> PlaybackStateChanged => _playBackStateChanged.AsObservable();

        public IObservable<AudioDataDto> AudioDataAvailable => _audioDataAvailable.AsObservable();

        public void Pause()
        {
            _playbackService?.Pause();
        }

        public void Play(string filePath)
        {
            if (_currentPlayBackFilePath != filePath || _playbackService == null)
            {
                _currentPlayBackFilePath = filePath;
                _playbackService = new PlaybackService(filePath);
                _playbackService
                    .AudioDataAvailable
                    .Subscribe(x => _audioDataAvailable.OnNext(x));
                _playbackService
                    .PlaybackStateChanged
                    .Subscribe(x => _playBackStateChanged.OnNext(x));
            }

            _playbackService.Play();
        }

        public void Record()
        {
            _recorderService.StartRecording();
        }

        public void SnipRecording()
        {
            _recorderService.SnipRecording();
        }

        public void Save(string filePath)
        {
            _filePathProvider.Save(filePath);
        }

        public void Stop()
        {
            if (IsRecording)
            {
                _recorderService.StopRecording();
            }
            else if (IsPlaying)
            {
                _playbackService?.Stop();
            }
        }

        public IObservable<IChangeSet<RecordingDto, Guid>> RecordingsChanged() => 
            _recordings.Connect().Transform(r => new RecordingDto(
                r.Id, 
                r.Name, 
                r.Tracks.Select(t => new TrackDto(t.RecordingId, t.Name, t.FilePath, t.Length)).ToList(), 
                r.FilePath, 
                r.Length));

        public void RemoveRecording(Guid id)
        {
            _recordings.Remove(id);
        }

        public AudioDataDto GetAudioDisplayData(string filePath)
        {
            var data = _audioFileLoaderService.GetAudioDisplayData(filePath);
            return new AudioDataDto(data.Buffer, data.TotalNumberOfSingleChannelSamples, data.SampleRate);
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