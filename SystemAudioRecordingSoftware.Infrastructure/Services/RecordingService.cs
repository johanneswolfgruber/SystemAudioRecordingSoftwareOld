using AutoMapper;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using SystemAudioRecordingSoftware.Application.Interfaces;
using SystemAudioRecordingSoftware.Domain.Events;
using SystemAudioRecordingSoftware.Domain.Model;
using SystemAudioRecordingSoftware.Domain.Types;

namespace SystemAudioRecordingSoftware.Infrastructure.Services
{
    public class RecordingService : IRecordingService
    {
        private readonly IMapper _mapper;
        private readonly IRecordsRepository _repository;
        private WasapiLoopbackCapture _capture;
        private Recording? _currentRecording;
        private WaveFileWriter? _writer;

        public RecordingService(IRecordsRepository repository, IDisplayDataProvider dataProvider, IMapper mapper)
        {
            _repository = repository;
            DisplayDataProvider = dataProvider;
            _mapper = mapper;
            _capture = new WasapiLoopbackCapture();
        }

        public event EventHandler<EventArgs>? RecordingStarted;

        public event EventHandler<CaptureStateChangedEventArgs>? CaptureStateChanged;

        public event EventHandler<StoppedEventArgs>? RecordingStopped
        {
            add { _capture.RecordingStopped += value; }
            remove { _capture.RecordingStopped -= value; }
        }

        public event EventHandler<WaveInEventArgs>? DataAvailable
        {
            add { _capture.DataAvailable += value; }
            remove { _capture.DataAvailable -= value; }
        }

        public IDisplayDataProvider DisplayDataProvider { get; }
        public bool IsRecording => _capture.CaptureState != CaptureState.Stopped;

        public Result<Guid> StartRecording()
        {
            InitializeRecordingEngine();
            _capture.StartRecording();
            CaptureStateChanged?.Invoke(this, new CaptureStateChangedEventArgs(_capture.CaptureState));

            if (_currentRecording is null)
            {
                return Result.Error<Guid>("No recording created");
            }

            return Result.Success(_currentRecording.Id);
        }

        public Result StopRecording()
        {
            _capture.StopRecording();
            CaptureStateChanged?.Invoke(this, new CaptureStateChangedEventArgs(_capture.CaptureState));
            return Result.Success();
        }

        private void InitializeRecordingEngine()
        {
            _capture = new WasapiLoopbackCapture();
            _currentRecording = _repository.CreateRecording();
            _writer = new WaveFileWriter(_currentRecording.FilePath, _capture.WaveFormat);
            DisplayDataProvider.WaveFormat = _capture.WaveFormat;

            _capture.DataAvailable += OnDataAvailable;
            _capture.RecordingStopped += OnRecordingStopped;
        }

        private void OnDataAvailable(object? sender, WaveInEventArgs args)
        {
            if (_writer == null)
            {
                throw new InvalidOperationException("The WaveFileWriter is not set");
            }

            _writer.Write(args.Buffer, 0, args.BytesRecorded);
            DisplayDataProvider.Add(args.Buffer, 0, args.BytesRecorded);
        }

        private void OnRecordingStopped(object? sender, StoppedEventArgs args)
        {
            if (_writer is null)
            {
                throw new InvalidOperationException("The WaveFileWriter is not set");
            }

            if (_currentRecording is null)
            {
                throw new InvalidOperationException("The Recording is not set");
            }

            _repository.UpdateRecording(_currentRecording with {Length = _writer.TotalTime});

            _writer.Dispose();
            _writer = null;

            _capture.DataAvailable -= OnDataAvailable;
            _capture.RecordingStopped -= OnRecordingStopped;
            _capture.Dispose();

            CaptureStateChanged?.Invoke(this, new CaptureStateChangedEventArgs(_capture.CaptureState));

            var nextStartTimeStamp = _currentRecording.Tracks.Count > 1
                ? _currentRecording.Tracks[1].Start
                : _currentRecording.Length;
            var tracks = _currentRecording.Tracks
                .OrderBy(t => t.Start)
                .ToList();

            var newTracks = new List<Track>();
            for (int i = 0; i < tracks.Count - 1; i++)
            {
                var length = nextStartTimeStamp - tracks[i].Start;
                nextStartTimeStamp = tracks[i + 1].Start;
                newTracks.Add(tracks[i] with {Name = $"Track {i + 1}", Length = length});
            }

            _repository.UpdateRecording(_currentRecording with {Tracks = newTracks});
        }
    }
}