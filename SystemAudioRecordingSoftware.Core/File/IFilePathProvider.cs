// (c) Johannes Wolfgruber, 2020

namespace SystemAudioRecordingSoftware.Core.File
{
    public interface IFilePathProvider
    {
        string CurrentRecordingFile { get; }
        string CurrentRecordingFolder { get; }

        void Save(string filePath);

        void SetRecordingFile(string fileName);

        void SetRecordingFolder(string folderPath);

        void CreateUniqueFilePath();
    }
}
