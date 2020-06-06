// (c) Johannes Wolfgruber, 2020
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Text;

namespace SystemAudioRecordingSoftware.Core.Audio
{
    internal sealed class SampleProvider : IWaveInSampleProvider
    {
        private readonly IWaveIn _source;
        private int _count;
        private float _maxValue;
        private float _minValue;

        public SampleProvider(IWaveIn source)
        {
            _source = source;
            NotificationCount = WaveFormat.SampleRate / 100;
        }

        public event EventHandler<MinMaxValuesEventArgs>? SampleAvailable;

        public int NotificationCount { get; set; }
        public WaveFormat WaveFormat => _source.WaveFormat;

        public void Add(byte[] buffer, int offset, int count)
        {
            int samplesNeeded = count / 4;
            var wb = new WaveBuffer(buffer);
            var floatBuffer = wb.FloatBuffer;
            var off = offset / 4;

            for (int n = 0; n < samplesNeeded; n += WaveFormat.Channels)
            {
                var value = floatBuffer[n + off];

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
