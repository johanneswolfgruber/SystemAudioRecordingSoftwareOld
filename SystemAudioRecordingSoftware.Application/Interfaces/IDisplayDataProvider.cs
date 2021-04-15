// unset

using NAudio.Wave;
using System;
using SystemAudioRecordingSoftware.Domain.Events;

namespace SystemAudioRecordingSoftware.Application.Interfaces
{
    public interface IDisplayDataProvider
    {
        int NotificationCount { get; set; }
        
        TimeSpan TotalTime { get; set; }

        WaveFormat WaveFormat { get; set; }

        void Add(byte[] buffer, int offset, int count);

        event EventHandler<AudioDataAvailableEventArgs> DataAvailable;
    }
}