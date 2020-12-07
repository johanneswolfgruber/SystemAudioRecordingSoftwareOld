// (c) Johannes Wolfgruber, 2020

using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using SystemAudioRecordingSoftware.Core.Model;

namespace SystemAudioRecordingSoftware.Core.Audio
{
    internal class SampleProviderBase
    {
        private readonly Subject<AudioDataDto> _audioDataAvailable;
        private int _count;
        private float _maxValue;
        private float _minValue;

        public SampleProviderBase(WaveFormat waveFormat)
        {
            _audioDataAvailable = new Subject<AudioDataDto>();
            WaveFormat = waveFormat;
            NotificationCount = WaveFormat.SampleRate / 100;
        }

        public int NotificationCount { get; set; }
        public IObservable<AudioDataDto> AudioDataAvailable => _audioDataAvailable.AsObservable();
        public WaveFormat WaveFormat { get; }

        protected void Add(float[] buffer, int offset, int numSamples)
        {
            var audioData = new List<float>();

            for (int n = 0; n < numSamples; n += WaveFormat.Channels)
            {
                var value = buffer[n + offset];

                _maxValue = Math.Max(_maxValue, value);
                _count++;

                if (_count >= NotificationCount && NotificationCount > 0)
                {
                    audioData.Add(_maxValue);
                    Reset();
                }
            }

            _audioDataAvailable
                .OnNext(new AudioDataDto(audioData, numSamples / WaveFormat.Channels, WaveFormat.SampleRate));
        }

        private void Reset()
        {
            _count = 0;
            _minValue = _maxValue = 0;
        }
    }
}