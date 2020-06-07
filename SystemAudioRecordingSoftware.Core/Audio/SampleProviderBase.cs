// (c) Johannes Wolfgruber, 2020
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Text;

namespace SystemAudioRecordingSoftware.Core.Audio
{
    internal class SampleProviderBase
    {
        private int _count;
        private float _maxValue;
        private float _minValue;

        public SampleProviderBase(WaveFormat waveFormat)
        {
            WaveFormat = waveFormat;
            NotificationCount = WaveFormat.SampleRate / 100;
        }

        public event EventHandler<MinMaxValuesEventArgs>? SampleAvailable;

        public int NotificationCount { get; set; }

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
                    SampleAvailable?.Invoke(this, new MinMaxValuesEventArgs(_minValue, _maxValue));
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
