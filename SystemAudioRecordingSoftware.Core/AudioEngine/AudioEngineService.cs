// (c) Johannes Wolfgruber, 2020
using NAudio.CoreAudioApi;
using NAudio.Wave;
using Splat;
using System;
using System.Collections.Generic;
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
        private readonly IPlaybackService _playbackService;
        private readonly IRecorderService _recorderService;
        private readonly Subject<MinMaxValuesEventArgs> _sampleAvailable;
        private readonly Subject<IReadOnlyList<Recording>> _recordingsChanged;

        public AudioEngineService(IFilePathProvider? filePathProvider = null,
                                  IRecorderService? recorderService = null,
                                  IPlaybackService? playbackService = null)
        {
            _filePathProvider = filePathProvider ?? Locator.Current.GetService<IFilePathProvider>();
            _recorderService = recorderService ?? Locator.Current.GetService<IRecorderService>();
            _playbackService = playbackService ?? Locator.Current.GetService<IPlaybackService>();

            _sampleAvailable = new Subject<MinMaxValuesEventArgs>();
            _recordingsChanged = new Subject<IReadOnlyList<Recording>>();

            _recorderService
                .SampleAvailable
                .Subscribe(x => OnSampleAvailable(x.MinValue, x.MaxValue));

            _recorderService
                .RecordingStopped
                .Subscribe(x => OnRecordingStopped());

            _playbackService
                .SampleAvailable
                .Subscribe(x => OnSampleAvailable(x.MinValue, x.MaxValue));
        }

        public bool IsPlaying => _playbackService.IsPlaying;
        public bool IsRecording => _recorderService.IsRecording;
        public IObservable<CaptureState> CaptureStateChanged => _recorderService.CaptureStateChanged;
        public IObservable<PlaybackState> PlaybackStateChanged => _playbackService.PlaybackStateChanged;
        public IObservable<MinMaxValuesEventArgs> SampleAvailable => _sampleAvailable.AsObservable();
        public IObservable<IReadOnlyList<Recording>> RecordingsChanged => _recordingsChanged.AsObservable();

        public void Pause()
        {
            _playbackService.Pause();
        }

        public void Play()
        {
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
                _playbackService.Stop();
            }
        }

        private void OnRecordingStopped()
        {
            _playbackService.Initialize(_filePathProvider.CurrentRecordingFile);
            _recordingsChanged.OnNext(_recorderService.GetAllRecordings());
        }

        private void OnSampleAvailable(float minValue, float maxValue)
        {
            _sampleAvailable.OnNext(new MinMaxValuesEventArgs(minValue, maxValue));
        }
    }
}
