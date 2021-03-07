using AutoMapper;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using SystemAudioRecordingSoftware.Application.Interfaces;
using SystemAudioRecordingSoftware.Domain.Events;
using SystemAudioRecordingSoftware.Infrastructure.Interfaces;

namespace SystemAudioRecordingSoftware.Infrastructure.Services
{
    public class RecordingService : IRecordingService
    {
        private readonly IMapper _mapper;
        private readonly IRecordsRepository _repository;
        private WasapiLoopbackCapture _capture;
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

        public void StartRecording()
        {
            InitializeRecordingEngine();
            _capture.StartRecording();
            CaptureStateChanged?.Invoke(this, new CaptureStateChangedEventArgs(_capture.CaptureState));
        }

        public void StopRecording()
        {
            _capture.StopRecording();
            CaptureStateChanged?.Invoke(this, new CaptureStateChangedEventArgs(_capture.CaptureState));
        }

        private void InitializeRecordingEngine()
        {
            _capture = new WasapiLoopbackCapture();
            var recording = _repository.CreateRecording();
            _writer = new WaveFileWriter(recording.FilePath, _capture.WaveFormat);
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
            if (_writer == null)
            {
                throw new InvalidOperationException("The WaveFileWriter is not set");
            }

            _writer.Dispose();
            _writer = null;

            _capture.DataAvailable -= OnDataAvailable;
            _capture.RecordingStopped -= OnRecordingStopped;
            _capture.Dispose();

            CaptureStateChanged?.Invoke(this, new CaptureStateChangedEventArgs(_capture.CaptureState));
        }
    }
}