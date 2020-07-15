// (c) Johannes Wolfgruber, 2020

using NAudio.CoreAudioApi;
using NAudio.Wave;
using Splat;
using System;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using SystemAudioRecordingSoftware.Core.Audio;
using SystemAudioRecordingSoftware.Core.File;
using SystemAudioRecordingSoftware.Core.Model;

namespace SystemAudioRecordingSoftware.Core.AudioEngine
{
    public sealed class RecorderService : IRecorderService
    {
        private readonly Subject<CaptureState> _captureStateChanged;
        private readonly IFilePathProvider _filePathProvider;
        private readonly Subject<StoppedEventArgs> _recordingStopped;
        private readonly Subject<MinMaxValuesEventArgs> _sampleAvailable;
        private readonly Subject<Recording> _newRecordingCreated;
        private WasapiLoopbackCapture _capture;
        private WaveInSampleProvider _sampleProvider;
        private WaveFileWriter? _writer;
        private IDisposable? _dataAvailableDisposable;
        private IDisposable? _recordingStoppedDisposable;
        private IDisposable? _sampleAvailableDisposable;

        public RecorderService(IFilePathProvider? filePathProvider = null)
        {
            _filePathProvider = filePathProvider ?? Locator.Current.GetService<IFilePathProvider>();

            _captureStateChanged = new Subject<CaptureState>();
            _sampleAvailable = new Subject<MinMaxValuesEventArgs>();
            _recordingStopped = new Subject<StoppedEventArgs>();
            _newRecordingCreated = new Subject<Recording>();

            _capture = new WasapiLoopbackCapture();
            _sampleProvider = new WaveInSampleProvider(_capture.WaveFormat);
        }

        public bool IsRecording => _capture.CaptureState != CaptureState.Stopped;
        public IObservable<CaptureState> CaptureStateChanged => _captureStateChanged.AsObservable();
        public IObservable<StoppedEventArgs> RecordingStopped => _recordingStopped.AsObservable();
        public IObservable<MinMaxValuesEventArgs> SampleAvailable => _sampleAvailable.AsObservable();
        public IObservable<Recording> NewRecordingCreated => _newRecordingCreated.AsObservable();

        public void StartRecording()
        {
            InitializeRecordingEngine();
            _capture.StartRecording();
            _captureStateChanged.OnNext(_capture.CaptureState);
        }

        public void StopRecording()
        {
            _capture.StopRecording();
            _captureStateChanged.OnNext(_capture.CaptureState);
        }

        private void InitializeRecordingEngine()
        {
            _filePathProvider.CreateUniqueFilePath();
            _capture = new WasapiLoopbackCapture();
            _sampleProvider = new WaveInSampleProvider(_capture.WaveFormat);
            _writer = new WaveFileWriter(_filePathProvider.CurrentRecordingFile, _capture.WaveFormat);

            _dataAvailableDisposable = Observable
                .FromEventPattern<WaveInEventArgs>(_capture, nameof(_capture.DataAvailable))
                .Subscribe(x => OnDataAvailable(x.EventArgs));

            _recordingStoppedDisposable = Observable
                .FromEventPattern<StoppedEventArgs>(_capture, nameof(_capture.RecordingStopped))
                .Subscribe(x => OnRecordingStopped(x.EventArgs));

            _sampleAvailableDisposable = _sampleProvider
                .SampleAvailable
                .Subscribe(x => _sampleAvailable.OnNext(new MinMaxValuesEventArgs(x.MinValue, x.MaxValue)));
        }

        private void OnDataAvailable(WaveInEventArgs args)
        {
            if (_writer == null)
            {
                throw new InvalidOperationException("The WaveFileWriter is not set");
            }

            _writer.Write(args.Buffer, 0, args.BytesRecorded);
            _sampleProvider.Add(args.Buffer, 0, args.BytesRecorded);
        }

        private void OnRecordingStopped(StoppedEventArgs args)
        {
            if (_writer == null)
            {
                throw new InvalidOperationException("The WaveFileWriter is not set");
            }

            var recording = new Recording(Guid.Parse(Path.GetFileNameWithoutExtension(_filePathProvider.CurrentRecordingFile)),
                                          string.Empty,
                                          _filePathProvider.CurrentRecordingFile,
                                          _writer.TotalTime);

            _writer.Dispose();
            _writer = null;
            _capture.Dispose();

            _dataAvailableDisposable?.Dispose();
            _recordingStoppedDisposable?.Dispose();
            _sampleAvailableDisposable?.Dispose();

            _captureStateChanged.OnNext(_capture.CaptureState);
            _recordingStopped.OnNext(args);
            _newRecordingCreated.OnNext(recording);
        }
    }
}
