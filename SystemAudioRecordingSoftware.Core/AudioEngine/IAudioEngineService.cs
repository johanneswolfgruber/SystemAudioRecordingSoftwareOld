// (c) Johannes Wolfgruber, 2020

using DynamicData;
using System;
using SystemAudioRecordingSoftware.Core.File;
using SystemAudioRecordingSoftware.Core.Model;

namespace SystemAudioRecordingSoftware.Core.AudioEngine
{
    public interface IAudioEngineService : IRecorderService, IPlaybackService, IAudioFileLoaderService
    {
        IObservable<AudioDataDto> AudioDataAvailable { get; }

        IObservable<IChangeSet<RecordingDto, Guid>> RecordingsChanged();

        void RemoveRecording(Guid id);
    }
}