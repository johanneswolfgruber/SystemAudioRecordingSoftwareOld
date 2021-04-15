﻿using NAudio.Wave;
using System;
using System.Collections.Generic;
using SystemAudioRecordingSoftware.Application.Interfaces;
using SystemAudioRecordingSoftware.Domain.Events;
using SystemAudioRecordingSoftware.Domain.Model;

namespace SystemAudioRecordingSoftware.Application.Services
{
    public class DisplayDataProvider : IDisplayDataProvider
    {
        private int _count;
        private float _maxValue;
        private WaveFormat _waveFormat;

        public DisplayDataProvider()
        {
            _waveFormat = new WaveFormat();
            NotificationCount = WaveFormat.SampleRate / 100;
            TotalTime = TimeSpan.Zero;
        }

        public int NotificationCount { get; set; }

        public TimeSpan TotalTime { get; set; }

        public WaveFormat WaveFormat
        {
            get => _waveFormat;
            set
            {
                if (Equals(value, _waveFormat))
                {
                    return;
                }

                _waveFormat = value;
                NotificationCount = _waveFormat.SampleRate / 100;
            }
        }

        public event EventHandler<AudioDataAvailableEventArgs>? DataAvailable;

        public void Add(byte[] buffer, int offset, int count)
        {
            int samplesNeeded = count / 4;
            var wb = new WaveBuffer(buffer);
            var floatBuffer = wb.FloatBuffer;
            var off = offset / 4;

            Add(floatBuffer, off, samplesNeeded);
        }

        private void Add(float[] buffer, int offset, int count)
        {
            var audioData = new List<AudioDataPoint>();
            var frameTime = TimeSpan.FromSeconds(((double)count / WaveFormat.Channels) / WaveFormat.SampleRate);

            for (int n = 0; n < count; n += WaveFormat.Channels)
            {
                var value = buffer[n + offset];
                _maxValue = Math.Max(_maxValue, value);
                _count++;

                if (_count >= NotificationCount && NotificationCount > 0)
                {
                    var timeStamp = 
                        TotalTime + TimeSpan.FromMilliseconds(((double)n / count) * frameTime.TotalMilliseconds);
                    audioData.Add(new AudioDataPoint(timeStamp, _maxValue));
                    Reset();
                }
            }

            TotalTime += frameTime;

            DataAvailable?.Invoke(this, new AudioDataAvailableEventArgs(new AudioDataDto(audioData)));
        }

        private void Reset()
        {
            _count = 0;
            _maxValue = 0;
        }
    }
}