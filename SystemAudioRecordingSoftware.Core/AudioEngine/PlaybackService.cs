// (c) Johannes Wolfgruber, 2020
using NAudio.Wave;
using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using SystemAudioRecordingSoftware.Core.Audio;

namespace SystemAudioRecordingSoftware.Core.AudioEngine
{
    public class PlaybackService : IPlaybackService
    {
        private readonly Subject<PlaybackState> _playbackStateChanged;
        private readonly Subject<MinMaxValuesEventArgs> _sampleAvailable;
        private WaveStream? _fileStream;
        private IWavePlayer _playbackDevice;
        private IDisposable? _playbackStopped;

        public PlaybackService()
        {
            _playbackDevice = new WaveOut { DesiredLatency = 200 };
            _playbackStateChanged = new Subject<PlaybackState>();
            _sampleAvailable = new Subject<MinMaxValuesEventArgs>();
        }

        public bool IsPlaying => _playbackDevice?.PlaybackState == PlaybackState.Playing;
        public IObservable<PlaybackState> PlaybackStateChanged => _playbackStateChanged.AsObservable();
        public IObservable<MinMaxValuesEventArgs> SampleAvailable => _sampleAvailable.AsObservable();

        public void Dispose()
        {
            Stop();
            CloseFile();
            _playbackDevice.Dispose();
            _playbackStopped?.Dispose();
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
            _playbackDevice.Pause();
            _playbackStateChanged.OnNext(_playbackDevice.PlaybackState);
        }

        public void Play()
        {
            if (_fileStream != null &&
                _playbackDevice.PlaybackState != PlaybackState.Playing)
            {
                if (_fileStream.Position == _fileStream.Length)
                {
                    _fileStream.Position = 0;
                }

                _playbackDevice.Play();
                _playbackStateChanged.OnNext(_playbackDevice.PlaybackState);
            }
        }

        public void Stop()
        {
            _playbackDevice.Stop();
            if (_fileStream != null)
            {
                _fileStream.Position = 0;
                _playbackStateChanged.OnNext(_playbackDevice.PlaybackState);
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
            _playbackStopped = Observable
                .FromEventPattern<StoppedEventArgs>(_playbackDevice, nameof(_playbackDevice.PlaybackStopped))
                .Subscribe(_ => OnPlaybackStopped());
            _playbackStateChanged.OnNext(_playbackDevice.PlaybackState);
        }

        private void EnsureDeviceCreated()
        {
            if (_playbackDevice == null)
            {
                CreateDevice();
            }
        }

        private void OnPlaybackStopped()
        {
            _playbackStateChanged.OnNext(_playbackDevice.PlaybackState);
        }

        private void OpenFile(string fileName)
        {
            try
            {
                var inputStream = new AudioFileReader(fileName);
                _fileStream = inputStream;
                var provider = new SampleProvider(inputStream);

                provider
                    .SampleAvailable
                    .Subscribe(x => _sampleAvailable.OnNext(new MinMaxValuesEventArgs(x.MinValue, x.MaxValue)));

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
