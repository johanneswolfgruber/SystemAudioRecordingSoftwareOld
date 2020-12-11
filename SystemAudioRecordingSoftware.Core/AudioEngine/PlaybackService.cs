// (c) Johannes Wolfgruber, 2020

using NAudio.Wave;
using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using SystemAudioRecordingSoftware.Core.Audio;
using SystemAudioRecordingSoftware.Core.Model;

namespace SystemAudioRecordingSoftware.Core.AudioEngine
{
    internal class PlaybackService : IPlaybackService
    {
        private readonly Subject<AudioDataDto> _audioDataAvailable;
        private readonly Subject<PlaybackState> _playbackStateChanged;
        private IDisposable? _audioDataAvailableDisposable;
        private WaveStream? _fileStream;
        private IWavePlayer _playbackDevice;
        private IDisposable? _playbackStopped;

        public PlaybackService()
        {
            _playbackDevice = new WaveOut {DesiredLatency = 200};
            _playbackStateChanged = new Subject<PlaybackState>();
            _audioDataAvailable = new Subject<AudioDataDto>();
        }

        public bool IsPlaying => _playbackDevice.PlaybackState == PlaybackState.Playing;
        public IObservable<PlaybackState> PlaybackStateChanged => _playbackStateChanged.AsObservable();
        public IObservable<AudioDataDto> PlaybackDataAvailable => _audioDataAvailable.AsObservable();

        public void Dispose()
        {
            StopPlayback();
            CloseFile();
            _playbackDevice.Dispose();
            _playbackStopped?.Dispose();
            _audioDataAvailableDisposable?.Dispose();
        }

        public void PausePlayback()
        {
            _playbackDevice.Pause();
            _playbackStateChanged.OnNext(_playbackDevice.PlaybackState);
        }

        public void Play()
        {
            if (_fileStream == null || _playbackDevice.PlaybackState == PlaybackState.Playing)
            {
                return;
            }

            if (_fileStream.Position == _fileStream.Length)
            {
                _fileStream.Position = 0;
            }

            _playbackDevice.Play();
            _playbackStateChanged.OnNext(_playbackDevice.PlaybackState);
        }

        public void StopPlayback()
        {
            _playbackDevice.Stop();
            if (_fileStream == null)
            {
                return;
            }

            _fileStream.Position = 0;
            _playbackStateChanged.OnNext(_playbackDevice.PlaybackState);
        }

        public void Initialize(string filePath) // TODO: add safeguard if not initialized
        {
            StopPlayback();
            CloseFile();
            EnsureDeviceCreated();
            OpenFile(filePath);
        }

        private void CloseFile()
        {
            _fileStream?.Dispose();
            _fileStream = null;
        }

        private void CreateDevice()
        {
            _playbackDevice = new WaveOut {DesiredLatency = 200};
            _playbackStopped = Observable
                .FromEventPattern<StoppedEventArgs>(_playbackDevice, nameof(_playbackDevice.PlaybackStopped))
                .Subscribe(_ => OnPlaybackStopped());
            _playbackStateChanged.OnNext(_playbackDevice.PlaybackState);
        }

        private void EnsureDeviceCreated()
        {
            CreateDevice();
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

                _audioDataAvailableDisposable =
                    provider.AudioDataAvailable.Subscribe(x => _audioDataAvailable.OnNext(x));

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