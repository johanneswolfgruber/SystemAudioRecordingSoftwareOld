// (c) Johannes Wolfgruber, 2020
using System;
using System.IO;

namespace SystemAudioRecordingSoftware.Core.File
{
    public sealed class FilePathProvider : IFilePathProvider
    {
        private const string _defaultFileName = "default";
        private const string _folderName = "SystemAudioRecordingSoftware";

        public FilePathProvider()
        {
            SetRecordingFile(_defaultFileName);
            SetRecordingFolder(Path.Combine(Path.GetTempPath(), _folderName));
        }

        public string CurrentRecordingFile { get; private set; } = string.Empty;
        public string CurrentRecordingFolder { get; private set; } = string.Empty;

        public void Save(string filePath)
        {
            System.IO.File.Copy(CurrentRecordingFile, filePath, true);
        }

        public void SetRecordingFile(string fileName)
        {
            var name = Path.ChangeExtension(fileName, ".wav");
            CurrentRecordingFile = Path.Combine(CurrentRecordingFolder, name);
        }

        public void SetRecordingFolder(string folderPath)
        {
            CurrentRecordingFolder = folderPath;
            CurrentRecordingFile = Path.Combine(CurrentRecordingFolder, Path.GetFileName(CurrentRecordingFile));
            Directory.CreateDirectory(CurrentRecordingFolder);
        }

        public void CreateUniqueFilePath()
        {
            var guid = Guid.NewGuid();
            SetRecordingFile(guid.ToString());
        }
    }
}
