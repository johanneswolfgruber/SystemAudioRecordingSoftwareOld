// (c) Johannes Wolfgruber, 2020

using FluentAssertions;
using Moq;
using NAudio.Wave;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using SystemAudioRecordingSoftware.Core.Audio;

namespace SystemAudioRecordingSoftware.Core.Tests
{
    internal class SampleProviderTests
    {
        [Test]
        public void SampleProvider_Notification_Count_Is_Set_Correctly_In_Constructor()
        {
            var waveFormat = WaveFormat.CreateALawFormat(44100, 2);

            var provider = new Mock<ISampleProvider>();
            provider
                .Setup(x => x.WaveFormat)
                .Returns(waveFormat);

            var sampleProvider = new SampleProvider(provider.Object);

            sampleProvider.NotificationCount.Should().Be(waveFormat.SampleRate / 100);
        }

        [Test]
        public void WaveInSampleProvider_Notification_Count_Is_Set_Correctly_In_Constructor()
        {
            var waveFormat = WaveFormat.CreateALawFormat(44100, 2);

            var sampleProvider = new WaveInSampleProvider(waveFormat);

            sampleProvider.NotificationCount.Should().Be(waveFormat.SampleRate / 100);
        }

        [Test]
        public void SampleProvider_Read_Notifies_With_Correct_Values()
        {
            var waveFormat = WaveFormat.CreateALawFormat(44100, 2);

            var bufferSize = waveFormat.SampleRate * 2 / 100 + 1;

            var provider = new Mock<ISampleProvider>();
            provider
                .Setup(x => x.WaveFormat)
                .Returns(waveFormat);
            provider
                .Setup(x => x.Read(It.IsAny<float[]>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(bufferSize);

            var sampleProvider = new SampleProvider(provider.Object);
            var buffer = CreateFloatBuffer(bufferSize, -10, 10);
            float minValue = 0;
            float maxValue = 0;

            sampleProvider
                .SampleAvailable
                .Subscribe(x =>
                {
                    minValue = x.MinValue;
                    maxValue = x.MaxValue;
                });

            sampleProvider.Read(buffer, 0, bufferSize);

            minValue.Should().Be(-10);
            maxValue.Should().Be(10);
        }

        private static float[] CreateFloatBuffer(int numSamples, float minValue, float maxValue)
        {
            var buffer = new List<float>(numSamples);

            for (int i = 0; i < numSamples; i++)
            {
                buffer.Add(0);
            }

            buffer[0] = minValue;
            buffer[2] = maxValue;

            return buffer.ToArray();
        }
    }
}
