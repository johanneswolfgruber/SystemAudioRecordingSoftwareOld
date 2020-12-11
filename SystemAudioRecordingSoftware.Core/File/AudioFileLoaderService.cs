// (c) Johannes Wolfgruber, 2020

using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using SystemAudioRecordingSoftware.Core.Model;

namespace SystemAudioRecordingSoftware.Core.File
{
    internal class AudioFileLoaderService : IAudioFileLoaderService
    {
        public IEnumerable<float> GetAudioData(string filePath)
        {
            throw new NotImplementedException();
        }

        public AudioDataDto GetAudioDisplayData(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
                throw new InvalidOperationException($"The file {filePath} does not exist.");
            }

            using var reader = new AudioFileReader(filePath);
            float[] buffer = new float[reader.Length / 2];
            reader.Read(buffer, 0, buffer.Length);
            var audioData = new float[buffer.Length / 2];
            Array.Copy(buffer, 0, audioData, 0, buffer.Length / 2);
            var numberOfChannels = reader.WaveFormat.Channels;
            var numberOfSamples = audioData.Length / numberOfChannels;
            var sampleRate = reader.WaveFormat.SampleRate;
            var data = audioData.Where((_, i) => i % (sampleRate / 100 * numberOfChannels) == 0);
            var totalTime = TimeSpan.FromSeconds((double)numberOfSamples / sampleRate);

            return new AudioDataDto(data, totalTime);
        }
    }
}