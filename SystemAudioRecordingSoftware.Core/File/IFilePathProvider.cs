// (c) Johannes Wolfgruber, 2020
using System.IO;

namespace SystemAudioRecordingSoftware.Core.File
{
    public interface IFilePathProvider
    {
        string CurrentRecordingFile { get; }
        string CurrentRecordingFolder { get; }

        public void SetRecordingFile(string fileName);

        public void SetRecordingFolder(string folderPath);
    }
}
