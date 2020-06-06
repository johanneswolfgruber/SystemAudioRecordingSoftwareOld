// (c) Johannes Wolfgruber, 2020
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SystemAudioRecordingSoftware.Core.File;

namespace SystemAudioRecordingSoftware.Core.Audio
{
    public sealed class Recorder
    {
        private WasapiLoopbackCapture _capture;
        private IWaveInSampleProvider _sampleProvider;
        private WaveFileWriter? _writer;

        public Recorder(IFilePathProvider filePathManager)
        {
            FilePathManager = filePathManager;

            _capture = new WasapiLoopbackCapture();
            _sampleProvider = new SampleProvider(_capture);
        }

        public event EventHandler<MinMaxValuesEventArgs>? SampleAvailable;

        public IFilePathProvider FilePathManager { get; }

        public async Task StartRecording()
        {
            InitializeRecordingEngine();

            _capture.StartRecording();

            while (_capture.CaptureState != NAudio.CoreAudioApi.CaptureState.Stopped)
            {
                await Task.Delay(500);
            }
        }

        public void StopRecording()
        {
            _capture.StopRecording();
        }

        private void InitializeRecordingEngine()
        {
            _capture = new WasapiLoopbackCapture();
            _sampleProvider = new SampleProvider(_capture);
            _writer = new WaveFileWriter(FilePathManager.CurrentRecordingFile, _capture.WaveFormat);

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
        }

        private void OnSampleAvailable(object? sender, MinMaxValuesEventArgs args)
        {
            SampleAvailable?.Invoke(this, new MinMaxValuesEventArgs(args.MinValue, args.MaxValue));
        }
    }
}
