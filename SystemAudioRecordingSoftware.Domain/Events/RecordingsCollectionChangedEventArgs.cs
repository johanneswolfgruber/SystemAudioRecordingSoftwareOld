using System;
using System.Collections.Generic;
using SystemAudioRecordingSoftware.Domain.Model;

namespace SystemAudioRecordingSoftware.Domain.Events
{
    public class RecordingsCollectionChangedEventArgs : EventArgs
    {
        public RecordingsCollectionChangedEventArgs(IReadOnlyList<RecordingDto> recordings)
        {
            Recordings = recordings;
        }

        public IReadOnlyList<RecordingDto> Recordings { get; }
    }
}