using SystemAudioRecordingSoftware.Domain.Model;

namespace SystemAudioRecordingSoftware.Domain.Events
{
    public class AudioDataAvailableEventArgs
    {
        public AudioDataAvailableEventArgs(AudioDataDto audioData)
        {
            AudioData = audioData;
        }

        public AudioDataDto AudioData { get; }
    }
}