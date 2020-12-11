// (c) Johannes Wolfgruber, 2020

using System.Collections.Generic;
using SystemAudioRecordingSoftware.Core.Model;

namespace SystemAudioRecordingSoftware.Core.File
{
    public interface IAudioFileLoaderService
    {
        IEnumerable<float> GetAudioData(string filePath);

        AudioDataDto GetAudioDisplayData(string filePath);
    }
}