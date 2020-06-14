// (c) Johannes Wolfgruber, 2020
using NAudio.Wave;
using Splat;
using System;
using SystemAudioRecordingSoftware.Core.File;

namespace SystemAudioRecordingSoftware.Core.Audio
{
    public sealed class RecorderService : IRecorderService
    {
        private readonly IFilePathProvider _filePathProvider;
        private WasapiLoopbackCapture _capture;
        private WaveInSampleProvider _sampleProvider;
        private WaveFileWriter? _writer;

        public RecorderService(IFilePathProvider? filePathProvider = null)
        {
            _filePathProvider = filePathProvider ?? Locator.Current.GetService<IFilePathProvider>();

            _capture = new WasapiLoopbackCapture();
            _sampleProvider = new WaveInSampleProvider(_capture.WaveFormat);
        }

        public event EventHandler<CaptureStateChangedEventArgs>? CaptureStateChanged;

        public event EventHandler<StoppedEventArgs>? RecordingStopped;

        public event EventHandler<MinMaxValuesEventArgs>? SampleAvailable;

        public bool IsRecording => _capture.CaptureState != NAudio.CoreAudioApi.CaptureState.Stopped;

        public void StartRecording()
        {
            InitializeRecordingEngine();
            _capture.StartRecording();
            CaptureStateChanged?.Invoke(this, new CaptureStateChangedEventArgs(_capture.CaptureState));
        }

        public void StopRecording()
        {
            _capture.StopRecording();
        }

        private void InitializeRecordingEngine()
        {
            _capture = new WasapiLoopbackCapture();
            _sampleProvider = new WaveInSampleProvider(_capture.WaveFormat);
            _writer = new WaveFileWriter(_filePathProvider.CurrentRecordingFile, _capture.WaveFormat);

            _capture.DataAvailable += OnDataAvailable;
            _capture.RecordingStopped += OnRecordingStopped;
            _sampleProvider.SampleAvailable += OnSampleAvailable;
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

            _writer.Dispose();
            _writer = null;
            _capture.Dispose();

            CaptureStateChanged?.Invoke(this, new CaptureStateChangedEventArgs(_capture.CaptureState));
            RecordingStopped?.Invoke(this, args);
        }

        private void OnSampleAvailable(object? sender, MinMaxValuesEventArgs args)
        {
            SampleAvailable?.Invoke(this, new MinMaxValuesEventArgs(args.MinValue, args.MaxValue));
        }
    }
}
