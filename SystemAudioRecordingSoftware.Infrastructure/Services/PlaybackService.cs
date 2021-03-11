using NAudio.Wave;
using Splat;
using System;
using System.Diagnostics;
using SystemAudioRecordingSoftware.Application.Interfaces;
using SystemAudioRecordingSoftware.Domain.Events;

namespace SystemAudioRecordingSoftware.Infrastructure.Services
{
    public class PlaybackService : IPlaybackService
    {
        private readonly Func<IWavePlayer> _playbackDeviceFactory;
        private IWavePlayer _playbackDevice;
        private AudioFileReader? _reader;

        public PlaybackService(Func<IWavePlayer>? playbackDeviceFactory = null)
        {
            _playbackDeviceFactory = playbackDeviceFactory ?? Locator.Current.GetService<Func<IWavePlayer>>();
            _playbackDevice = _playbackDeviceFactory();
        }

        public bool IsPlaying => _playbackDevice.PlaybackState == PlaybackState.Playing;
        public event EventHandler<EventArgs>? PlaybackStarted;
        public event EventHandler<PlaybackStateChangedEventArgs>? PlaybackStateChanged;

        public event EventHandler<StoppedEventArgs>? PlaybackStopped
        {
            add { _playbackDevice.PlaybackStopped += value; }
            remove { _playbackDevice.PlaybackStopped -= value; }
        }

        public void PausePlayback()
        {
            _playbackDevice.Pause();
            PlaybackStateChanged?.Invoke(this, new PlaybackStateChangedEventArgs(_playbackDevice.PlaybackState));
        }

        public void Play(string filePath)
        {
            StopPlayback();
            CloseFile();
            CreateDevice();
            OpenFile(filePath);
            _playbackDevice.Play();
        }

        public void StopPlayback()
        {
            _playbackDevice.Stop();
            if (_reader == null)
            {
                return;
            }

            _reader.Position = 0;
            PlaybackStateChanged?.Invoke(this, new PlaybackStateChangedEventArgs(_playbackDevice.PlaybackState));
        }

        public void Dispose()
        {
            _playbackDevice.Dispose();
            _reader?.Dispose();
        }

        private void CloseFile()
        {
            _reader?.Dispose();
            _reader = null;
        }

        private void CreateDevice()
        {
            _playbackDevice = _playbackDeviceFactory();

            _playbackDevice.PlaybackStopped += OnPlaybackStopped;

            PlaybackStateChanged?.Invoke(this, new PlaybackStateChangedEventArgs(_playbackDevice.PlaybackState));
        }

        private void OnPlaybackStopped(object? sender, StoppedEventArgs args)
        {
            if (_reader != null)
            {
                _reader.Position = 0;
            }

            PlaybackStateChanged?.Invoke(this, new PlaybackStateChangedEventArgs(_playbackDevice.PlaybackState));
        }

        private void OpenFile(string filePath)
        {
            try
            {
                _reader = new AudioFileReader(filePath);
                _playbackDevice.Init(_reader);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                CloseFile();
            }
        }
    }
}