// (c) Johannes Wolfgruber, 2020
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace SystemAudioRecordingSoftware.Core.Audio
{
    internal sealed class WaveInSampleProvider : SampleProviderBase
    {
        public WaveInSampleProvider(WaveFormat waveFormat)
            : base(waveFormat)
        {
        }

        public void Add(byte[] buffer, int offset, int count)
        {
            int samplesNeeded = count / 4;
            var wb = new WaveBuffer(buffer);
            var floatBuffer = wb.FloatBuffer;
            var off = offset / 4;

            Add(floatBuffer, off, samplesNeeded);
        }
    }
}
