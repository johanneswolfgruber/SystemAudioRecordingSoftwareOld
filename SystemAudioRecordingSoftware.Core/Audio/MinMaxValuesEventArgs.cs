// (c) Johannes Wolfgruber, 2020
using System;

namespace SystemAudioRecordingSoftware.Core.Audio
{
    public class MinMaxValuesEventArgs : EventArgs
    {
        public MinMaxValuesEventArgs(float minValue, float maxValue)
        {
            MaxValue = maxValue;
            MinValue = minValue;
        }

        public float MaxValue { get; private set; }
        public float MinValue { get; private set; }
    }
}
