using System;
using SystemAudioRecordingSoftware.Presentation.Controls.Shapes;

namespace SystemAudioRecordingSoftware.Presentation.Controls.Waveform
{
    internal class MarkerLine
    {
        public MarkerLine()
        {
            Line = new Line();
            TimeStamp = TimeSpan.Zero;
        }
        
        public MarkerLine(Line line, TimeSpan timeStamp)
        {
            Line = line;
            TimeStamp = timeStamp;
        }

        public TimeSpan TimeStamp { get; set; }
        public bool IsSelected { get; set; }
        public Line Line { get; set; }
    }
}