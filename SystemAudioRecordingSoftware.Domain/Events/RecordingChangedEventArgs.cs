using System;
using SystemAudioRecordingSoftware.Domain.Model;

namespace SystemAudioRecordingSoftware.Domain.Events
{
    public class RecordingChangedEventArgs : EventArgs
    {
        public RecordingChangedEventArgs(RecordingDto recording)
        {
            Recording = recording;
        }

        public RecordingDto Recording { get; }
    }
}