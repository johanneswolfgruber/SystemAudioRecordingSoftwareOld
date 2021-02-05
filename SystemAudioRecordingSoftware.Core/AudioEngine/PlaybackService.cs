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
        private IWavePlayer _playbackDevice;
        private AudioFileReader? _reader;

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
            _audioDataAvailableDisposable?.Dispose();
        }

        public void PausePlayback()
        {
            _playbackDevice.Pause();
            _playbackStateChanged.OnNext(_playbackDevice.PlaybackState);
        }

        public void Play()
        {
            if (_reader == null || _playbackDevice.PlaybackState == PlaybackState.Playing)
            {
                return;
            }

            if (_reader.Position == _reader.Length)
            {
                _reader.Position = 0;
            }

            _playbackDevice.Play();
            _playbackStateChanged.OnNext(_playbackDevice.PlaybackState);
        }

        public void StopPlayback()
        {
            _playbackDevice.Stop();
            if (_reader == null)
            {
                return;
            }

            _reader.Position = 0;
            _playbackStateChanged.OnNext(_playbackDevice.PlaybackState);
        }

        public void Initialize(string filePath) // TODO: add safeguard if not initialized
        {
            StopPlayback();
            CloseFile();
            CreateDevice();
            OpenFile(filePath);
        }

        private void CloseFile()
        {
            _reader?.Dispose();
            _reader = null;
        }

        private void CreateDevice()
        {
            _playbackDevice = new WaveOutEvent {DesiredLatency = 200};

            _playbackDevice.PlaybackStopped += OnPlaybackStopped;

            _playbackStateChanged.OnNext(_playbackDevice.PlaybackState);
        }

        private void OnPlaybackStopped(object? sender, StoppedEventArgs args)
        {
            if (_reader != null)
            {
                _reader.Position = 0;
            }

            _playbackStateChanged.OnNext(_playbackDevice.PlaybackState);
        }

        private void OpenFile(string filePath)
        {
            try
            {
                _reader = new AudioFileReader(filePath);

                var provider = new SampleProvider(_reader);

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