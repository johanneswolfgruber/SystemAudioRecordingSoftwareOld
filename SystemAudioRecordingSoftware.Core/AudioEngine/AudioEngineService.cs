// (c) Johannes Wolfgruber, 2020
using NAudio.CoreAudioApi;
using NAudio.Wave;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using SystemAudioRecordingSoftware.Core.Audio;
using SystemAudioRecordingSoftware.Core.File;
using SystemAudioRecordingSoftware.Core.Model;

namespace SystemAudioRecordingSoftware.Core.AudioEngine
{
    public sealed class AudioEngineService : IAudioEngineService
    {
        private readonly IFilePathProvider _filePathProvider;
        private readonly IRecorderService _recorderService;
        private readonly Subject<MinMaxValuesEventArgs> _sampleAvailable;
        private readonly Subject<IReadOnlyList<Recording>> _recordingsChanged;
        private readonly Subject<PlaybackState> _playBackStateChanged;
        private readonly Dictionary<Guid, Recording> _recordings;
        private string _currentPlayBackFilePath;
        private IPlaybackService? _playbackService;

        public AudioEngineService(IFilePathProvider? filePathProvider = null,
                                  IRecorderService? recorderService = null)
        {
            _filePathProvider = filePathProvider ?? Locator.Current.GetService<IFilePathProvider>();
            _recorderService = recorderService ?? Locator.Current.GetService<IRecorderService>();

            _sampleAvailable = new Subject<MinMaxValuesEventArgs>();
            _recordingsChanged = new Subject<IReadOnlyList<Recording>>();
            _playBackStateChanged = new Subject<PlaybackState>();
            _recordings = new Dictionary<Guid, Recording>();
            _currentPlayBackFilePath = string.Empty;

            _recorderService
                .SampleAvailable
                .Subscribe(x => OnSampleAvailable(x.MinValue, x.MaxValue));

            _recorderService
                .RecordingStopped
                .Subscribe(x => OnRecordingStopped());

            _recorderService
                .NewRecordingCreated
                .Subscribe(x => OnNewRecordingCreated(x));
        }

        public bool IsPlaying => _playbackService != null && _playbackService.IsPlaying;

        public bool IsRecording => _recorderService.IsRecording;

        public IObservable<CaptureState> CaptureStateChanged => _recorderService.CaptureStateChanged;

        public IObservable<PlaybackState> PlaybackStateChanged => _playBackStateChanged.AsObservable();

        public IObservable<MinMaxValuesEventArgs> SampleAvailable => _sampleAvailable.AsObservable();

        public IObservable<IReadOnlyList<Recording>> RecordingsChanged => _recordingsChanged.AsObservable();

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
                    .SampleAvailable
                    .Subscribe(x => OnSampleAvailable(x.MinValue, x.MaxValue));
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

        private void OnNewRecordingCreated(Recording recording)
        {
            _recordings.Add(recording.Id, recording);
            _recordingsChanged.OnNext(_recordings
                .Select(x => x.Value)
                .ToList());
        }

        private void OnRecordingStopped()
        {
            //_playbackService.Initialize(_filePathProvider.CurrentRecordingFile);
        }

        private void OnSampleAvailable(float minValue, float maxValue)
        {
            _sampleAvailable.OnNext(new MinMaxValuesEventArgs(minValue, maxValue));
        }
    }
}
