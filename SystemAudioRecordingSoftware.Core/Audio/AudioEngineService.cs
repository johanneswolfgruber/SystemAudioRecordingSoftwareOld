// (c) Johannes Wolfgruber, 2020
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Text;
using SystemAudioRecordingSoftware.Core.File;

namespace SystemAudioRecordingSoftware.Core.Audio
{
    public sealed class AudioEngineService : IAudioEngineService
    {
        private readonly IFilePathProvider _filePathProvider;
        private readonly IPlaybackService _playbackService;
        private readonly IRecorderService _recorderService;

        public AudioEngineService(IFilePathProvider filePathProvider, IRecorderService recorderService, IPlaybackService playbackService)
        {
            _filePathProvider = filePathProvider;
            _recorderService = recorderService;
            _playbackService = playbackService;

            _recorderService.CaptureStateChanged += OnCaptureStateChanged;
            _recorderService.SampleAvailable += OnSampleAvailable;
            _recorderService.RecordingStopped += OnRecordingStopped;
            _playbackService.SampleAvailable += OnSampleAvailable;
        }

        public event EventHandler? CaptureStateChanged;

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

        private void OnCaptureStateChanged(object? sender, EventArgs args)
        {
            CaptureStateChanged?.Invoke(this, args);
        }

        private void OnRecordingStopped(object? sender, StoppedEventArgs e)
        {
            _playbackService.Initialize(_filePathProvider.CurrentRecordingFile);
        }

        private void OnSampleAvailable(object? sender, MinMaxValuesEventArgs args)
        {
            SampleAvailable?.Invoke(this, args);
        }
    }
}
