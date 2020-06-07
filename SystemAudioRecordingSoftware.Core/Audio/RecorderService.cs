// (c) Johannes Wolfgruber, 2020
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SystemAudioRecordingSoftware.Core.File;

namespace SystemAudioRecordingSoftware.Core.Audio
{
    public sealed class RecorderService : IRecorderService
    {
        private WasapiLoopbackCapture _capture;
        private IFilePathProvider _filePathProvider;
        private WaveInSampleProvider _sampleProvider;
        private WaveFileWriter? _writer;

        public RecorderService(IFilePathProvider filePathProvider)
        {
            _filePathProvider = filePathProvider;

            _capture = new WasapiLoopbackCapture();
            _sampleProvider = new WaveInSampleProvider(_capture.WaveFormat);
        }

        public event EventHandler? CaptureStateChanged;

        public event EventHandler<StoppedEventArgs>? RecordingStopped;

        public event EventHandler<MinMaxValuesEventArgs>? SampleAvailable;

        public bool IsRecording => _capture.CaptureState != NAudio.CoreAudioApi.CaptureState.Stopped;

        public void StartRecording()
        {
            InitializeRecordingEngine();
            _capture.StartRecording();
            CaptureStateChanged?.Invoke(this, EventArgs.Empty);
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

            CaptureStateChanged?.Invoke(this, EventArgs.Empty);
            RecordingStopped?.Invoke(this, args);
        }

        private void OnSampleAvailable(object? sender, MinMaxValuesEventArgs args)
        {
            SampleAvailable?.Invoke(this, new MinMaxValuesEventArgs(args.MinValue, args.MaxValue));
        }
    }
}
