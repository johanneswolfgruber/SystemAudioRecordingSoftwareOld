// (c) Johannes Wolfgruber, 2020
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Text;

namespace SystemAudioRecordingSoftware.Core.Audio
{
    internal class SampleProvider : SampleProviderBase, ISampleProvider
    {
        private readonly ISampleProvider _source;

        public SampleProvider(ISampleProvider source)
            : base(source.WaveFormat)
        {
            _source = source;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            var samplesRead = _source.Read(buffer, offset, count);

            Add(buffer, offset, samplesRead);

            return samplesRead;
        }
    }
}
