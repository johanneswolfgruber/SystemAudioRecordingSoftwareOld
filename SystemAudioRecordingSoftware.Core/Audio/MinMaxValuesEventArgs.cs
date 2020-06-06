// (c) Johannes Wolfgruber, 2020
namespace SystemAudioRecordingSoftware.Core.Audio
{
    public class MinMaxValuesEventArgs
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
