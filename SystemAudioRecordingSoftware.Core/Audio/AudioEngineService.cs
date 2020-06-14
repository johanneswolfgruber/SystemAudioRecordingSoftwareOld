// (c) Johannes Wolfgruber, 2020
using NAudio.Wave;
using Splat;
using System;
using SystemAudioRecordingSoftware.Core.File;

namespace SystemAudioRecordingSoftware.Core.Audio
{
    public sealed class AudioEngineService : IAudioEngineService
    {
        private readonly IFilePathProvider _filePathProvider;
        private readonly IPlaybackService _playbackService;
        private readonly IRecorderService _recorderService;

        public AudioEngineService(IFilePathProvider? filePathProvider = null,
            IRecorderService? recorderService = null, IPlaybackService? playbackService = null)
        {
            _filePathProvider = filePathProvider ?? Locator.Current.GetService<IFilePathProvider>();
            _recorderService = recorderService ?? Locator.Current.GetService<IRecorderService>();
            _playbackService = playbackService ?? Locator.Current.GetService<IPlaybackService>();

            _recorderService.CaptureStateChanged += OnCaptureStateChanged;
            _recorderService.SampleAvailable += OnSampleAvailable;
            _recorderService.RecordingStopped += OnRecordingStopped;
            _playbackService.SampleAvailable += OnSampleAvailable;
            _playbackService.PlaybackStateChanged += OnPlaybackStateChanged;
        }

        public event EventHandler<CaptureStateChangedEventArgs>? CaptureStateChanged;

        public event EventHandler<PlaybackStateChangedEventArgs>? PlaybackStateChanged;

        public event EventHandler<MinMaxValuesEventArgs>? SampleAvailable;

        public bool IsPlaying => _playbackService.IsPlaying;

        public bool IsRecording => _recorderService.IsRecording;

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

        private void OnCaptureStateChanged(object? sender, CaptureStateChangedEventArgs args)
        {
            CaptureStateChanged?.Invoke(this, args);
        }

        private void OnPlaybackStateChanged(object? sender, PlaybackStateChangedEventArgs args)
        {
            PlaybackStateChanged?.Invoke(this, args);
        }

        private void OnRecordingStopped(object? sender, StoppedEventArgs args)
        {
            _playbackService.Initialize(_filePathProvider.CurrentRecordingFile);
        }

        private void OnSampleAvailable(object? sender, MinMaxValuesEventArgs args)
        {
            SampleAvailable?.Invoke(this, args);
        }
    }
}
