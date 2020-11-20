// (c) Johannes Wolfgruber, 2020

using DynamicData;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using SystemAudioRecordingSoftware.Core.Model;

namespace SystemAudioRecordingSoftware.Core.AudioEngine
{
    public interface IAudioEngineService
    {
        bool IsPlaying { get; }
        bool IsRecording { get; }
        IObservable<CaptureState> CaptureStateChanged { get; }
        IObservable<PlaybackState> PlaybackStateChanged { get; }
        IObservable<AudioDataDto> AudioDataAvailable { get; }

        IObservable<IChangeSet<RecordingDto, Guid>> RecordingsChanged();

        void Pause();

        void Play(string filePath);

        void Record();

        void SnipRecording();

        void Save(string filePath);

        void Stop();

        void RemoveRecording(Guid id);
        
        AudioDataDto GetAudioDisplayData(string filePath);
    }
}
