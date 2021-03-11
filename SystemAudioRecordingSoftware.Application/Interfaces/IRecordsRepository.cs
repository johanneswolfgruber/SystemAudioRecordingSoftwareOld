using System;
using System.Collections.Generic;
using SystemAudioRecordingSoftware.Domain.Events;
using SystemAudioRecordingSoftware.Domain.Model;
using SystemAudioRecordingSoftware.Domain.Types;

namespace SystemAudioRecordingSoftware.Application.Interfaces
{
    public interface IRecordsRepository
    {
        Recording CreateRecording();

        IReadOnlyList<Recording> GetAllRecordings();

        Result<Recording> GetById(Guid id);
        event EventHandler<RecordingChangedEventArgs> RecordingChanged;
        event EventHandler<RecordingsCollectionChangedEventArgs> RecordingsCollectionChanged;

        Result RemoveRecording(Guid id);

        Result RemoveTrack(Guid recordingId, Guid trackId);

        Result UpdateRecording(Recording r);
    }
}