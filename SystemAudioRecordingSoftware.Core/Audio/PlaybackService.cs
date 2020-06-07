// (c) Johannes Wolfgruber, 2020
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SystemAudioRecordingSoftware.Core.Audio
{
    public class PlaybackService : IPlaybackService
    {
        private WaveStream? _fileStream;
        private IWavePlayer? _playbackDevice;

        public event EventHandler<MinMaxValuesEventArgs>? SampleAvailable;

        public bool IsPlaying => _playbackDevice?.PlaybackState == PlaybackState.Playing;

        public void Dispose()
        {
            Stop();
            CloseFile();
            _playbackDevice?.Dispose();
            _playbackDevice = null;
        }

        public void Initialize(string fileName)
        {
            Stop();
            CloseFile();
            EnsureDeviceCreated();
            OpenFile(fileName);
        }

        public void Pause()
        {
            _playbackDevice?.Pause();
        }

        public void Play()
        {
            if (_playbackDevice != null && _fileStream != null && _playbackDevice.PlaybackState != PlaybackState.Playing)
            {
                _playbackDevice.Play();
            }
        }

        public void Stop()
        {
            _playbackDevice?.Stop();
            if (_fileStream != null)
            {
                _fileStream.Position = 0;
            }
        }

        private void CloseFile()
        {
            _fileStream?.Dispose();
            _fileStream = null;
        }

        private void CreateDevice()
        {
            _playbackDevice = new WaveOut { DesiredLatency = 200 };
        }

        private void EnsureDeviceCreated()
        {
            if (_playbackDevice == null)
            {
                CreateDevice();
            }
        }

        private void OpenFile(string fileName)
        {
            try
            {
                var inputStream = new AudioFileReader(fileName);
                _fileStream = inputStream;
                var provider = new SampleProvider(inputStream);
                provider.SampleAvailable += (s, a) => SampleAvailable?.Invoke(this, a);
                _playbackDevice.Init(provider);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                CloseFile();
            }
        }
    }
}
