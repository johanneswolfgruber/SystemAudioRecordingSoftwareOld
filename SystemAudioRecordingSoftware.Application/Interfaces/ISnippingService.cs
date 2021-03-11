using System;
using SystemAudioRecordingSoftware.Domain.Types;

namespace SystemAudioRecordingSoftware.Application.Interfaces
{
    public interface ISnippingService
    {
        Result<TimeSpan> SnipCurrentRecording();

        Result SnipRecording(Guid recordingId, TimeSpan timeStamp);
    }
}