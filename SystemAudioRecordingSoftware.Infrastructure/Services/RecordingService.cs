using AutoMapper;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using SystemAudioRecordingSoftware.Application.Interfaces;
using SystemAudioRecordingSoftware.Domain.Events;
using SystemAudioRecordingSoftware.Domain.Extensions;
using SystemAudioRecordingSoftware.Domain.Types;

namespace SystemAudioRecordingSoftware.Infrastructure.Services
{
    public class RecordingService : IRecordingService
    {
        private readonly IMapper _mapper;
        private readonly IRecordsRepository _repository;
        private WasapiLoopbackCapture _capture;
        private Guid? _currentRecordingId;
        private WaveFileWriter? _writer;
        private TimeSpan _currentTimeStamp;

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

            if (_currentRecordingId is null)
            {
                return Result.Error<Guid>("No recording created");
            }

            return Result.Success(_currentRecordingId.Value);
        }

        public Result StopRecording()
        {
            _capture.StopRecording();
            CaptureStateChanged?.Invoke(this, new CaptureStateChangedEventArgs(_capture.CaptureState));
            return Result.Success();
        }

        public Result<TimeSpan> GetCurrentTimeStamp()
        {
            return IsRecording ? Result.Success(_currentTimeStamp) : Result.Error<TimeSpan>("Not recording");
        }

        public Result<Guid> GetCurrentRecording()
        {
            return _currentRecordingId is null
                ? Result.Error<Guid>("Not recording")
                : Result.Success(_currentRecordingId.Value);
        }

        private void InitializeRecordingEngine()
        {
            _capture = new WasapiLoopbackCapture();
            var recording = _repository.CreateRecording();
            _currentRecordingId = recording.Id;
            _writer = new WaveFileWriter(recording.FilePath, _capture.WaveFormat);
            DisplayDataProvider.WaveFormat = _capture.WaveFormat;
            DisplayDataProvider.TotalTime = TimeSpan.Zero;

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
            _currentTimeStamp = _writer.TotalTime;
            DisplayDataProvider.Add(args.Buffer, 0, args.BytesRecorded);
        }

        private void OnRecordingStopped(object? sender, StoppedEventArgs args)
        {
            if (_writer is null)
            {
                throw new InvalidOperationException("The WaveFileWriter is not set");
            }

            if (_currentRecordingId is null)
            {
                throw new InvalidOperationException("The Recording is not set");
            }

            var recordingResult = _repository.GetById(_currentRecordingId.Value);
            if (recordingResult.Failed)
            {
                throw new InvalidOperationException(recordingResult.ErrorText);
            }

            var recording = recordingResult.Value! with {Length = _writer.TotalTime};

            _writer.Dispose();
            _writer = null;

            _capture.DataAvailable -= OnDataAvailable;
            _capture.RecordingStopped -= OnRecordingStopped;
            _capture.Dispose();

            CaptureStateChanged?.Invoke(this, new CaptureStateChangedEventArgs(_capture.CaptureState));

            _repository.UpdateRecording(recording with {Tracks = recording.UpdateTrackTimeStamps(recording.Length)});
            _currentRecordingId = null;
        }
    }
}