﻿// (c) Johannes Wolfgruber, 2020

using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using SystemAudioRecordingSoftware.Core.Audio;
using SystemAudioRecordingSoftware.Core.Model;

namespace SystemAudioRecordingSoftware.Core.AudioEngine
{
    internal sealed class RecorderService : IRecorderService
    {
        private readonly Subject<AudioDataDto> _audioDataAvailable;
        private readonly Subject<CaptureState> _captureStateChanged;
        private readonly Subject<Recording> _newRecordingCreated;
        private readonly Subject<StoppedEventArgs> _recordingStopped;
        private readonly List<TimeSpan> _snipsList;
        private IDisposable? _audioDataAvailableDisposable;
        private WasapiLoopbackCapture _capture;
        private Recording? _currentRecording;
        private WaveInSampleProvider _sampleProvider;
        private WaveFileWriter? _writer;

        public RecorderService()
        {
            _captureStateChanged = new Subject<CaptureState>();
            _audioDataAvailable = new Subject<AudioDataDto>();
            _recordingStopped = new Subject<StoppedEventArgs>();
            _newRecordingCreated = new Subject<Recording>();
            _snipsList = new List<TimeSpan>();

            _capture = new WasapiLoopbackCapture();
            _sampleProvider = new WaveInSampleProvider(_capture.WaveFormat);
        }

        public bool IsRecording => _capture.CaptureState != CaptureState.Stopped;
        public IObservable<CaptureState> CaptureStateChanged => _captureStateChanged.AsObservable();
        public IObservable<StoppedEventArgs> RecordingStopped => _recordingStopped.AsObservable();
        public IObservable<AudioDataDto> RecorderDataAvailable => _audioDataAvailable.AsObservable();
        public IObservable<Recording> NewRecordingCreated => _newRecordingCreated.AsObservable();

        public Guid StartRecording()
        {
            InitializeRecordingEngine();
            _capture.StartRecording();
            _captureStateChanged.OnNext(_capture.CaptureState);

            return _currentRecording!.Id;
        }

        public void StopRecording()
        {
            _capture.StopRecording();
            _captureStateChanged.OnNext(_capture.CaptureState);
        }

        public TimeSpan SnipRecording()
        {
            if (_writer == null)
            {
                throw new InvalidOperationException("The WaveFileWriter is not set");
            }

            var time = _writer.TotalTime;
            _snipsList.Add(time);

            return time;
        }

        public void SnipRecording(Guid? _, TimeSpan timeStamp)
        {
            _snipsList.Add(timeStamp);
        }

        private List<Track> GetTracksFromSnips(Guid recordingId, TimeSpan writerTotalTime)
        {
            var previousSnip = TimeSpan.Zero;
            var tracks = _snipsList
                .OrderBy(x => x)
                .Select((t, i) =>
                {
                    var time = t - previousSnip;
                    previousSnip = t;
                    return new Track(Guid.NewGuid(), recordingId, $"Track {i + 1}", string.Empty, previousSnip, time);
                })
                .ToList();

            tracks.Add(new Track(Guid.NewGuid(), recordingId, $"Track {tracks.Count + 1}", string.Empty, previousSnip,
                writerTotalTime - previousSnip));

            return tracks;
        }

        private void InitializeRecordingEngine()
        {
            _capture = new WasapiLoopbackCapture();

            var id = Guid.NewGuid();
            var path = Path.Combine(Path.GetTempPath(), id + ".wav");
            _currentRecording = new Recording(id, string.Empty, new List<Track>(), TimeSpan.Zero, path);

            _sampleProvider = new WaveInSampleProvider(_capture.WaveFormat);
            _writer = new WaveFileWriter(_currentRecording.FilePath, _capture.WaveFormat);

            _capture.DataAvailable += OnDataAvailable;
            _capture.RecordingStopped += OnRecordingStopped;

            _audioDataAvailableDisposable = _sampleProvider.AudioDataAvailable.Subscribe(_audioDataAvailable.OnNext);
        }

        private void OnDataAvailable(object? sender, WaveInEventArgs args)
        {
            if (_writer == null)
            {
                throw new InvalidOperationException("The WaveFileWriter is not set");
            }

            _writer.Write(args.Buffer, 0, args.BytesRecorded);
            _sampleProvider.Add(args.Buffer, 0, args.BytesRecorded);
        }

        private void OnRecordingStopped(object? sender, StoppedEventArgs args)
        {
            if (_writer == null)
            {
                throw new InvalidOperationException("The WaveFileWriter is not set");
            }

            if (_currentRecording == null)
            {
                throw new ArgumentException("Recording cannot be null");
            }

            var tracks = GetTracksFromSnips(_currentRecording.Id, _writer.TotalTime);

            var recording = _currentRecording with {Tracks = tracks};

            _snipsList.Clear();

            _writer.Dispose();
            _writer = null;

            _capture.DataAvailable -= OnDataAvailable;
            _capture.RecordingStopped -= OnRecordingStopped;
            _capture.Dispose();

            _audioDataAvailableDisposable?.Dispose();

            _captureStateChanged.OnNext(_capture.CaptureState);
            _recordingStopped.OnNext(args);
            _newRecordingCreated.OnNext(recording);
        }
    }
}