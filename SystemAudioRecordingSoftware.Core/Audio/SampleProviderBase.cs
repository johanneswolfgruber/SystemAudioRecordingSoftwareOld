// (c) Johannes Wolfgruber, 2020

using NAudio.Wave;
using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace SystemAudioRecordingSoftware.Core.Audio
{
    internal class SampleProviderBase
    {
        private readonly Subject<MinMaxValuesEventArgs> _sampleAvailable;
        private int _count;
        private float _maxValue;
        private float _minValue;

        public SampleProviderBase(WaveFormat waveFormat)
        {
            _sampleAvailable = new Subject<MinMaxValuesEventArgs>();
            WaveFormat = waveFormat;
            NotificationCount = WaveFormat.SampleRate / 100;
        }

        public int NotificationCount { get; set; }
        public IObservable<MinMaxValuesEventArgs> SampleAvailable => _sampleAvailable.AsObservable();
        public WaveFormat WaveFormat { get; }

        protected void Add(float[] buffer, int offset, int numSamples)
        {
            for (int n = 0; n < numSamples; n += WaveFormat.Channels)
            {
                var value = buffer[n + offset];

                _maxValue = Math.Max(_maxValue, value);
                _minValue = Math.Min(_minValue, value);

                _count++;

                if (_count >= NotificationCount && NotificationCount > 0)
                {
                    _sampleAvailable.OnNext(new MinMaxValuesEventArgs(_minValue, _maxValue));
                    Reset();
                }
            }
        }

        private void Reset()
        {
            _count = 0;
            _minValue = _maxValue = 0;
        }
    }
}
