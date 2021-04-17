using System;

namespace SystemAudioRecordingSoftware.Presentation.Controls.Waveform
{
    internal class ReadonlyLineDisplay : LineDisplayBase
    {
        public ReadonlyLineDisplay(Func<TimeSpan, double> timeToX) : base(timeToX)
        {
        }
    }
}