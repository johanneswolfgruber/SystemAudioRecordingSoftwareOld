using System;

namespace SystemAudioRecordingSoftware.Domain.Model
{
    public readonly struct AudioDataPoint
    {
        public AudioDataPoint(TimeSpan timeStamp, float data)
        {
            TimeStamp = timeStamp;
            Data = data;
        }

        public TimeSpan TimeStamp { get; }
        
        public float Data { get; }
    }
}